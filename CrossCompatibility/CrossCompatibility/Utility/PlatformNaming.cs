using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public static class PlatformNaming
    {
        public static string PlatformNameJoiner
        {
            get
            {
                return "_";
            }
        }

        /// <summary>
        /// Gets a unique name for the target PowerShell platform.
        /// Schema is "{os-name}_{os-arch}_{os-version}_{powershell-version}_{process-arch}".
        /// os-name for Windows is "win-{sku-id}".
        /// os-name for Linux is the ID entry in /etc/os-release.
        /// os-name for Mac is "macos".
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static string GetPlatformName(PlatformData platform)
        {
            string psVersion = platform.PowerShell.Version?.ToString();
            string osVersion = platform.OperatingSystem.Version;
            string osArch = platform.OperatingSystem.Architecture.ToString().ToLower();
            string pArch = platform.PowerShell.ProcessArchitecture.ToString().ToLower();

            string[] platformNameComponents;
            switch (platform.OperatingSystem.Family)
            {
                case OSFamily.Windows:
                    uint skuId = platform.OperatingSystem.SkuId;

                    platformNameComponents = new [] { $"win-{skuId}", osArch, osVersion, psVersion, pArch };
                    break;

                case OSFamily.MacOS:
                    platformNameComponents = new [] { "macos", osArch, osVersion, psVersion, pArch };
                    break;

                case OSFamily.Linux:
                    string distroId = platform.OperatingSystem.DistributionId;
                    string distroVersion = platform.OperatingSystem.DistributionVersion;

                    platformNameComponents = new [] { distroId, osArch, distroVersion, psVersion, pArch };
                    break;

                default:
                    // We shouldn't ever see anything like this
                    platformNameComponents = new [] { "unknown", osArch, osVersion ?? "?", psVersion, pArch };
                    break;
            }

            return string.Join(PlatformNameJoiner, platformNameComponents);
        }
    }
}