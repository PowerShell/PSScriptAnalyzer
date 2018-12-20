using System;
using System.Linq;
using System.Runtime.Serialization;
using CrossCompatibility.Common;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
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
        public JsonDictionary<string, TypeAcceleratorData> TypeAccelerators { get; set; }

        /// <summary>
        /// Table of all assemblies available in the PowerShell runtime,
        /// keyed by simple assembly name.
        /// </summary>
        [DataMember]
        public JsonDictionary<string, AssemblyData> Assemblies { get; set; }

        public object Clone()
        {
            return new AvailableTypeData()
            {
                TypeAccelerators = (JsonDictionary<string, TypeAcceleratorData>)TypeAccelerators.Clone(),
                Assemblies = (JsonDictionary<string, AssemblyData>)Assemblies.Clone()
            };
        }
    }
}