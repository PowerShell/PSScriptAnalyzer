// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// A report generated on a particular PowerShell platform
    /// describing the platform and what commands and types are
    /// available on that platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CompatibilityProfileData : ICloneable
    {
        /// <summary>
        /// The identifying name of this profile, for unique reference.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// The verison of the profile schema this profile object uses.
        /// </summary>
        [DataMember]
        public Version ProfileSchemaVersion { get; set; }

        /// <summary>
        /// For an aggregate profile, the IDs of all the profiles used to make it.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] ConstituentProfiles { get; set; }

        /// <summary>
        /// Describes the what types and commands are available
        /// on the target platform.
        /// </summary>
        [DataMember]
        public RuntimeData Runtime { get; set; }

        /// <summary>
        /// Describes the target platform on which a PowerShell script
        /// will run, including the PowerShell installation,
        /// the .NET runtime and the operating system environment.
        /// </summary>
        [DataMember]
        public PlatformData Platform { get; set; }

        public object Clone()
        {
            return new CompatibilityProfileData()
            {
                Runtime = (RuntimeData)Runtime.Clone(),
                Platform = (PlatformData)Platform.Clone()
            };
        }
    }
}
