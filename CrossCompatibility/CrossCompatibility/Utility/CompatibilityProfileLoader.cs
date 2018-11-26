using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.CrossCompatibility.Query;
using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public class CompatibilityProfileLoader
    {
        private readonly JsonProfileSerializer _jsonSerializer;

        private readonly IDictionary<string, CompatibilityProfileData> _profileCache;

        public CompatibilityProfileLoader()
        {
            _jsonSerializer = JsonProfileSerializer.Create();
            _profileCache = new Dictionary<string, CompatibilityProfileData>();
        }

        public CompatibilityProfileData GetProfileFromFilePath(string path)
        {
            if (_profileCache.ContainsKey(path))
            {
                return _profileCache[path];
            }

            CompatibilityProfileDataMut compatibilityProfileMut = _jsonSerializer.DeserializeFromFile(path);

            var compatibilityProfile = new CompatibilityProfileData(compatibilityProfileMut);

            _profileCache.Add(path, compatibilityProfile);

            return compatibilityProfile;
        }
    }
}