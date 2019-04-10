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
        private readonly Data.ModuleData _moduleData;

        private readonly Lazy<Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>, IReadOnlyDictionary<string, CommandData>>> _commands;

        /// <summary>
        /// Create a query object around a module data object.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="version">The version of the module.</param>
        /// <param name="moduleData">The module data object.</param>
        public ModuleData(string name, Version version, Data.ModuleData moduleData)
        {
            _moduleData = moduleData;

            Name = name;
            Version = version;

            _commands = new Lazy<Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>, IReadOnlyDictionary<string, CommandData>>>(() => CreateCommandTables(moduleData.Functions, moduleData.Cmdlets, moduleData.Aliases));
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
        public IReadOnlyDictionary<string, FunctionData> Functions => _commands.Value.Item1;

        /// <summary>
        /// Cmdlets exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, CmdletData> Cmdlets => _commands.Value.Item2;

        /// <summary>
        /// Variables exported by the module.
        /// </summary>
        public IReadOnlyList<string> Variables => _moduleData.Variables;

        /// <summary>
        /// Aliases exported by the module.
        /// </summary>
        public IReadOnlyDictionary<string, CommandData> Aliases => _commands.Value.Item3;

        private static Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>, IReadOnlyDictionary<string, CommandData>> CreateCommandTables(
            IReadOnlyDictionary<string, Data.FunctionData> functions,
            IReadOnlyDictionary<string, Data.CmdletData> cmdlets,
            IReadOnlyDictionary<string, string> aliases)
        {
            Dictionary<string, FunctionData> funcDict = null;
            Dictionary<string, CmdletData> cmdletDict = null;
            Dictionary<string, CommandData> aliasDict = null;

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

            if (aliases != null)
            {
                aliasDict = new Dictionary<string, CommandData>(aliases.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, string> alias in aliases)
                {
                    if (funcDict != null && funcDict.TryGetValue(alias.Value, out FunctionData function))
                    {
                        aliasDict[alias.Key] = function;
                        continue;
                    }

                    if (cmdletDict != null && cmdletDict.TryGetValue(alias.Value, out CmdletData cmdlet))
                    {
                        aliasDict[alias.Key] = cmdlet;
                        continue;
                    }
                }
            }

            return new Tuple<IReadOnlyDictionary<string, FunctionData>, IReadOnlyDictionary<string, CmdletData>, IReadOnlyDictionary<string, CommandData>>(
                funcDict,
                cmdletDict,
                aliasDict);
        }
    }
}
