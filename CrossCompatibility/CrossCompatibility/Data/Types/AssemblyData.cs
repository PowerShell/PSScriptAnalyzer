using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
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
        public IDictionary<string, IDictionary<string, TypeData>> Types { get; set; }

        public object Clone()
        {
            return new AssemblyData()
            {
                AssemblyName = (AssemblyNameData)AssemblyName.Clone(),
                Types = Types?.ToDictionary(ns => ns.Key, ns => (IDictionary<string, TypeData>)ns.Value.ToDictionary(t => t.Key ?? throw new Exception ($"Null typename for {t}"), t => (TypeData)t.Value.Clone()))
            };
        }
    }
}