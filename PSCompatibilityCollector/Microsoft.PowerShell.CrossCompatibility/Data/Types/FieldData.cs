// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a field on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FieldData : ICloneable
    {
        /// <summary>
        /// The type of the field.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Create a deep clone of the field data object.
        /// </summary>
        public object Clone()
        {
            return new FieldData()
            {
                Type = Type
            };
        }
    }
}
