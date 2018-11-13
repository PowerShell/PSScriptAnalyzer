using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Query.Types;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class RuntimeData
    {
        public RuntimeData(RuntimeDataMut runtimeData)
        {
            Modules = runtimeData.Modules.ToDictionary(m => m.Key, m => new ModuleData(m.Key, m.Value));
            Types = new AvailableTypeData(runtimeData.Types);
        }

        public AvailableTypeData Types { get; }

        public IReadOnlyDictionary<string, ModuleData> Modules { get; }
    }
}