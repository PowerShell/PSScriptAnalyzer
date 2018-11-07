using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public static class PlatformNaming
    {
        public static string GetPlatformName(PlatformData platform)
        {
            string psVersion = platform.PowerShell.Version?.ToString();
            string osVersion = platform.OperatingSystem.Version;
            string arch = platform.Machine.Architecture;
            switch (platform.OperatingSystem.Family)
            {
                case OSFamily.Windows:

                    return $"win.{osVersion}-{psVersion}-{arch}";

                case OSFamily.MacOS:
                    return $"macos.{osVersion}-{psVersion}-{arch}";

                case OSFamily.Linux:
                    string distroId = platform.OperatingSystem.DistributionId;
                    string distroVersion = platform.OperatingSystem.DistributionVersion;

                    return $"linux_{distroId}.{distroVersion}-{psVersion}-{arch}";

                default:
                    return $"unknown.{osVersion ?? "?"}-{psVersion}-{arch}";
            }
        }
    }
}