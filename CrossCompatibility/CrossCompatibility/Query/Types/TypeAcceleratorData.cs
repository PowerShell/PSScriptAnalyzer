using TypeAcceleratorDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeAcceleratorData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Types
{
    public class TypeAcceleratorData
    {
        private readonly TypeAcceleratorDataMut _typeAcceleratorData;

        public TypeAcceleratorData(string name, TypeAcceleratorDataMut TypeAcceleratorData)
        {
            Name = name;
        }

        public string Name { get; }

        public string Assembly => _typeAcceleratorData.Assembly;

        public string Type => _typeAcceleratorData.Type;
    }
}