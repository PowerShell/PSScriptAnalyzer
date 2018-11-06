using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Platform
{
    /// <summary>
    /// Denotes the broad grouping
    /// of operating systems a particular
    /// PowerShell host OS may fall under.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum OSFamily
    {
        /// <summary>An unrecognized operating system.</summary>
        [EnumMember]
        Other = 0,

        /// <summary>A Windows operating system.</summary>
        [EnumMember]
        Windows,

        /// <summary>A macOS operating system.</summary>
        [EnumMember]
        MacOS,
        
        /// <summary>A Linux operating system.</summary>
        [EnumMember]
        Linux,
    }

    /// <summary>
    /// Describes an operating system
    /// that hosts a PowerShell runtime.
    /// </summary>
    [Serializable]
    [DataContract]
    public class OperatingSystemData
    {
	/// <summary>
	/// The name of the operating system as
	/// reported by $PSVersionTable.
	/// </summary>
        [DataMember]
        public string Name { get; set; }

	/// <summary>
	/// The platform as reported by
	/// $PSVersionTable.
	/// </summary>
        [DataMember]
        public string Platform { get; set; }

	/// <summary>
	/// The broad kind of operating system
	/// the target PowerShell runtime runs on.
	/// </summary>
        [DataMember]
        public OSFamily Family { get; set; }

	/// <summary>
	/// The version of the operating system.
	/// On Windows and macOS this is given by
	/// System.Environment.OSVersion.Version.
	/// On Linux, this takes the output of uname -r
	/// to track the kernel SKU.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Version { get; set; }

	/// <summary>
	/// If specified, the Windows Service Pack
	/// of the operating system.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ServicePack { get; set; }

	/// <summary>
	/// On Linux, the name of the distribution
	/// family (e.g. "Ubuntu"). Taken from
	/// "ID" in /etc/*-release
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DistributionId { get; set; }

	/// <summary>
	/// On Linux, the version of the particular
	/// distribtion. Taken from "VERSION_ID" in
	/// /etc/*-release.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DistributionVersion { get; set; }

	/// <summary>
	/// On Linux, the full name of the distribution
	/// version. Taken from "PRETTY_NAME" in /etc/*-release.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DistributionPrettyName { get; set; }
    }
}
