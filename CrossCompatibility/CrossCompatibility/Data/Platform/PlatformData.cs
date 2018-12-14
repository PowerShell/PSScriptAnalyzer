using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Platform
{
    /// <summary>
    /// Describes the platform PowerShell runs on.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PlatformData : ICloneable
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
        /// .NET/CLR host runtime information.
        /// </summary>
        [DataMember(Name = ".NET")]
        public DotnetData Dotnet { get; set; }

        public object Clone()
        {
            return new PlatformData()
            {
                Dotnet = Dotnet.DeepClone(),
                OperatingSystem = (OperatingSystemData)OperatingSystem.Clone(),
                PowerShell = (PowerShellData)PowerShell.Clone()
            };
        }
    }
}
