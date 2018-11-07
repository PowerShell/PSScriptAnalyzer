using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Platform
{
    /// <summary>
    /// Denotes a machine architecture.
    /// Intended to align with System.Runtime.InteropServices.Architecture.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum Architecture
    {
        /// <summary>
        /// Denotes the 32-bit Intel architecture.
        /// </summary>
        [EnumMember]
        X86 = 0,

        /// <summary>
        /// Denotes the 64-bit Intel architecture.
        /// AKA x86_64 or amd64.
        /// </summary>
        [EnumMember]
        X64 = 1,

        /// <summary>
        /// Denotes the 32-bit ARM architecture.
        /// AKA arm32.
        /// </summary>
        [EnumMember]
        Arm = 2,

        /// <summary>
        /// Denotes the 64-bit ARM architecture.
        /// </summary>
        [EnumMember]
        Arm64 = 3,
    }
}