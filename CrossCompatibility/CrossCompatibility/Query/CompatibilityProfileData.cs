using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;
using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Modules.ModuleData;
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class CompatibilityProfileData
    {
        private readonly RuntimeData _runtimeData;

        private readonly PlatformData _platformData;

        public CompatibilityProfileData(CompatibilityProfileDataMut compatibilityProfileData)
        {
            _runtimeData = new RuntimeData(compatibilityProfileData.Compatibility);
            _platformData = new PlatformData(compatibilityProfileData.Platform);
        }
    }
}