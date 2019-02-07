// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyData;

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
        public AssemblyData(AssemblyDataMut assemblyData)
        {
            AssemblyName = new AssemblyNameData(assemblyData.AssemblyName);
            Types = assemblyData.Types?.ToDictionary(ns => ns.Key, ns => (IReadOnlyDictionary<string, TypeData>)ns.Value.ToDictionary(t => t.Key, t => new TypeData(t.Key, t.Value), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The name of the assembly, broken into constituent parts.
        /// </summary>
        public AssemblyNameData AssemblyName { get; }

        /// <summary>
        /// Lookup table of types in the assembly, keyed by namespace and then type name.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> Types { get; }
    }
}