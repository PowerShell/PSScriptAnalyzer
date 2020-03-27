// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for collected data about commands and types available in a PowerShell runtime.
    /// </summary>
    public class RuntimeData
    {
        /// <summary>
        /// Create a new query object around collected PowerShell runtime data.
        /// </summary>
        /// <param name="runtimeData">The collected PowerShell runtime data object.</param>
        public RuntimeData(Data.RuntimeData runtimeData)
        {
            Types = new AvailableTypeData(runtimeData.Types);
            Common = new CommonPowerShellData(runtimeData.Common);

            Modules = CreateModuleTable(runtimeData.Modules);
            NonAliasCommands = CreateNonAliasCommandLookupTable(Modules);
            Aliases = CreateAliasLookupTable(runtimeData.Modules, NonAliasCommands);
            SetModuleAliases(runtimeData.Modules);
            Commands = new DualLookupTable<string, IReadOnlyList<CommandData>>(NonAliasCommands, Aliases);
            NativeCommands = NativeCommandLookupTable.Create(runtimeData.NativeCommands);
        }

        /// <summary>
        /// All the types and type accelerators available in the PowerShell runtime.
        /// </summary>
        public AvailableTypeData Types { get; }

        /// <summary>
        /// All the default modules available to the PowerShell runtime, keyed by module name and then version.
        /// </summary>
        /// <value></value>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> Modules { get; }

        /// <summary>
        /// A lookup table for commands from modules.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Commands { get; }

        /// <summary>
        /// All the native commands/applications available to PowerShell.
        /// </summary>
        public NativeCommandLookupTable NativeCommands { get; }

        /// <summary>
        /// PowerShell runtime data not confined to a module.
        /// </summary>
        public CommonPowerShellData Common { get; }

        internal IReadOnlyDictionary<string, IReadOnlyList<CommandData>> NonAliasCommands { get; }

        internal IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Aliases { get; }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> CreateModuleTable(IDictionary<string, JsonDictionary<Version, Data.ModuleData>> modules)
        {
            var moduleDict = new Dictionary<string, IReadOnlyDictionary<Version, ModuleData>>(modules.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JsonDictionary<Version, Data.ModuleData>> moduleVersions in modules)
            {
                var moduleVersionDict = new Dictionary<Version, ModuleData>(moduleVersions.Value.Count);
                foreach (KeyValuePair<Version, Data.ModuleData> module in moduleVersions.Value)
                {
                    moduleVersionDict[module.Key] = new ModuleData(name: moduleVersions.Key, version: module.Key, moduleData: module.Value);
                }
                moduleDict[moduleVersions.Key] = moduleVersionDict;
            }
            return moduleDict;
        }

        private void SetModuleAliases(JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, Data.ModuleData>> moduleData)
        {
            foreach (KeyValuePair<string, IReadOnlyDictionary<Version, ModuleData>> moduleVersions in Modules)
            {
                foreach (KeyValuePair<Version, ModuleData> module in moduleVersions.Value)
                {
                    module.Value.SetAliasTable(this, moduleData[moduleVersions.Key][module.Key].Aliases);
                }
            }
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateNonAliasCommandLookupTable(
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
                }
            }

            return commandTable;
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateAliasLookupTable(
            IReadOnlyDictionary<string, JsonDictionary<Version, Data.ModuleData>> modules,
            IReadOnlyDictionary<string, IReadOnlyList<CommandData>> commands)
        {
            var aliasTable = new Dictionary<string, IReadOnlyList<CommandData>>();
            foreach (KeyValuePair<string, JsonDictionary<Version, Data.ModuleData>> module in modules)
            {
                foreach (KeyValuePair<Version, Data.ModuleData> moduleVersion in module.Value)
                {
                    if (moduleVersion.Value.Aliases == null)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, string> alias in moduleVersion.Value.Aliases)
                    {
                        if (commands.TryGetValue(alias.Value, out IReadOnlyList<CommandData> aliasedCommands))
                        {
                            aliasTable[alias.Key] = aliasedCommands;
                        }
                    }
                }
            }
            return aliasTable;
        }

        private class DualLookupTable<K, V> : IReadOnlyDictionary<K, V>
        {
            private readonly IReadOnlyDictionary<K, V> _firstTable;

            private readonly IReadOnlyDictionary<K, V> _secondTable;

            public DualLookupTable(IReadOnlyDictionary<K, V> firstTable, IReadOnlyDictionary<K, V> secondTable)
            {
                _firstTable = firstTable;
                _secondTable = secondTable;
            }

            public V this[K key]
            {
                get
                {
                    if (_firstTable.TryGetValue(key, out V firstValue))
                    {
                        return firstValue;
                    }

                    if (_secondTable.TryGetValue(key, out V secondValue))
                    {
                        return secondValue;
                    }

                    throw new KeyNotFoundException();
                }
            }

            public IEnumerable<K> Keys => _firstTable.Keys.Concat(_secondTable.Keys);

            public IEnumerable<V> Values => _firstTable.Values.Concat(_secondTable.Values);

            public int Count => _firstTable.Count + _secondTable.Count;

            public bool ContainsKey(K key)
            {
                return _firstTable.ContainsKey(key) || _secondTable.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                return _firstTable.Concat(_secondTable).GetEnumerator();
            }

            public bool TryGetValue(K key, out V value)
            {
                return _firstTable.TryGetValue(key, out value)
                    || _secondTable.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
