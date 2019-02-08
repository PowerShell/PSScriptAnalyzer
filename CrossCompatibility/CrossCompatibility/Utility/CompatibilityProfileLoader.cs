// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.CrossCompatibility.Query;
using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;

#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    /// <summary>
    /// Encapsulates loading and caching of compatibility profiles.
    /// Intended to be thread safe for usage by multiple PSSA rules.
    /// </summary>
    public class CompatibilityProfileLoader
    {
        private static Lazy<CompatibilityProfileLoader> s_sharedInstance = new Lazy<CompatibilityProfileLoader>(() => new CompatibilityProfileLoader());

        /// <summary>
        /// A lazy-initialized static instance to allow for a shared profile cache.
        /// </summary>
        public static CompatibilityProfileLoader StaticInstance => s_sharedInstance.Value;

        private readonly JsonProfileSerializer _jsonSerializer;

        private readonly IDictionary<string, CompatibilityProfileData> _profileCache;

        private readonly object _loaderLock;

        /// <summary>
        /// Create a new compatibility profile loader with an empty cache.
        /// </summary>
        public CompatibilityProfileLoader()
        {
            _jsonSerializer = JsonProfileSerializer.Create();
            _loaderLock = new object();

            // Cache keys are filenames, which must be case-insensitive in Windows and macOS
#if NETSTANDARD2_0
            _profileCache = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new Dictionary<string, CompatibilityProfileData>()
                : new Dictionary<string, CompatibilityProfileData>(StringComparer.OrdinalIgnoreCase);
#else
            _profileCache = new Dictionary<string, CompatibilityProfileData>(StringComparer.OrdinalIgnoreCase);
#endif
        }

        /// <summary>
        /// Load a profile from a path.
        /// Caches profiles based on path, so that repeated calls do not require JSON deserialization.
        /// </summary>
        /// <param name="path">The path to load a profile from.</param>
        /// <returns>A query object around the loaded profile.</returns>
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

        /// <summary>
        /// Clear all loaded profiles from this loader.
        /// </summary>
        public void ClearCache()
        {
            lock (_loaderLock)
            {
                _profileCache.Clear();
            }
        }
    }
}