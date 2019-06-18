// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a native application or util command available on a PowerShell platform.
    /// </summary>
    [DataContract]
    public class NativeCommandData : ICloneable
    {
        /// <summary>
        /// The version of the application given, if the information is available.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Version Version { get; set; }

        /// <summary>
        /// The path where the application can be found.
        /// </summary>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Create a deep clone of the native command data object.
        /// </summary>
        public object Clone()
        {
            return new NativeCommandData()
            {
                Version = Version,
                Path = Path
            };
        }
    }
}
