using AssemblyDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class AssemblyData
    {
        private readonly AssemblyDataMut _assemblyData;

        public AssemblyData(AssemblyDataMut assemblyData)
        {
            _assemblyData = assemblyData;
            AssemblyName = new AssemblyNameData(_assemblyData.AssemblyName);
        }

        public AssemblyNameData AssemblyName { get; }
    }
}