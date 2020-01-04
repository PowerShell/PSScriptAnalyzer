// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>> _commandInfoCache;
        private readonly Helper _helperInstance;
        private readonly RunspacePool _runspacePool;
        private bool disposed = false;

        /// <summary>
        /// Create a fresh command info cache instance.
        /// </summary>
        public CommandInfoCache(Helper pssaHelperInstance)
        {
            _commandInfoCache = new ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>>();
            _helperInstance = pssaHelperInstance;
            _runspacePool = RunspaceFactory.CreateRunspacePool(1, 10);
            _runspacePool.Open();
        }

        /// <summary>Dispose the runspace pool</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ( disposed )
            {
                return;
            }

            if ( disposing )
            {
                _runspacePool.Dispose();
            }

            disposed = true;
        }

        /// <summary>
        /// Retrieve a command info object about a command.
        /// </summary>
        /// <param name="commandName">Name of the command to get a commandinfo object for.</param>
        /// <param name="commandTypes">What types of command are needed. If omitted, all types are retrieved.</param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string commandName, CommandTypes? commandTypes = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            var key = new CommandLookupKey(commandName, commandTypes);
            // Atomically either use PowerShell to query a command info object, or fetch it from the cache
            return _commandInfoCache.GetOrAdd(key, new Lazy<CommandInfo>(() => GetCommandInfoInternal(commandName, commandTypes))).Value;
        }


        /// <summary>
        /// Get a CommandInfo object of the given command name
        /// </summary>
        /// <returns>Returns null if command does not exists</returns>
        private CommandInfo GetCommandInfoInternal(string cmdName, CommandTypes? commandType)
        {
            // 'Get-Command ?' would return % for example due to PowerShell interpreting is a single-character-wildcard search and not just the ? alias.
            // For more details see https://github.com/PowerShell/PowerShell/issues/9308
            cmdName = WildcardPattern.Escape(cmdName);

            using (var ps = System.Management.Automation.PowerShell.Create())
            {
                ps.RunspacePool = _runspacePool;

                ps.AddCommand("Get-Command")
                    .AddParameter("Name", cmdName)
                    .AddParameter("ErrorAction", "SilentlyContinue");

                if (commandType != null)
                {
                    ps.AddParameter("CommandType", commandType);
                }

                return ps.Invoke<CommandInfo>()
                    .FirstOrDefault();
            }
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
