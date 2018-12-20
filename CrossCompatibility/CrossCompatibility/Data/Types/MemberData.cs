using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Describes either the instance or static members on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MemberData : ICloneable
    {
        /// <summary>
        /// Array of overloads of the constructors
        /// of the type. Each element is an array of the type
        /// names of the constructor parameters in order.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[][] Constructors { get; set; }

        /// <summary>
        /// Fields on the type, keyed by field name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, FieldData> Fields { get; set; }

        /// <summary>
        /// Properties on this type, keyed by property name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, PropertyData> Properties { get; set; }

        /// <summary>
        /// Indexers on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IndexerData[] Indexers { get; set; }

        /// <summary>
        /// Methods on the type, keyed by method name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, MethodData> Methods { get; set; }

        /// <summary>
        /// Events on the type, keyed by event name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, EventData> Events { get; set; }

        /// <summary>
        /// Types nested within the type, keyed by type name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, TypeData> NestedTypes { get; set; }

        public object Clone()
        {
            return new MemberData()
            {
                Constructors = Constructors?.Select(c => (string[])c.Clone()).ToArray(),
                Events = Events?.ToDictionary(e => e.Key, e => (EventData)e.Value.Clone()),
                Fields = Fields?.ToDictionary(f => f.Key, f => (FieldData)f.Value.Clone()),
                Indexers = Indexers?.Select(i => (IndexerData)i.Clone()).ToArray(),
                Methods = Methods?.ToDictionary(m => m.Key, m => (MethodData)m.Value.Clone()),
                NestedTypes = NestedTypes?.ToDictionary(t => t.Key, t => (TypeData)t.Value.Clone()),
                Properties = Properties?.ToDictionary(p => p.Key, p => (PropertyData)p.Value.Clone())
            };
        }
    }
}