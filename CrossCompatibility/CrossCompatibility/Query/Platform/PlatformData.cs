using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Platform.PlatformData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Platform
{
    public class PlatformData
    {
        public PlatformData(PlatformDataMut platformData)
        {
            if (platformData != null)
            {
                Dotnet = new DotnetData(platformData.Dotnet);
                OperatingSystem = new OperatingSystemData(platformData.OperatingSystem);
                PowerShell = new PowerShellData(platformData.PowerShell);
            }
        }

        public DotnetData Dotnet { get; }

        public OperatingSystemData OperatingSystem { get; }

        public PowerShellData PowerShell { get; }
    }
}