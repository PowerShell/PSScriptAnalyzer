// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes an operating system
    /// that hosts a PowerShell runtime.
    /// </summary>
    [Serializable]
    [DataContract]
    public class OperatingSystemData : ICloneable
    {
	/// <summary>
	/// The name of the operating system.
	/// </summary>
        [DataMember]
        public string Name { get; set; }

	/// <summary>
	/// The description of the operating system as
	/// reported by $PSVersionTable.
	/// </summary>
        [DataMember]
        public string Description { get; set; }

	/// <summary>
	/// The platform as reported by
	/// $PSVersionTable.
	/// </summary>
        [DataMember]
        public string Platform { get; set; }

        /// <summary>
        /// The OS machine architecture.
        /// From System.Runtime.InteropServices.RuntimeInformation.OSArchitecture
        /// in .NET Core. Either X64 or X86 in .NET Framework.
        /// </summary>
        [DataMember]
        public Architecture Architecture { get; set; }

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
        /// The Windows SKU identifier, corresponding to
        /// the GetProductInfo() sysinfo API:
        /// https://docs.microsoft.com/en-us/windows/desktop/api/sysinfoapi/nf-sysinfoapi-getproductinfo
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public uint? SkuId { get; set; }

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

        /// <summary>
        /// Create a deep clone of the operating system data object.
        /// </summary>
        public object Clone()
        {
            return new OperatingSystemData()
            {
                Architecture = Architecture,
                DistributionId = DistributionId,
                DistributionPrettyName = DistributionPrettyName,
                DistributionVersion = DistributionVersion,
                Family = Family,
                Name = Name,
                Platform = Platform,
                ServicePack = ServicePack,
                SkuId = SkuId,
                Version = Version
            };
        }
    }
}
