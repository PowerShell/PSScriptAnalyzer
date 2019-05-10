// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a PowerShell type accelerator.
    /// </summary>
    [Serializable]
    [DataContract]
    public class TypeAcceleratorData : ICloneable
    {
        /// <summary>
        /// Describes which assembly the type in the accelerator comes from.
        /// </summary>
        [DataMember]
        public string Assembly { get; set; }

        /// <summary>
        /// The full name of the type the accelerator points to.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Create a deep clone of the type accelerator data object.
        /// </summary>
        public object Clone()
        {
            return new TypeAcceleratorData()
            {
                Assembly = Assembly,
                Type = Type
            };
        }
    }
}
