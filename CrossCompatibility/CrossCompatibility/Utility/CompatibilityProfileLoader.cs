// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerShell.CrossCompatibility.Query;
using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;

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
        private static readonly Lazy<CompatibilityProfileLoader> s_sharedInstance = new Lazy<CompatibilityProfileLoader>(() => new CompatibilityProfileLoader());

        private readonly JsonProfileSerializer _jsonSerializer;

        private readonly ConcurrentDictionary<string, Lazy<CompatibilityProfileCacheEntry>> _profileCache;

        /// <summary>
        /// A lazy-initialized static instance to allow for a shared profile cache.
        /// </summary>
        public static CompatibilityProfileLoader StaticInstance => s_sharedInstance.Value;

        /// <summary>
        /// Create a new compatibility profile loader with an empty cache.
        /// </summary>
        public CompatibilityProfileLoader()
        {
            _jsonSerializer = JsonProfileSerializer.Create();

            // Cache keys are filenames, which must be case-insensitive in Windows and macOS
#if NETSTANDARD2_0
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _profileCache = new ConcurrentDictionary<string, Lazy<CompatibilityProfileCacheEntry>>();
            }
            else
            {
                _profileCache = new ConcurrentDictionary<string, Lazy<CompatibilityProfileCacheEntry>>(StringComparer.OrdinalIgnoreCase);
            }
#else
            _profileCache = new ConcurrentDictionary<string, Lazy<CompatibilityProfileCacheEntry>>(StringComparer.OrdinalIgnoreCase);
