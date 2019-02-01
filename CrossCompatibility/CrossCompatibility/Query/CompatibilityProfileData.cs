using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;
using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData;
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class CompatibilityProfileData
    {
        public CompatibilityProfileData(CompatibilityProfileDataMut compatibilityProfileData)
        {
            Platform = new PlatformData(compatibilityProfileData.Platform);
            Runtime = new RuntimeData(compatibilityProfileData.Compatibility, isForWindows: compatibilityProfileData.Platform.OperatingSystem.Family == OSFamily.Windows);
        }

        public RuntimeData Runtime { get; }

        public PlatformData Platform { get; }
    }
}