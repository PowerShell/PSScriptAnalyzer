using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Describes a .NET assembly available in PowerShell.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AssemblyData
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
        public IDictionary<string, IDictionary<string, TypeData>> Types { get; set; }
    }
}