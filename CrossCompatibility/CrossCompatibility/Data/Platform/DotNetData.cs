using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Platform
{
    /// <summary>
    /// Denotes a particular .NET runtime that
    /// PowerShell supports.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum DotnetRuntime
    {
        /// <summary>An unrecognized .NET runtime.</summary>
        [EnumMember]
        Other = 0,

        /// <summary>The .NET Framework runtime.</summary>
        [EnumMember]
        Framework,

        /// <summary>The .NET Core runtime.</summary>
        [EnumMember]
        Core,
    }

    /// <summary>
    /// Describes a .NET runtime on which PowerShell runs.
    /// </summary>
    [Serializable]
    [DataContract]
    public class DotnetData
    {
        /// <summary>
        /// The version of the .NET core language runtime
        /// reported by the target PowerShell platform.
        /// </summary>
        [DataMember]
        public Version ClrVersion { get; set; }

        /// <summary>
        /// The .NET runtime PowerShell is running on.
        /// </summary>
        [DataMember]
        public DotnetRuntime Runtime { get; set; }
    }
}