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
            Modules = runtimeData.Modules.ToDictionary(m => m.Key, m => new ModuleData(m.Key, m.Value));
            Types = new AvailableTypeData(runtimeData.Types);

            _commands = new Lazy<IReadOnlyDictionary<string, IReadOnlyList<CommandData>>>(() => CreateCommandLookupTable(Modules.Values));
        }

        public AvailableTypeData Types { get; }

        public IReadOnlyDictionary<string, ModuleData> Modules { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<CommandData>> Commands => _commands.Value;

        private static IReadOnlyDictionary<string, IReadOnlyList<CommandData>> CreateCommandLookupTable(
            IEnumerable<ModuleData> modules)
        {
            var commandTable = new Dictionary<string, IReadOnlyList<CommandData>>();
            foreach (ModuleData module in modules)
            {
            }

            return commandTable;
        }
    }
}