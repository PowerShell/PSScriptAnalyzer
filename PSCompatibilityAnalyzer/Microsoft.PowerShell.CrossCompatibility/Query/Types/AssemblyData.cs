// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for .NET assembly data.
    /// </summary>
    public class AssemblyData
    {
        private readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>>> _types;

        /// <summary>
        /// Create a query object for assembly data from collected assembly data.
        /// </summary>
        /// <param name="assemblyData">Collected assembly data.</param>
        public AssemblyData(Data.Types.AssemblyData assemblyData)
        {
            AssemblyName = new AssemblyNameData(assemblyData.AssemblyName);

            if (assemblyData.Types != null)
            {
                _types = new Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>>>(() => CreateTypeDictionary(assemblyData.Types));
            }
        }

        /// <summary>
        /// The name of the assembly, broken into constituent parts.
        /// </summary>
        public AssemblyNameData AssemblyName { get; }

        /// <summary>
        /// Lookup table of types in the assembly, keyed by namespace and then type name.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> Types => _types?.Value;

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> CreateTypeDictionary(IReadOnlyDictionary<string, JsonDictionary<string, Data.Types.TypeData>> typeData)
        {
            var namespaceDict = new Dictionary<string, IReadOnlyDictionary<string, TypeData>>(typeData.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JsonDictionary<string, Data.Types.TypeData>> nspace in typeData)
            {
                var typeDict = new Dictionary<string, TypeData>(nspace.Value.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Data.Types.TypeData> type in nspace.Value)
                {
                    typeDict[type.Key] = new TypeData(type.Key, type.Value);
                }
                namespaceDict[nspace.Key] = typeDict;
            }
            return namespaceDict;
        }
    }
}
