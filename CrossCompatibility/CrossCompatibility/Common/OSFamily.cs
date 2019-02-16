// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility
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
}
