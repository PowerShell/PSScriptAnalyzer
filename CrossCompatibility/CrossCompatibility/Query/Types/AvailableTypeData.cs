using System.Collections.Generic;
using System.Linq;
using AvailableTypeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.AvailableTypeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Types
{
    public class AvailableTypeData
    {
        public AvailableTypeData(AvailableTypeDataMut availableTypeData)
        {
            TypeAccelerators = availableTypeData.TypeAccelerators.ToDictionary(ta => ta.Key, ta => ta.Value);
            Assemblies = availableTypeData.Assemblies.ToDictionary(asm => asm.Key, asm => new AssemblyData(asm.Value));
        }

        public IReadOnlyDictionary<string, string> TypeAccelerators { get; }

        public IReadOnlyDictionary<string, AssemblyData> Assemblies { get; }
    }
}