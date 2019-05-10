// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes all the members on a .NET type,
    /// broken up into static and instance members.
    /// </summary>
    [Serializable]
    [DataContract]
    public class TypeData : ICloneable
    {
        [DataMember(EmitDefaultValue = false)]
        public bool IsEnum { get; set; }

        /// <summary>
        /// The static members on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MemberData Static { get; set; }

        /// <summary>
        /// The instance members on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MemberData Instance { get; set; }

        /// <summary>
        /// Create a deep clone of the type data object.
        /// </summary>
        public object Clone()
        {
            return new TypeData()
            {
                Static = (MemberData)Static?.Clone(),
                Instance = (MemberData)Instance?.Clone()
            };
        }
    }
}
