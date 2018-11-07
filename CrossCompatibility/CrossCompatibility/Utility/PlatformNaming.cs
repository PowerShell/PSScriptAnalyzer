using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public static class PlatformNaming
    {
        public static string GetPlatformName(PlatformData platform)
        {
            string psVersion = platform.PowerShell.Version?.ToString();
            string osVersion = platform.OperatingSystem.Version;
            string osArch = platform.OperatingSystem.Architecture.ToString().ToLower();
            string pArch = platform.PowerShell.ProcessArchitecture.ToString().ToLower();
            switch (platform.OperatingSystem.Family)
            {
                case OSFamily.Windows:

                    return $"win-{osArch}-{osVersion}-{psVersion}-{pArch}";

                case OSFamily.MacOS:
                    return $"macos-{osArch}-{osVersion}-{psVersion}-{pArch}";

                case OSFamily.Linux:
                    string distroId = platform.OperatingSystem.DistributionId;
                    string distroVersion = platform.OperatingSystem.DistributionVersion;

                    return $"linux_{distroId}-{osArch}-{distroVersion}-{psVersion}-{pArch}";

                default:
                    // We shouldn't ever see anything like this
                    return $"unknown-{osArch}-{osVersion ?? "?"}-{psVersion}-{pArch}";
            }
        }
    }
}