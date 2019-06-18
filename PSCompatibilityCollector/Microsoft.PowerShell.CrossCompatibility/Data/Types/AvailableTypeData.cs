// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Aggregator to hold all type data and the type accelerator
    /// lookup table for a given PowerShell platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AvailableTypeData : ICloneable
    {
        /// <summary>
        /// The type accelerator lookup table, with type accelerators
        /// as keys and their corresponding full type name as values.
        /// </summary>
        [DataMember]
        public JsonCaseInsensitiveStringDictionary<TypeAcceleratorData> TypeAccelerators { get; set; }

        /// <summary>
        /// Table of all assemblies available in the PowerShell runtime,
        /// keyed by simple assembly name.
        /// </summary>
        [DataMember]
        public JsonDictionary<string, AssemblyData> Assemblies { get; set; }

        /// <summary>
        /// Create a deep clone of the available type data object.
        /// </summary>
        public object Clone()
        {
            return new AvailableTypeData()
            {
                TypeAccelerators = (JsonCaseInsensitiveStringDictionary<TypeAcceleratorData>)TypeAccelerators.Clone(),
                Assemblies = (JsonDictionary<string, AssemblyData>)Assemblies.Clone()
            };
        }
    }
}
