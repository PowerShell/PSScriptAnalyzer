using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Query.Types;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class RuntimeData
    {
        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>> _commands;

        public RuntimeData(RuntimeDataMut runtimeData)
        {
            Modules = runtimeData.Modules.ToDictionary(m => m.Key, m => (IReadOnlyDictionary<Version, ModuleData>)m.Value.ToDictionary(mv => mv.Key, mv => new ModuleData(m.Key, mv.Key, mv.Value)), StringComparer.OrdinalIgnoreCase);
            Types = new AvailableTypeData(runtimeData.Types);

            _commands = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>>(() => CreateCommandLookupTable(Modules.Values.SelectMany(mv => mv.Values)));
        }

        public AvailableTypeData Types { get; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<Version, ModuleData>> Modules { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Commands => _commands.Value;

        private static IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateCommandLookupTable(
            IEnumerable<ModuleData> modules)
        {
            var commandTable = new Dictionary<string, IReadOnlyList<CommandData>>(StringComparer.OrdinalIgnoreCase);
            foreach (ModuleData module in modules)
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

            foreach (ModuleData module in modules)
            {
                if (module.Aliases != null)
                {
                    foreach (KeyValuePair<string, string> alias in module.Aliases)
                    {
                        // Link the alias to the actual command
                        if (commandTable.ContainsKey(alias.Value))
                        {
                            // TODO: This isn't quite accurate, but we need more information
                            // on which command the alias actually targets
                            commandTable.Add(alias.Key, commandTable[alias.Value]);
                            continue;
                        }

                        commandTable.Add(alias.Key, new List<CommandData>());
                    }
                }
            }

            return commandTable;
        }
    }
}