// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides threadsafe caching around CommandInfo lookups with `Get-Command -Name ...`.
    /// </summary>
    internal class CommandInfoCache : IDisposable
    {
        private const string GetCommandName = "Microsoft.PowerShell.Core\\Get-Command";

        private readonly ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>> _commandInfoCache;
        private readonly ConcurrentDictionary<CmdletInfo, IReadOnlyDictionary<string, CommandParameterSnapshot>> _parameterSnapshots
            = new ConcurrentDictionary<CmdletInfo, IReadOnlyDictionary<string, CommandParameterSnapshot>>();
        private readonly ConcurrentDictionary<CmdletInfo, IReadOnlyList<string>> _mandatoryParameters
            = new ConcurrentDictionary<CmdletInfo, IReadOnlyList<string>>();

        /// <summary>
        /// Guards all access to <see cref="_runspace"/> so that only one thread at a time drives the
        /// PowerShell engine. The engine is not thread safe, so concurrent lookups can fail transiently,
        /// see https://github.com/PowerShell/PowerShell/issues/4003.
        /// A monitor is used rather than a semaphore because it is re-entrant, which avoids a deadlock
        /// should a lookup ever end up calling back into the cache on the same thread.
        /// </summary>
        private readonly object _runspaceLock = new object();

        private readonly Runspace _runspace;
        private volatile bool disposed = false;

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
        /// <param name="callSite">
        /// The Ast of the command being resolved, used to check whether a same-named local function
        /// definition is actually in lexical scope at this call site rather than merely present in the file.
        /// </param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string commandName, CommandTypes? commandTypes = null, bool bypassCache = false, Ast callSite = null)
        {
            try
            {
                return GetCachedCommandInfo(commandName, commandTypes, bypassCache, callSite);
            }
            catch (Exception exception) when (IsGetCommandResolutionException(exception))
            {
                // Failed Lazy lookups have already been evicted; never cache a lookup failure as a miss.
                return null;
            }
        }

        private CommandInfo GetCachedCommandInfo(string commandName, CommandTypes? commandTypes, bool bypassCache, Ast callSite = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            // A name the analyzed script defines itself is not the command of the same name that happens
            // to be installed here; resolving it would validate the call against the wrong definition.
            if (LocalFunctionScope.Current?.IsDefinedInScript(commandName, callSite) == true)
            {
                return null;
            }

            var key = new CommandLookupKey(commandName, commandTypes);
            if (bypassCache)
            {
                return GetCommandInfoInternal(commandName, commandTypes);
            }
            // Atomically either use PowerShell to query a command info object, or fetch it from the cache
            if (!_commandInfoCache.TryGetValue(key, out var lazyCommandInfo))
            {
                lazyCommandInfo = _commandInfoCache.GetOrAdd(key, CreateLookup(commandName, commandTypes));
            }
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
                RemoveLookup(key, lazyCommandInfo);
                throw;
            }
        }

        private void RemoveLookup(CommandLookupKey key, Lazy<CommandInfo> lookup)
        {
            ((ICollection<KeyValuePair<CommandLookupKey, Lazy<CommandInfo>>>)_commandInfoCache)
                .Remove(new KeyValuePair<CommandLookupKey, Lazy<CommandInfo>>(key, lookup));
        }

        private Lazy<CommandInfo> CreateLookup(string commandName, CommandTypes? commandTypes)
        {
            return new Lazy<CommandInfo>(() => GetCommandInfoInternal(commandName, commandTypes));
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

                    Collection<CommandInfo> result = ps.Invoke<CommandInfo>();

                    // 'Get-Command' is invoked with 'SilentlyContinue', so a resolution error can only
                    // mean that the engine failed to resolve 'Get-Command' itself in the runspace.
                    // That happened intermittently when lookups ran concurrently because the PowerShell engine
                    // is not thread safe, see https://github.com/PowerShell/PowerShell/issues/4003 and
                    // https://github.com/PowerShell/PSScriptAnalyzer/issues/2205
                    // Lookups are serialized now, so this should no longer occur; if it does, the cache
                    // entry is evicted and the lookup surfaces as a null command info.
                    // SilentlyContinue can set HadErrors without populating the stream for an unknown name.
                    // Any error that is not the expected resolution failure is unexpected (an engine or
                    // module failure) and must propagate rather than be silently negative-cached as a
                    // missing command.
                    if (ps.HadErrors && ps.Streams.Error.Count > 0)
                    {
                        ErrorRecord unexpected = ps.Streams.Error.FirstOrDefault(error => !IsGetCommandResolutionError(error));
                        if (unexpected != null)
                        {
                            throw unexpected.Exception;
                        }

                        throw ps.Streams.Error[0].Exception;
                    }
                    return result.FirstOrDefault();
                }
            }
        }

        private static bool IsGetCommandResolutionError(ErrorRecord errorRecord)
        {
            return IsGetCommandResolutionException(errorRecord?.Exception);
        }

        /// <summary>
        /// Retrieves parameter metadata without allowing other threads to drive the command's runspace.
        /// </summary>
        public Dictionary<string, ParameterMetadata> GetCommandParameters(
            string commandName, CommandTypes? commandTypes = null, bool bypassCache = false, Ast callSite = null)
        {
            return GetCommandMetadata(commandName, commandTypes, bypassCache, callSite, command =>
            {
                lock (_runspaceLock)
                {
                    // Dynamic parameter getters execute PowerShell code and mutate runspace state,
                    // even though they look like ordinary property reads.
                    if (disposed) return null;
                    return command.Parameters;
                }
            });
        }

        /// <summary>
        /// Retrieves parameter sets under the same lock as command lookups and dynamic parameter queries.
        /// </summary>
        public ReadOnlyCollection<CommandParameterSetInfo> GetCommandParameterSets(string commandName, Ast callSite = null)
        {
            return GetCommandMetadata(commandName, null, false, callSite, command =>
            {
                lock (_runspaceLock)
                {
                    if (disposed) return null;
                    return command.ParameterSets;
                }
            });
        }

        /// <summary>
        /// Contains PowerShell metadata failures: the failed command object is evicted so subsequent
        /// calls can recover, and null is returned. Unavailable metadata is never cached.
        /// Unexpected exceptions still propagate.
        /// </summary>
        private T GetCommandMetadata<T>(
            string commandName, CommandTypes? commandTypes, bool bypassCache, Ast callSite, Func<CommandInfo, T> readMetadata)
            where T : class
        {
            // Resolve Lazy values outside the runspace lock: another thread's factory may need it.
            var command = GetCommandInfo(commandName, commandTypes, bypassCache, callSite);
            if (disposed || command == null || command.CommandType == CommandTypes.Application) return null;
            try
            {
                return readMetadata(command);
            }
            catch (Exception exception) when (IsMetadataException(exception))
            {
                var key = new CommandLookupKey(commandName, commandTypes);
                if (_commandInfoCache.TryGetValue(key, out var lookup)
                    && lookup.IsValueCreated && ReferenceEquals(lookup.Value, command))
                {
                    RemoveLookup(key, lookup);
                }
                return null;
            }
        }

        private static bool IsRunspaceAffinityException(Exception exception)
        {
            // PowerShell objects can have runspace affinity, see PowerShell issue 4003 and PSSA issue 1708.
            return exception is InvalidOperationException || exception is NullReferenceException;
        }

        private static bool IsMetadataException(Exception exception)
        {
            return IsRunspaceAffinityException(exception)
                || exception is RuntimeException || exception is PSNotSupportedException;
        }

        private static CmdletInfo GetStaticCmdlet(CommandInfo command)
        {
            // Aliases, functions, subclasses and provider/dynamic cmdlets retain the locked,
            // uncached path. IDynamicParameters includes implementations inherited from base types.
            if (command == null || command.GetType() != typeof(CmdletInfo)) return null;
            var cmdlet = (CmdletInfo)command;
            return cmdlet.ImplementingType != null
                && !typeof(IDynamicParameters).IsAssignableFrom(cmdlet.ImplementingType) ? cmdlet : null;
        }

        public IReadOnlyDictionary<string, CommandParameterSnapshot> GetParameterSnapshot(
            string commandName, CommandTypes? commandTypes = null, bool bypassCache = false, Ast callSite = null)
        {
            return GetCommandMetadata(commandName, commandTypes, bypassCache, callSite,
                command => GetParameterSnapshot(command, bypassCache));
        }

        private IReadOnlyDictionary<string, CommandParameterSnapshot> GetParameterSnapshot(CommandInfo command, bool bypassCache)
        {
            var staticCmdlet = bypassCache ? null : GetStaticCmdlet(command);
            if (disposed || command == null) return null;
            if (staticCmdlet != null && _parameterSnapshots.TryGetValue(staticCmdlet, out var cached)) return cached;

            lock (_runspaceLock)
            {
                if (disposed) return null;
                if (staticCmdlet != null && _parameterSnapshots.TryGetValue(staticCmdlet, out cached)) return cached;
                var parameters = command.Parameters;
                if (parameters == null) return null;
                var snapshot = new ReadOnlyDictionary<string, CommandParameterSnapshot>(
                    parameters.ToDictionary(p => p.Key, p => new CommandParameterSnapshot(p.Value), parameters.Comparer));
                if (staticCmdlet != null) _parameterSnapshots[staticCmdlet] = snapshot;
                return snapshot;
            }
        }

        public IReadOnlyList<string> GetMandatoryParameterNames(string commandName, Ast callSite = null)
        {
            return GetCommandMetadata(commandName, null, false, callSite,
                command => GetMandatoryParameterNames(command, bypassCache: false));
        }

        private IReadOnlyList<string> GetMandatoryParameterNames(CommandInfo command, bool bypassCache)
        {
            var staticCmdlet = bypassCache ? null : GetStaticCmdlet(command);
            if (disposed || command == null) return null;
            if (staticCmdlet != null && _mandatoryParameters.TryGetValue(staticCmdlet, out var cached)) return cached;

            lock (_runspaceLock)
            {
                if (disposed) return null;
                if (staticCmdlet != null && _mandatoryParameters.TryGetValue(staticCmdlet, out cached)) return cached;
                var parameterSets = command.ParameterSets;
                var parameters = command.Parameters;
                if (parameterSets == null || parameters == null) return null;
                int setCount = parameterSets.Count;
                var mandatory = new List<string>();
                foreach (var parameter in parameters.Values)
                {
                    if (parameter.Attributes.Count >= setCount
                        && parameter.Attributes.OfType<ParameterAttribute>().Count(a => a.Mandatory) >= setCount)
                    {
                        mandatory.Add(parameter.Name);
                    }
                }
                var snapshot = mandatory.AsReadOnly();
                if (staticCmdlet != null) _mandatoryParameters[staticCmdlet] = snapshot;
                return snapshot;
            }
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
    }
}
