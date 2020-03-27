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
        /// <summary>
        /// Create a query object for assembly data from collected assembly data.
        /// </summary>
        /// <param name="assemblyData">Collected assembly data.</param>
        public AssemblyData(Data.AssemblyData assemblyData)
        {
            AssemblyName = new AssemblyNameData(assemblyData.AssemblyName);

            if (assemblyData.Types != null)
            {
                Types = CreateTypeDictionary(assemblyData.Types);
            }
        }

        /// <summary>
        /// The name of the assembly, broken into constituent parts.
        /// </summary>
        public AssemblyNameData AssemblyName { get; }

        /// <summary>
        /// Lookup table of types in the assembly, keyed by namespace and then type name.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> Types { get; }

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> CreateTypeDictionary(IReadOnlyDictionary<string, JsonDictionary<string, Data.TypeData>> typeData)
        {
            var namespaceDict = new Dictionary<string, IReadOnlyDictionary<string, TypeData>>(typeData.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JsonDictionary<string, Data.TypeData>> nspace in typeData)
            {
                var typeDict = new Dictionary<string, TypeData>(nspace.Value.Count, StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Data.TypeData> type in nspace.Value)
                {
                    typeDict[type.Key] = new TypeData(type.Key, type.Value);
                }
                namespaceDict[nspace.Key] = typeDict;
            }
            return namespaceDict;
        }
    }
}
