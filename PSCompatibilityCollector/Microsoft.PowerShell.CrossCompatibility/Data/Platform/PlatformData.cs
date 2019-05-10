// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
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
        [DataMember]
        public DotnetData Dotnet { get; set; }

        /// <summary>
        /// Create a deep clone of the platform data object.
        /// </summary>
        public object Clone()
        {
            return new PlatformData()
            {
                Dotnet = (DotnetData)Dotnet.Clone(),
                OperatingSystem = (OperatingSystemData)OperatingSystem.Clone(),
                PowerShell = (PowerShellData)PowerShell.Clone()
            };
        }
    }
}
