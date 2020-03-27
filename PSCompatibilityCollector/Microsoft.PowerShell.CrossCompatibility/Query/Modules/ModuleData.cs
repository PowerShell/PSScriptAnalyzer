// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a PowerShell module data object.
    /// </summary>
    public class ModuleData
    {
        /// <summary>
        /// Create a query object around a module data object.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="version">The version of the module.</param>
        /// <param name="moduleData">The module data object.</param>
        public ModuleData(string name, Version version, Data.ModuleData moduleData)
        {
            Name = name;
            Version = version;
            Guid = moduleData.Guid;
            Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>> commands = CreateCommandTables(moduleData.Functions, moduleData.Cmdlets);
            Functions = commands.Item1;
            Cmdlets = commands.Item2;
            if (moduleData.Variables != null)
            {
                Variables = new List<string>(moduleData.Variables);
            }
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
        public Guid Guid { get; }

        /// <summary>
        /// Functions exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, FunctionData> Functions { get; }

        /// <summary>
        /// Cmdlets exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, CmdletData> Cmdlets { get; }

        /// <summary>
        /// Variables exported by the module.
        /// </summary>
        public IReadOnlyList<string> Variables { get; }

        /// <summary>
        /// Aliases exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Aliases { get; private set; }

        internal void SetAliasTable(
            RuntimeData runtimeData,
            IReadOnlyDictionary<string, string> aliases)
        {
            if (aliases == null || aliases.Count == 0)
            {
                return;
            }

            var aliasTable = new Dictionary<string, IReadOnlyList<CommandData>>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> alias in aliases)
            {
                if (runtimeData.Aliases.TryGetValue(alias.Key, out IReadOnlyList<CommandData> aliasedCommands))
                {
                    aliasTable[alias.Key] = aliasedCommands;
                }
            }

            Aliases = aliasTable;
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
