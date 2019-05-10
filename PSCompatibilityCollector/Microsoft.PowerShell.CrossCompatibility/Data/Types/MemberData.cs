// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
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
        public JsonDictionary<string, FieldData> Fields { get; set; }

        /// <summary>
        /// Properties on this type, keyed by property name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, PropertyData> Properties { get; set; }

        /// <summary>
        /// Indexers on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IndexerData[] Indexers { get; set; }

        /// <summary>
        /// Methods on the type, keyed by method name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, MethodData> Methods { get; set; }

        /// <summary>
        /// Events on the type, keyed by event name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, EventData> Events { get; set; }

        /// <summary>
        /// Types nested within the type, keyed by type name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, TypeData> NestedTypes { get; set; }

        /// <summary>
        /// Create a deep clone of the member data object.
        /// </summary>
        public object Clone()
        {
            return new MemberData()
            {
                Constructors = Constructors?.Select(c => (string[])c.Clone()).ToArray(),
                Events = (JsonDictionary<string, EventData>)Events?.Clone(),
                Fields = (JsonDictionary<string, FieldData>)Fields?.Clone(),
                Indexers = Indexers?.Select(i => (IndexerData)i.Clone()).ToArray(),
                Methods = (JsonDictionary<string, MethodData>)Methods?.Clone(),
                NestedTypes = (JsonDictionary<string, TypeData>)NestedTypes?.Clone(),
                Properties = (JsonDictionary<string, PropertyData>)Properties?.Clone(),
            };
        }
    }
}
