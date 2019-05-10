// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a property on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PropertyData : ICloneable
    {
        /// <summary>
        /// Lists the accessors available on this property.
        /// </summary>
        [DataMember]
        public AccessorType[] Accessors { get; set; }

        /// <summary>
        /// The full name of the type of the property.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Create deep clone of the property data object.
        /// </summary>
        public object Clone()
        {
            return new PropertyData()
            {
                Accessors = (AccessorType[])Accessors.Clone(),
                Type = Type
            };
        }
    }
}
