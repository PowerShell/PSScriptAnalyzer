// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a set of members on a .NET type.
    /// MemberData objects collect either static or instance members available on a type.
    /// </summary>
    public class MemberData
    {
        private readonly Lazy<IReadOnlyDictionary<string, FieldData>> _fields;

        private readonly Lazy<IReadOnlyDictionary<string, PropertyData>> _properties;

        private readonly Lazy<IReadOnlyDictionary<string, MethodData>> _methods;

        private readonly Lazy<IReadOnlyDictionary<string, EventData>> _events;

        private readonly Lazy<IReadOnlyDictionary<string, TypeData>> _nestedtypes;

        private readonly Lazy<IReadOnlyList<IndexerData>> _indexers;

        /// <summary>
        /// Create a new query object around a collected .NET member data.
        /// </summary>
        /// <param name="memberData">The collected .NET member data.</param>
        public MemberData(Data.MemberData memberData)
        {
            if (memberData.Constructors != null)
            {
                Constructors = memberData.Constructors;
            }

            if (memberData.Fields != null)
            {
                _fields = new Lazy<IReadOnlyDictionary<string, FieldData>>(() => CreateFieldTable(memberData.Fields));
            }

            if (memberData.Properties != null)
            {
                _properties = new Lazy<IReadOnlyDictionary<string, PropertyData>>(() => CreatePropertyTable(memberData.Properties));
            }

            if (memberData.Methods != null)
            {
                _methods = new Lazy<IReadOnlyDictionary<string, MethodData>>(() => CreateMethodTable(memberData.Methods));
            }

            if (memberData.Events != null)
            {
                _events = new Lazy<IReadOnlyDictionary<string, EventData>>(() => CreateEventTable(memberData.Events));
            }

            if (memberData.NestedTypes != null)
            {
                _nestedtypes = new Lazy<IReadOnlyDictionary<string, TypeData>>(() => CreateNestedTypeTable(memberData.NestedTypes));
            }

            if (memberData.Indexers != null)
            {
                _indexers = new Lazy<IReadOnlyList<IndexerData>>(() => CreateIndexerList(memberData.Indexers));
            }
        }

        /// <summary>
        /// Constructor overloads on the type, arranged as an array of arrays of full type names.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Constructors { get; }

        /// <summary>
        /// Lookup table of all fields on the type.
        /// </summary>
        public IReadOnlyDictionary<string, FieldData> Fields => _fields?.Value;

        /// <summary>
        /// Lookup table of all properties on the type.
        /// </summary>
        public IReadOnlyDictionary<string, PropertyData> Properties => _properties?.Value;

        /// <summary>
        /// Lookup table of all methods on the type.
        /// </summary>
        public IReadOnlyDictionary<string, MethodData> Methods => _methods?.Value;

        /// <summary>
        /// List of all indexers on the type.
        /// </summary>
        public IReadOnlyList<IndexerData> Indexers => _indexers?.Value;

        /// <summary>
        /// Lookup table of all events on the type.
        /// </summary>
        public IReadOnlyDictionary<string, EventData> Events => _events?.Value;

        /// <summary>
        /// Lookup table of all nested types in the type.
        /// </summary>
        public IReadOnlyDictionary<string, TypeData> NestedTypes => _nestedtypes?.Value;

        private static IReadOnlyDictionary<string, FieldData> CreateFieldTable(IReadOnlyDictionary<string, Data.FieldData> fields)
        {
            var dict = new Dictionary<string, FieldData>(fields.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.FieldData> field in fields)
            {
                dict[field.Key] = new FieldData(field.Key, field.Value);
            }
            return dict;
        }

        private static IReadOnlyDictionary<string, PropertyData> CreatePropertyTable(IReadOnlyDictionary<string, Data.PropertyData> properties)
        {
            var dict = new Dictionary<string, PropertyData>(properties.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.PropertyData> property in properties)
            {
                dict[property.Key] = new PropertyData(property.Key, property.Value);
            }
            return dict;
        }

        private static IReadOnlyDictionary<string, MethodData> CreateMethodTable(IReadOnlyDictionary<string, Data.MethodData> methods)
        {
            var dict = new Dictionary<string, MethodData>(methods.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.MethodData> method in methods)
            {
                dict[method.Key] = new MethodData(method.Key, method.Value);
            }
            return dict;
        }

        private static IReadOnlyDictionary<string, EventData> CreateEventTable(IReadOnlyDictionary<string, Data.EventData> events)
        {
            var dict = new Dictionary<string, EventData>(events.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.EventData> e in events)
            {
                dict[e.Key] = new EventData(e.Key, e.Value);
            }
            return dict;
        }

        private static IReadOnlyDictionary<string, TypeData> CreateNestedTypeTable(IReadOnlyDictionary<string, Data.TypeData> nestedTypes)
        {
            var dict = new Dictionary<string, TypeData>(nestedTypes.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.TypeData> type in nestedTypes)
            {
                dict[type.Key] = new TypeData(type.Key, type.Value);
            }
            return dict;
        }

        private static IReadOnlyList<IndexerData> CreateIndexerList(IReadOnlyList<Data.IndexerData> indexers)
        {
            var indexerList = new IndexerData[indexers.Count];
            for (int i = 0; i < indexerList.Length; i++)
            {
                indexerList[i] = new IndexerData(indexers[i]);
            }
            return indexerList;
        }
    }
}
