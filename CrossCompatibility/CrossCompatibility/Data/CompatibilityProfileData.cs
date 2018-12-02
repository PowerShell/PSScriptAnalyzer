using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// A report generated on a particular PowerShell platform
    /// describing the platform and what commands and types are
    /// available on that platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CompatibilityProfileData
    {
        /// <summary>
        /// Describes the what types and commands are available
        /// on the target platform.
        /// </summary>
        [DataMember]
        public RuntimeData Compatibility { get; set; }

        /// <summary>
        /// Describes the target platforms on which a PowerShell script
        /// will run, including the PowerShell installation,
        /// the .NET runtime and the operating system environment.
        /// </summary>
        [DataMember]
        public PlatformData[] Platforms { get; set; }

        public CompatibilityProfileData DeepClone()
        {
            return new CompatibilityProfileData()
            {
                Compatibility = Compatibility.DeepClone(),
                Platforms = Platforms.Select(p => p.DeepClone()).ToArray()
            };
        }
    }
}