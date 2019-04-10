// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Holds a structured form of a full assembly name.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AssemblyNameData : ICloneable
    {
        /// <summary>
        /// The simple name of the assembly.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Version Version { get; set; }

        /// <summary>
        /// The culture of the assembly.
        /// This should not be null, but null
        /// should be considered the same as "neutral".
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Culture { get; set; }

        /// <summary>
        /// The public key token of the assembly.
        /// This may be null if the assembly has no public
        /// key token (i.e. is unsigned).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public byte[] PublicKeyToken { get; set; }

        /// <summary>
        /// Create a deep clone of the assembly name data object.
        /// </summary>
        public object Clone()
        {
            return new AssemblyNameData()
            {
                Name = Name,
                Version = Version,
                Culture = Culture,
                PublicKeyToken = (byte[])PublicKeyToken?.Clone()
            };
        }
    }
}
