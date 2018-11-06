using System;
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
    public class CompatibilityReportData
    {
        /// <summary>
        /// Describes the what types and commands are available
        /// on the target platform.
        /// </summary>
        [DataMember]
        public CompatibilityData Compatibility { get; set; }

        /// <summary>
        /// Describes a target platform on which a PowerShell script
        /// will run, including the PowerShell installation,
        /// the .NET runtime and the operating system environment.
        /// </summary>
        [DataMember]
        public PlatformData Platform { get; set; }
    }
}