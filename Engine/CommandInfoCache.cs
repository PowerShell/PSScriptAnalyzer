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
    internal class CommandInfoCache
    {
        private readonly ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>> _commandInfoCache;
        private readonly RunspacePool _runspacePool;
        private readonly Helper _helperInstance;

        /// <summary>
        /// Create a fresh command info cache instance.
        /// </summary>
        public CommandInfoCache(Helper pssaHelperInstance)
        {
            _commandInfoCache = new ConcurrentDictionary<CommandLookupKey, Lazy<CommandInfo>>();
            _helperInstance = pssaHelperInstance;
            // There are only 4 rules that use the CommandInfo cache and each rule does not request more than one concurrent command info request
            _runspacePool = RunspaceFactory.CreateRunspacePool(1, 5);
            _runspacePool.Open();
        }

        /// <summary>
        /// Retrieve a command info object about a command.
        /// </summary>
        /// <param name="commandName">Name of the command to get a commandinfo object for.</param>
        /// <param name="aliasName">The alias of the command to be used in the cache key. If not given, uses the command name.</param>
        /// <param name="commandTypes">What types of command are needed. If omitted, all types are retrieved.</param>
        /// <returns></returns>
        public CommandInfo GetCommandInfo(string commandName, string aliasName = null, CommandTypes? commandTypes = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return null;
            }

            // If alias name is given, we store the entry under that, but search with the command name
            var key = new CommandLookupKey(aliasName ?? commandName, commandTypes);

            // Atomically either use PowerShell to query a command info object, or fetch it from the cache
            return _commandInfoCache.GetOrAdd(key, new Lazy<CommandInfo>(() => GetCommandInfoInternal(commandName, commandTypes))).Value;
        }

        /// <summary>
        /// Retrieve a command info object about a command.
        /// </summary>
        /// <param name="commandName">Name of the command to get a commandinfo object for.</param>
        /// <param name="commandTypes">What types of command are needed. If omitted, all types are retrieved.</param>
        /// <returns></returns>
        [Obsolete("Alias lookup is expensive and should not be relied upon for command lookup")]
        public CommandInfo GetCommandInfoLegacy(string commandOrAliasName, CommandTypes? commandTypes = null)
        {
            string commandName = _helperInstance.GetCmdletNameFromAlias(commandOrAliasName);

            return string.IsNullOrEmpty(commandName)
                ? GetCommandInfo(commandOrAliasName, commandTypes: commandTypes)
                : GetCommandInfo(commandName, aliasName: commandOrAliasName, commandTypes: commandTypes);
        }

        /// <summary>
        /// Get a CommandInfo object of the given command name
        /// </summary>
        /// <returns>Returns null if command does not exists</returns>
        private CommandInfo GetCommandInfoInternal(string cmdName, CommandTypes? commandType)
        {
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
