using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Platform
{
    /// <summary>
    /// Describes the platform PowerShell runs on.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlatformData
    {
	/// <summary>
	/// Host operating system information.
	/// </summary>
        [DataMember]
        public OperatingSystemData OperatingSystem { get; set; }

	/// <summary>
	/// PowerShell version information.
	/// </summary>
        [DataMember]
        public PowerShellData PowerShell { get; set; }

	/// <summary>
	/// Host machine information.
	/// </summary>
        [DataMember]
        public MachineData Machine { get; set; }

	/// <summary>
	/// .NET/CLR host runtime information.
	/// </summary>
        [DataMember(Name = ".NET")]
        public DotNetData DotNet { get; set; }
    }
}
