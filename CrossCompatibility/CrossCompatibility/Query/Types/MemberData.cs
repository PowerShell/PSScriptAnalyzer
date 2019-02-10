// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using MemberDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.MemberData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a set of members on a .NET type.
    /// MemberData objects collect either static or instance members available on a type.
    /// </summary>
    public class MemberData
    {
        private readonly MemberDataMut _memberData;

        /// <summary>
        /// Create a new query object around a collected .NET member data.
        /// </summary>
        /// <param name="memberData">The collected .NET member data.</param>
        public MemberData(MemberDataMut memberData)
        {
            _memberData = memberData;
            Fields = memberData.Fields?.ToDictionary(f => f.Key, f => new FieldData(f.Key, f.Value), StringComparer.OrdinalIgnoreCase);
            Properties = memberData.Properties?.ToDictionary(p => p.Key, p => new PropertyData(p.Key, p.Value), StringComparer.OrdinalIgnoreCase);
            Indexers = memberData.Indexers?.Select(i => new IndexerData(i)).ToArray();
            Events = memberData.Events?.ToDictionary(e => e.Key, e => new EventData(e.Key, e.Value), StringComparer.OrdinalIgnoreCase);
            NestedTypes = memberData.NestedTypes?.ToDictionary(t => t.Key, t => new TypeData(t.Key, t.Value), StringComparer.OrdinalIgnoreCase);
            Methods = memberData.Methods?.ToDictionary(m => m.Key, m => new MethodData(m.Key, m.Value), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Constructor overloads on the type, arranged as an array of arrays of full type names.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Constructors => _memberData.Constructors;

        /// <summary>
        /// Lookup table of all fields on the type.
        /// </summary>
        public IReadOnlyDictionary<string, FieldData> Fields { get; }

        /// <summary>
        /// Lookup table of all properties on the type.
        /// </summary>
        public IReadOnlyDictionary<string, PropertyData> Properties { get; }

        /// <summary>
        /// Lookup table of all methods on the type.
        /// </summary>
        public IReadOnlyDictionary<string, MethodData> Methods { get; }

        /// <summary>
        /// List of all indexers on the type.
        /// </summary>
        public IReadOnlyList<IndexerData> Indexers { get; }

        /// <summary>
        /// Lookup table of all events on the type.
        /// </summary>
        public IReadOnlyDictionary<string, EventData> Events { get; }

        /// <summary>
        /// Lookup table of all nested types in the type.
        /// </summary>
        public IReadOnlyDictionary<string, TypeData> NestedTypes { get; }
    }
}