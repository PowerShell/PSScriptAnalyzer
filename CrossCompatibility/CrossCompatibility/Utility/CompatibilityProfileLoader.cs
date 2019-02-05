// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.CrossCompatibility.Query;
using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public class CompatibilityProfileLoader
    {
        private static Lazy<CompatibilityProfileLoader> s_sharedInstance = new Lazy<CompatibilityProfileLoader>(() => new CompatibilityProfileLoader());

        public static CompatibilityProfileLoader StaticInstance => s_sharedInstance.Value;

        private readonly JsonProfileSerializer _jsonSerializer;

        private readonly IDictionary<string, CompatibilityProfileData> _profileCache;

        private readonly object _loaderLock;

        public CompatibilityProfileLoader()
        {
            _jsonSerializer = JsonProfileSerializer.Create();
            _profileCache = new Dictionary<string, CompatibilityProfileData>();
            _loaderLock = new object();
        }

        public CompatibilityProfileData GetProfileFromFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            lock (_loaderLock)
            {
                if (_profileCache.ContainsKey(path))
                {
                    return _profileCache[path];
                }

                CompatibilityProfileDataMut compatibilityProfileMut = _jsonSerializer.DeserializeFromFile(path);

                var compatibilityProfile = new CompatibilityProfileData(compatibilityProfileMut);

                _profileCache[path] = compatibilityProfile;

                return compatibilityProfile;
            }
        }

        public void ClearCache()
        {
            lock (_loaderLock)
            {
                _profileCache.Clear();
            }
        }
    }
}