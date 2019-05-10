// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TypeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.TypeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for metadata on a .NET type.
    /// </summary>
    public class TypeData
    {
        /// <summary>
        /// Create a new type query object from collected .NET type data.
        /// </summary>
        /// <param name="name">The simple (non-namespace qualified) name of the type.</param>
        /// <param name="typeData">Collected type data.</param>
        public TypeData(string name, TypeDataMut typeData)
        {
            Name = name;
            IsEnum = typeData.IsEnum;
            Instance = typeData.Instance == null ? null : new MemberData(typeData.Instance);
            Static = typeData.Static == null ? null : new MemberData(typeData.Static);
        }

        /// <summary>
        /// The simple, non-namespace-qualified name of the type.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if the type is an enum, false otherwise.
        /// </summary>
        public bool IsEnum { get; }

        /// <summary>
        /// All instance members of the type.
        /// </summary>
        public MemberData Instance { get; }

        /// <summary>
        /// All static members of the type.
        /// </summary>
        public MemberData Static { get; }
    }
}
