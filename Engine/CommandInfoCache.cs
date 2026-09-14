// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
#if !DISABLE_ENGINE_RETRIES
        /// <summary>
        /// Number of times a command lookup is attempted before giving up.
        /// Command lookups can fail transiently because the PowerShell engine is not thread safe,
        /// see https://github.com/PowerShell/PowerShell/issues/4003
        /// </summary>
        private const int MaxLookupAttempts = 3;
#endif
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
            using (PerformanceTelemetry.EnterLock(_runspaceLock))
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
                PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupBypasses);
                return GetCommandInfoInternal(commandName, commandTypes);
            }
            // Atomically either use PowerShell to query a command info object, or fetch it from the cache
            if (!_commandInfoCache.TryGetValue(key, out var lazyCommandInfo))
            {
                lazyCommandInfo = _commandInfoCache.GetOrAdd(key, CreateLookup(commandName, commandTypes));
            }
            try
            {
                var commandInfo = lazyCommandInfo.Value;
                if (commandInfo == null)
                {
                    RemoveLookup(key, lazyCommandInfo);
                }
                return commandInfo;
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
            return new Lazy<CommandInfo>(() =>
            {
                PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupMisses);
                return GetCommandInfoInternal(commandName, commandTypes);
            });
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

#if !DISABLE_ENGINE_RETRIES
            for (int attempt = 1; ; attempt++)
#endif
            {
                // Serialize all use of the PowerShell engine. Only cache misses reach this point;
                // lookups that are already cached are served without taking the lock.
                using (PerformanceTelemetry.EnterLock(_runspaceLock))
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

                        Collection<CommandInfo> result;
                        try
                        {
                            result = ps.Invoke<CommandInfo>();
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
                            PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupResolutionFailures);
#if DISABLE_ENGINE_RETRIES
                            throw;
#else
                            if (attempt >= MaxLookupAttempts)
                            {
                                return null;
                            }
                            PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupRetries);
                            continue;
#endif
                        }

                        // SilentlyContinue can set HadErrors without populating the stream for an unknown name.
                        if (ps.HadErrors && ps.Streams.Error.Count > 0 && ps.Streams.Error.All(IsGetCommandResolutionError))
                        {
                            PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupResolutionFailures);
#if DISABLE_ENGINE_RETRIES
                            // Surface error-stream failures as well as terminating exceptions in verification builds.
                            throw ps.Streams.Error[0].Exception;
#else
                            if (attempt >= MaxLookupAttempts)
                            {
                                return null;
                            }
                            PerformanceTelemetry.Increment(ref PerformanceTelemetry.LookupRetries);
                            continue;
#endif
                        }
                        return result.FirstOrDefault();
                    }
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
            string commandName, CommandTypes? commandTypes = null, bool bypassCache = false)
        {
            return GetCommandMetadata(commandName, commandTypes, bypassCache, (command, bypass) =>
            {
                using (PerformanceTelemetry.EnterLock(_runspaceLock))
                {
                    // Dynamic parameter getters execute PowerShell code and mutate runspace state,
                    // even though they look like ordinary property reads.
                    if (disposed) return null;
                    PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataQueries);
                    return command.Parameters;
                }
            });
        }

        /// <summary>
        /// Retrieves parameter sets under the same lock as command lookups and dynamic parameter queries.
        /// </summary>
        public ReadOnlyCollection<CommandParameterSetInfo> GetCommandParameterSets(string commandName)
        {
            return GetCommandMetadata(commandName, null, false, (command, bypass) =>
            {
                using (PerformanceTelemetry.EnterLock(_runspaceLock))
                {
                    if (disposed) return null;
                    PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataQueries);
                    return command.ParameterSets;
                }
            });
        }

        /// <summary>
        /// Contains PowerShell metadata failures in normal builds and exposes them in verification builds.
        /// Only affinity failures warrant one fresh command lookup; other metadata errors return null.
        /// Failed command objects are evicted, and unavailable metadata is never cached.
        /// </summary>
        private T GetCommandMetadata<T>(
            string commandName, CommandTypes? commandTypes, bool bypassCache, Func<CommandInfo, bool, T> readMetadata)
            where T : class
        {
#if !DISABLE_ENGINE_RETRIES
            while (true)
#endif
            {
                // Resolve Lazy values outside the runspace lock: another thread's factory may need it.
                var command = GetCommandInfo(commandName, commandTypes, bypassCache);
                if (disposed || command == null || command.CommandType == CommandTypes.Application) return null;
                try
                {
                    return readMetadata(command, bypassCache);
                }
                catch (Exception exception) when (IsMetadataException(exception))
                {
                    PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataFailures);
                    var key = new CommandLookupKey(commandName, commandTypes);
                    if (_commandInfoCache.TryGetValue(key, out var lookup)
                        && lookup.IsValueCreated && ReferenceEquals(lookup.Value, command))
                    {
                        RemoveLookup(key, lookup);
                    }
#if DISABLE_ENGINE_RETRIES
                    throw;
#else
                    if (bypassCache || !IsRunspaceAffinityException(exception)) return null;
                    PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataRetries);
                    bypassCache = true;
#endif
                }
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
            string commandName, CommandTypes? commandTypes = null, bool bypassCache = false)
        {
            return GetCommandMetadata(commandName, commandTypes, bypassCache, GetParameterSnapshot);
        }

        private IReadOnlyDictionary<string, CommandParameterSnapshot> GetParameterSnapshot(CommandInfo command, bool bypassCache)
        {
            var staticCmdlet = bypassCache ? null : GetStaticCmdlet(command);
            if (disposed || command == null) return null;
            if (staticCmdlet != null && _parameterSnapshots.TryGetValue(staticCmdlet, out var cached)) return cached;

            using (PerformanceTelemetry.EnterLock(_runspaceLock))
            {
                if (disposed) return null;
                if (staticCmdlet != null && _parameterSnapshots.TryGetValue(staticCmdlet, out cached)) return cached;
                PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataQueries);
                var parameters = command.Parameters;
                if (parameters == null) return null;
                var snapshot = new ReadOnlyDictionary<string, CommandParameterSnapshot>(
                    parameters.ToDictionary(p => p.Key, p => new CommandParameterSnapshot(p.Value), parameters.Comparer));
                if (staticCmdlet != null) _parameterSnapshots[staticCmdlet] = snapshot;
                return snapshot;
            }
        }

        public IReadOnlyList<string> GetMandatoryParameterNames(string commandName)
        {
            return GetCommandMetadata(commandName, null, false, GetMandatoryParameterNames);
        }

        private IReadOnlyList<string> GetMandatoryParameterNames(CommandInfo command, bool bypassCache)
        {
            var staticCmdlet = bypassCache ? null : GetStaticCmdlet(command);
            if (disposed || command == null) return null;
            if (staticCmdlet != null && _mandatoryParameters.TryGetValue(staticCmdlet, out var cached)) return cached;

            using (PerformanceTelemetry.EnterLock(_runspaceLock))
            {
                if (disposed) return null;
                if (staticCmdlet != null && _mandatoryParameters.TryGetValue(staticCmdlet, out cached)) return cached;
                PerformanceTelemetry.Increment(ref PerformanceTelemetry.MetadataQueries);
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
                    hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
                    hash = hash * 31 + CommandTypes.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
