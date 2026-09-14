// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Linq;
using System.Management.Automation.Runspaces;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides threadsafe caching around CommandInfo lookups with `Get-Command -Name ...`.
    /// </summary>
    internal class CommandInfoCache : IDisposable
    {
        /// <summary>
        /// Number of times a command lookup is attempted before giving up.
        /// Command lookups can fail transiently because the PowerShell engine is not thread safe,
        /// see https://github.com/PowerShell/PowerShell/issues/4003
        /// </summary>
        private const int MaxLookupAttempts = 3;
        private const string GetCommandName = "Microsoft.PowerShell.Core\\Get-Command";

        private readonly ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>> _commandInfoCache;

        /// <summary>
        /// Guards all access to <see cref="_runspace"/> so that only one thread at a time drives the
        /// PowerShell engine. The engine is not thread safe, so concurrent lookups can fail transiently,
        /// see https://github.com/PowerShell/PowerShell/issues/4003.
        /// A monitor is used rather than a semaphore because it is re-entrant, which avoids a deadlock
        /// should a lookup ever end up calling back into the cache on the same thread.
        /// </summary>
        private readonly object _runspaceLock = new object();

        private readonly Runspace _runspace;
        private bool disposed = false;

        /// <summary>
        /// Create a fresh command info cache instance.
        /// </summary>
        public CommandInfoCache()
        {
            _commandInfoCache = new ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>>();
            // A single runspace rather than a pool: all lookups are serialized on it, so that the
            // PowerShell engine is never driven concurrently.
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
        }

        /// <summary>Dispose the runspace</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Always take the lock, also on the finalizer path, so that 'disposed' is never
            // published without the runspace being disposed along with it and so that the runspace
            // cannot be disposed while a lookup is in flight.
            lock (_runspaceLock)
            {
                if ( disposed )
                {
                    return;
                }

                disposed = true;

                if ( disposing )
                {
                    _runspace.Dispose();
                }
            }
        }

        /// <summary>
        /// Retrieve a command info object about a command.
        /// </summary>
        /// <param name="commandName">Name of the command to get a commandinfo object for.</param>
        /// <param name="commandTypes">What types of command are needed. If omitted, all types are retrieved.</param>
        /// <param name="bypassCache">When needed due to runspace affinity problems of some PowerShell objects.</param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string commandName, CommandTypes? commandTypes = null, bool bypassCache = false)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            var key = new CommandLookupKey(commandName, commandTypes);
            if (bypassCache)
            {
                return GetCommandInfoInternal(commandName, commandTypes);
            }
            // Atomically either use PowerShell to query a command info object, or fetch it from the cache
            var lazyCommandInfo = _commandInfoCache.GetOrAdd(key, new Lazy<CommandInfo>(() => GetCommandInfoInternal(commandName, commandTypes)));
            try
            {
                return lazyCommandInfo.Value;
            }
            catch
            {
                // Lazy<T> caches exceptions forever, which would make every subsequent lookup of this
                // command fail for the lifetime of the process. Evict the entry so that the next lookup
                // can try again. Only remove the faulted instance so that a replacement that another
                // thread may already have added is left alone.
                ((ICollection<KeyValuePair<CommandLookupKey, Lazy<CommandInfo>>>)_commandInfoCache)
                    .Remove(new KeyValuePair<CommandLookupKey, Lazy<CommandInfo>>(key, lazyCommandInfo));
                throw;
            }
        }


        /// <summary>
        /// Get a CommandInfo object of the given command name
        /// </summary>
        /// <returns>Returns null if command does not exists</returns>
        private CommandInfo GetCommandInfoInternal(string cmdName, CommandTypes? commandType)
        {
            string moduleName = null;
            string actualCmdName = cmdName;

            // Check if cmdName is in the format "moduleName\CmdletName" (exactly one backslash)
            int backslashIndex = cmdName.IndexOf('\\');
            if (
                backslashIndex > 0 &&
                backslashIndex == cmdName.LastIndexOf('\\') &&
                backslashIndex != cmdName.Length - 1 &&
                backslashIndex != 0
            )
            {
                moduleName = cmdName.Substring(0, backslashIndex);
                actualCmdName = cmdName.Substring(backslashIndex + 1);
            }
            // 'Get-Command ?' would return % for example due to PowerShell interpreting is a single-character-wildcard search and not just the ? alias.
            // For more details see https://github.com/PowerShell/PowerShell/issues/9308
            actualCmdName = WildcardPattern.Escape(actualCmdName);

            for (int attempt = 1; ; attempt++)
            {
                // Serialize all use of the PowerShell engine. Only cache misses reach this point;
                // lookups that are already cached are served without taking the lock.
                lock (_runspaceLock)
                {
                    if (disposed)
                    {
                        return null;
                    }

                    using (var ps = System.Management.Automation.PowerShell.Create())
                    {
                        ps.Runspace = _runspace;

                        ps.AddCommand(GetCommandName)
                            .AddParameter("Name", actualCmdName)
                            .AddParameter("ErrorAction", "SilentlyContinue");

                        if (commandType != null)
                        {
                            ps.AddParameter("CommandType", commandType);
                        }

                        if (!string.IsNullOrEmpty(moduleName))
                        {
                            ps.AddParameter("Module", moduleName);
                        }

                        try
                        {
                            var result = ps.Invoke<CommandInfo>();
                            if (ps.HadErrors && ps.Streams.Error.All(IsGetCommandResolutionError))
                            {
                                if (attempt >= MaxLookupAttempts)
                                {
                                    return null;
                                }

                                continue;
                            }

                            return result.FirstOrDefault();
                        }
                        // 'Get-Command' is invoked with 'SilentlyContinue', so a CommandNotFoundException can only
                        // mean that the engine failed to resolve 'Get-Command' itself in the runspace.
                        // That happened intermittently when lookups ran concurrently because the PowerShell engine
                        // is not thread safe, see https://github.com/PowerShell/PowerShell/issues/4003 and
                        // https://github.com/PowerShell/PSScriptAnalyzer/issues/2205
                        // Lookups are serialized now, so this should no longer occur, but the retry is kept as a
                        // safety net for hosts that drive the engine from other threads at the same time.
                        catch (RuntimeException exception) when (IsGetCommandResolutionException(exception))
                        {
                            if (attempt >= MaxLookupAttempts)
                            {
                                return null;
                            }
                        }

                    }
                }
            }
        }

        private static bool IsGetCommandResolutionError(ErrorRecord errorRecord)
        {
            return IsGetCommandResolutionException(errorRecord?.Exception);
        }

        private static bool IsGetCommandResolutionException(Exception exception)
        {
            if (exception is CommandNotFoundException)
            {
                return true;
            }

            if (exception is ParentContainsErrorRecordException parentContainsErrorRecordException)
            {
                return IsGetCommandResolutionException(parentContainsErrorRecordException.InnerException);
            }

            return false;
        }

        private struct CommandLookupKey : IEquatable<CommandLookupKey>
        {
            private readonly string Name;

            private readonly CommandTypes CommandTypes;

            internal CommandLookupKey(string name, CommandTypes? commandTypes)
            {
                Name = name;
                CommandTypes = commandTypes ?? CommandTypes.All;
            }

            public bool Equals(CommandLookupKey other)
            {
                return CommandTypes == other.CommandTypes
                    && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
            }

            public override int GetHashCode()
            {
                // Algorithm from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Name.ToUpperInvariant().GetHashCode();
                    hash = hash * 31 + CommandTypes.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
