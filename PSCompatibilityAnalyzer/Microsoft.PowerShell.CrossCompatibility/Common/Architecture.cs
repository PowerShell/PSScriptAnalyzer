// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility
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
