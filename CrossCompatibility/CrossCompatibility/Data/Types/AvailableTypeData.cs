using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Aggregator to hold all type data and the type accelerator
    /// lookup table for a given PowerShell platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AvailableTypeData
    {
        /// <summary>
        /// The type accelerator lookup table, with type accelerators
        /// as keys and their corresponding full type name as values.
        /// </summary>
        [DataMember]
        public IDictionary<string, string> TypeAccelerators { get; set; }

        /// <summary>
        /// Table of all assemblies available in the PowerShell runtime,
        /// keyed by simple assembly name.
        /// </summary>
        [DataMember]
        public IDictionary<string, AssemblyData> Assemblies { get; set; }
    }
}