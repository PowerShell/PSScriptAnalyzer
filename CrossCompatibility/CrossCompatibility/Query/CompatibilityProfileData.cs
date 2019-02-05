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
            Runtime = new RuntimeData(compatibilityProfileData.Runtime);

            Id = compatibilityProfileData.Id;

            if (compatibilityProfileData.ConstituentProfiles != null)
            {
                ConstituentProfiles = new HashSet<string>(compatibilityProfileData.ConstituentProfiles);
            }

            // This should only be null in the case of the anyplatform_union profile
            if (compatibilityProfileData.Platform != null)
            {
                Platform = new PlatformData(compatibilityProfileData.Platform);
            }
        }

        public string Id { get; }

        public IReadOnlyCollection<string> ConstituentProfiles { get; }

        public RuntimeData Runtime { get; }

        public PlatformData Platform { get; }
    }
}