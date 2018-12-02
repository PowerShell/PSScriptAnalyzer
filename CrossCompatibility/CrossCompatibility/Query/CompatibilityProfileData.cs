using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;
using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData;
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class CompatibilityProfileData
    {
        public CompatibilityProfileData(CompatibilityProfileDataMut compatibilityProfileData)
        {
            Runtime = new RuntimeData(compatibilityProfileData.Compatibility);
            Platform = compatibilityProfileData.Platforms.Select(p => new PlatformData(p)).ToArray();
        }

        public RuntimeData Runtime { get; }

        public IReadOnlyList<PlatformData> Platform { get; }
    }
}