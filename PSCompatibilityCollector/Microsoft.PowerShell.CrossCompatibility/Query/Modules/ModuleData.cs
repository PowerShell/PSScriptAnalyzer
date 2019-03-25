// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a PowerShell module data object.
    /// </summary>
    public class ModuleData
    {
        private readonly RuntimeData _parent;

        private readonly Data.ModuleData _moduleData;

        private readonly Lazy<Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>>> _lazyCommands;

        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>> _lazyAliases;

        /// <summary>
        /// Create a query object around a module data object.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="version">The version of the module.</param>
        /// <param name="moduleData">The module data object.</param>
        public ModuleData(string name, Version version, RuntimeData parent, Data.ModuleData moduleData)
        {
            _moduleData = moduleData;
            _parent = parent;

            Name = name;
            Version = version;

            _lazyCommands = new Lazy<Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>>>(() => CreateCommandTables(moduleData.Functions, moduleData.Cmdlets));
            _lazyAliases = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>>(() => CreateAliasTable(_moduleData.Aliases));
        }

        /// <summary>
        /// The name of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The version of the module.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// The GUID of the module.
        /// </summary>
        public Guid Guid => _moduleData.Guid;

        /// <summary>
        /// Functions exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionData> Functions => _lazyCommands.Value.Item1;

        /// <summary>
        /// Cmdlets exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, CmdletData> Cmdlets => _lazyCommands.Value.Item2;

        /// <summary>
        /// Variables exported by the module.
        /// </summary>
        public IReadOnlyList<string> Variables => _moduleData.Variables;

        /// <summary>
        /// Aliases exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Aliases => _lazyAliases.Value;

        private IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateAliasTable(IReadOnlyDictionary<string, string> aliases)
        {
            if (aliases == null || aliases.Count == 0)
            {
                return null;
            }

            var aliasTable = new Dictionary<string, IReadOnlyList<CommandData>>();
            foreach (KeyValuePair<string, string> alias in aliases)
            {
                if (_parent.Aliases.TryGetValue(alias.Key, out IReadOnlyList<CommandData> aliasedCommands))
                {
                    aliasTable[alias.Key] = aliasedCommands;
                }
            }

            return aliasTable;
        }

        private static Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>> CreateCommandTables(
            IReadOnlyDictionary<string, Data.FunctionData> functions,
            IReadOnlyDictionary<string, Data.CmdletData> cmdlets)
        {
            Dictionary<string, FunctionData> funcDict = null;
            Dictionary<string, CmdletData> cmdletDict = null;

            if (functions != null)
            {
                funcDict = new Dictionary<string, FunctionData>(functions.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Data.FunctionData> function in functions)
                {
                    funcDict[function.Key] = new FunctionData(function.Key, function.Value);
                }
            }

            if (cmdlets != null)
            {
                cmdletDict = new Dictionary<string, CmdletData>(cmdlets.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Data.CmdletData> cmdlet in cmdlets)
                {
                    cmdletDict[cmdlet.Key] = new CmdletData(cmdlet.Key, cmdlet.Value);
                }
            }

            return new Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>>(
                funcDict,
                cmdletDict);
        }
    }
}
