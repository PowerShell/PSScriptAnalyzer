using Types = Microsoft.PowerShell.CrossCompatibility.Data.Types;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class AssemblyData
    {
        private readonly Types.AssemblyData _assemblyData;

        public AssemblyData(Types.AssemblyData assemblyData)
        {
            _assemblyData = assemblyData;
            AssemblyName = new AssemblyNameData(_assemblyData.AssemblyName);
        }

        public AssemblyNameData AssemblyName { get; }
    }
}