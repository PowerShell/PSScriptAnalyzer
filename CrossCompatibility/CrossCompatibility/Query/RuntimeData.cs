// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.PowerShell.CrossCompatibility.Query.Types;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for collected data about commands and types available in a PowerShell runtime.
    /// </summary>
    public class RuntimeData
    {
        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>>> _modules;

        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>> _commands;

        private readonly Lazy<NativeCommandLookupTable> _nativeCommands;

        /// <summary>
        /// Create a new query object around collected PowerShell runtime data.
        /// </summary>
        /// <param name="runtimeData">The collected PowerShell runtime data object.</param>
        public RuntimeData(Data.RuntimeData runtimeData)
        {
            Types = new AvailableTypeData(runtimeData.Types);
            Common = new CommonPowerShellData(runtimeData.Common);

            _modules = new Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>>>(() => CreateModuleTable(runtimeData.Modules));
            _commands = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>>(() => CreateCommandLookupTable(Modules));
            _nativeCommands = new Lazy<NativeCommandLookupTable>(() => NativeCommandLookupTable.Create(runtimeData.NativeCommands));
        }

        /// <summary>
        /// All the types and type accelerators available in the PowerShell runtime.
        /// </summary>
        public AvailableTypeData Types { get; }

        /// <summary>
        /// All the default modules available to the PowerShell runtime, keyed by module name and then version.
        /// </summary>
        /// <value></value>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> Modules => _modules.Value;

        /// <summary>
        /// A lookup table for commands from modules.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Commands => _commands.Value;

        /// <summary>
        /// All the native commands/applications available to PowerShell.
        /// </summary>
        public NativeCommandLookupTable NativeCommands => _nativeCommands.Value;

        /// <summary>
        /// PowerShell runtime data not confined to a module.
        /// </summary>
        public CommonPowerShellData Common { get; }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> CreateModuleTable(IDictionary<string, JsonDictionary<Version, Data.Modules.ModuleData>> modules)
        {
            var moduleDict = new Dictionary<string, IReadOnlyDictionary<Version, ModuleData>>(modules.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JsonDictionary<Version, Data.Modules.ModuleData>> moduleVersions in modules)
            {
                var moduleVersionDict = new Dictionary<Version, ModuleData>(moduleVersions.Value.Count);
                foreach (KeyValuePair<Version, Data.Modules.ModuleData> module in moduleVersions.Value)
                {
                    moduleVersionDict[module.Key] = new ModuleData(name: moduleVersions.Key, version: module.Key, moduleData: module.Value);
                }
                moduleDict[moduleVersions.Key] = moduleVersionDict;
            }
            return moduleDict;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateCommandLookupTable(
            IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> modules)
        {
            var commandTable = new Dictionary<string, IReadOnlyList<CommandData>>(StringComparer.OrdinalIgnoreCase);
            foreach (IReadOnlyDictionary<Version, ModuleData> moduleVersions in modules.Values)
            {
                foreach (ModuleData module in moduleVersions.Values)
                {
                    if (module.Cmdlets != null)
                    {
                        foreach (KeyValuePair<string, CmdletData> cmdlet in module.Cmdlets)
                        {
                            if (!commandTable.ContainsKey(cmdlet.Key))
                            {
                                commandTable.Add(cmdlet.Key, new List<CommandData>());
                            }

                            ((List<CommandData>)commandTable[cmdlet.Key]).Add(cmdlet.Value);
                        }
                    }

                    if (module.Functions != null)
                    {
                        foreach (KeyValuePair<string, FunctionData> function in module.Functions)
                        {
                            if (!commandTable.ContainsKey(function.Key))
                            {
                                commandTable.Add(function.Key, new List<CommandData>());
                            }

                            ((List<CommandData>)commandTable[function.Key]).Add(function.Value);
                        }
                    }

                    if (module.Aliases != null)
                    {
                        foreach (KeyValuePair<string, CommandData> alias in module.Aliases)
                        {
                            if (!commandTable.ContainsKey(alias.Key))
                            {
                                commandTable.Add(alias.Key, new List<CommandData>());
                            }

                            ((List<CommandData>)commandTable[alias.Key]).Add(alias.Value);
                        }
                    }
                }
            }

            return commandTable;
        }
    }
}
