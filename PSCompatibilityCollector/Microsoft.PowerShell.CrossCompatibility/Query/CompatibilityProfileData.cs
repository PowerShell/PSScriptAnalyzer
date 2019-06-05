// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CompatibilityProfileDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CompatibilityProfileData;
using RuntimeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.RuntimeData;
using PlatformDataMut = Microsoft.PowerShell.CrossCompatibility.Data.ModuleData;
using Microsoft.PowerShell.CrossCompatibility.Query;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell platform data,
    /// together forming a profile of PowerShell on some platform.
    /// Catalogues the platform PowerShell runs on, and what types and commands
    /// are available to PowerShell on that platform.
    /// </summary>
    public class CompatibilityProfileData
    {
        /// <summary>
        /// Create a query object around a collected compatibility profile.
        /// </summary>
        /// <param name="compatibilityProfileData">The collected compatibility profile data.</param>
        public CompatibilityProfileData(CompatibilityProfileDataMut compatibilityProfileData)
        {
            Runtime = new RuntimeData(compatibilityProfileData.Runtime);

            Id = compatibilityProfileData.Id;

            if (compatibilityProfileData.ConstituentProfiles != null)
            {
                // This is intended to be case-sensitive
                ConstituentProfiles = new ReadOnlySet<string>(compatibilityProfileData.ConstituentProfiles);
            }

            // This should only be null in the case of the anyplatform_union profile
            if (compatibilityProfileData.Platform != null)
            {
                Platform = new PlatformData(compatibilityProfileData.Platform);
            }
        }

        /// <summary>
        /// The unique identifier for this profile.
        /// Used to identify whether it is included in a profile union.
        /// For a generated profile, this is the platform name,
        /// but may be specified by the user for a custom profile.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// In a union profile, lists all the profiles used to form it.
        /// </summary>
        public ReadOnlySet<string> ConstituentProfiles { get; }

        /// <summary>
        /// Information about types and commands available in the PowerShell runtime
        /// by default on this platform.
        /// </summary>
        public RuntimeData Runtime { get; }

        /// <summary>
        /// Information about the platform on which this PowerShell runtime is running.
        /// </summary>
        public PlatformData Platform { get; }
    }
}
