// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a .NET assembly available in PowerShell.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AssemblyData : ICloneable
    {
        /// <summary>
        /// Holds a structured description of the assembly name.
        /// </summary>
        [DataMember]
        public AssemblyNameData AssemblyName { get; set; }

        /// <summary>
        /// Describes all the types publicly exposed by an assembly, keyed by namespace
        /// and then type name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, JsonDictionary<string, TypeData>> Types { get; set; }

        /// <summary>
        /// Create a deep clone of the assembly data object.
        /// </summary>
        public object Clone()
        {
            return new AssemblyData()
            {
                AssemblyName = (AssemblyNameData)AssemblyName.Clone(),
                Types = (JsonDictionary<string, JsonDictionary<string, TypeData>>)Types?.Clone()
            };
        }
    }
}