#endif
        }

        /// <summary>
        /// For a set of profile paths, retrieve those profiles,
        /// along with the union profile for comparison.
        /// </summary>
        /// <param name="profileDirPath">The absolute path to the profile directory.</param>
        /// <param name="profilePaths">Absolute paths to all profiles to load.</param>
        /// <param name="unionProfile">The loaded union profile to compare against.</param>
        /// <returns>A list of hydrated profiles from all the profile paths given, not necessarily in order.</returns>
        public IEnumerable<CompatibilityProfileData> GetProfilesWithUnion(
            DirectoryInfo profileDirPath,
            IEnumerable<string> profilePaths,
            out CompatibilityProfileData unionProfile)
        {
            IEnumerable<CompatibilityProfileCacheEntry> profileEntries = GetProfilesFromPaths(profilePaths);

            unionProfile = GetUnionProfile(profileDirPath);

            return profileEntries.Select(p => p.Profile);
        }

        /// <summary>
        /// Clear all loaded profiles from this loader.
        /// </summary>
        public void ClearCache()
        {
            _profileCache.Clear();
        }

        private IEnumerable<CompatibilityProfileCacheEntry> GetProfilesFromPaths(IEnumerable<string> profilePaths)
        {
            // We have a situation where:
            //   - multiple caller threads
            //   - with some arguments the same but possibly some different
            //   - are trying to perform expensive (partially CPU-bound) cacheable computations
            //   - which are trivially parallel
            // We want to control all concurrency from the caller,
            // but also want to parallelize the computations for maximum throughput.
            //
            // So we:
            //   - Corrale all the load calls through a threadsafe cache of lazy calls (fan the load calls in from the number of calling threads)
            //   - Transform the query into PLINQ
            //   - Evaluate the lazy calls in parallel (fan the load calls out to the available global threadpool)
            //   - Put them into an array for the caller
            return profilePaths.Select(path => GetNonUnionProfileFromFilePath(path))
                .AsParallel()
                .AsUnordered()
                .Select(cacheEntry => cacheEntry.Value);
        }

        /// <summary>
        /// Load a profile from a path.
        /// Caches profiles based on path, so that repeated calls do not require JSON deserialization.
        /// </summary>
        /// <param name="path">The path to load a profile from.</param>
        /// <returns>A query object around the loaded profile.</returns>
        private Lazy<CompatibilityProfileCacheEntry> GetNonUnionProfileFromFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return _profileCache.GetOrAdd(path, new Lazy<CompatibilityProfileCacheEntry>(() => {
                CompatibilityProfileDataMut compatibilityProfileMut = _jsonSerializer.DeserializeFromFile(path);

                var compatibilityProfile = new CompatibilityProfileData(compatibilityProfileMut);

                return new CompatibilityProfileCacheEntry(
                    compatibilityProfileMut,
                    compatibilityProfile);
            }));
        }

        private CompatibilityProfileData GetUnionProfile(DirectoryInfo profileDir)
        {
            IEnumerable<CompatibilityProfileCacheEntry> profiles = GetProfilesFromPaths(profileDir.EnumerateFiles("*.json")
                .Where(file => file.Name.IndexOf("union", StringComparison.OrdinalIgnoreCase) < 0)
                .Select(file => file.FullName));

            string unionId = GetUnionIdFromProfiles(profiles);
            string unionPath = Path.Combine(profileDir.FullName, unionId + ".json");

            return _profileCache.GetOrAdd(unionPath, new Lazy<CompatibilityProfileCacheEntry>(() => {
                try
                {
                    // We read the ID first to avoid needing to rehydrate MBs of unneeded JSON
                    if (JsonProfileSerializer.ReadIdFromProfileFile(unionPath) == unionId)
                    {
                        CompatibilityProfileDataMut loadedUnionProfile = _jsonSerializer.DeserializeFromFile(unionPath);

                        // This is unlikely, but the ID has limited entropy
                        if (UnionMatchesProfiles(loadedUnionProfile, profiles))
                        {
                            return new CompatibilityProfileCacheEntry(
                                loadedUnionProfile,
                                new CompatibilityProfileData(loadedUnionProfile));
                        }
                    }

                    // We found the union file, but it didn't match for some reason
                    File.Delete(unionPath);
                }
                catch (Exception)
                {
                    // Do nothing, we will now generate the profile
                }

                // Loading the union file failed, so we are forced to generate it
                CompatibilityProfileDataMut generatedUnionProfile = ProfileCombination.UnionMany(unionId, profiles.Select(p => p.MutableProfileData));

                // Write the union to the filesystem for faster startup later
                Task.Run(() => {
                    _jsonSerializer.SerializeToFile(generatedUnionProfile, unionPath);
                });

                return new CompatibilityProfileCacheEntry(
                    generatedUnionProfile,
                    new CompatibilityProfileData(generatedUnionProfile));
            })).Value.Profile;
        }

        private static string GetUnionIdFromProfiles(IEnumerable<CompatibilityProfileCacheEntry> profiles)
        {
            // Build an order-independent hashcode
            int hash = 0;
            foreach (CompatibilityProfileCacheEntry profile in profiles)
            {
                unchecked
                {
                    hash += GetDeterministicIdCode(profile.Profile.Id);
                }
            }

            // Return a hex representation of the hashcode
            return "union_" + hash.ToString("x");
        }

        private static int GetDeterministicIdCode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return 0;
            }

            byte[] idBytes = Encoding.UTF8.GetBytes(id);
            int code = 0;
            for (int i = 0; i < idBytes.Length; i += 4)
            {
                int currInt;

                int remainingBytes = idBytes.Length - i;
                if (remainingBytes >= 4)
                {
                    currInt = BitConverter.ToInt32(idBytes, i);
                }
                else
                {
                    var lastBytes = new byte[4];
                    for (int j = 0; j < 4; j++)
                    {
                        lastBytes[j] = j < remainingBytes
                            ? idBytes[i + j]
                            : (byte)0;
                    }
                    currInt = BitConverter.ToInt32(lastBytes, 0);
                }

                unchecked
                {
                    code += currInt;
                }
            }

            return code;
        }

        private static bool UnionMatchesProfiles(
            CompatibilityProfileDataMut unionProfile,
            IEnumerable<CompatibilityProfileCacheEntry> profiles)
        {
            var idsToSee = new HashSet<string>(unionProfile.ConstituentProfiles);
            foreach (CompatibilityProfileCacheEntry profile in profiles)
            {
                // Check that every ID is in the list
                if (!idsToSee.Remove(profile.Profile.Id))
                {
                    return false;
                }
            }

            // Check that there are no other IDs in the profile
            return idsToSee.Count == 0;
        }

        private class CompatibilityProfileCacheEntry
        {
            public CompatibilityProfileCacheEntry(
                CompatibilityProfileDataMut mutableProfileData,
                CompatibilityProfileData profile)
            {
                MutableProfileData = mutableProfileData;
                Profile = profile;
            }

            public CompatibilityProfileDataMut MutableProfileData { get; }

            public CompatibilityProfileData Profile { get; }
        }
    }
}