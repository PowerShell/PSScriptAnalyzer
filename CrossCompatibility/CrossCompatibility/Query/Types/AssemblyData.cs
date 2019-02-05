// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AssemblyDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.AssemblyData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class AssemblyData
    {
        public AssemblyData(AssemblyDataMut assemblyData)
        {
            AssemblyName = new AssemblyNameData(assemblyData.AssemblyName);
            Types = assemblyData.Types?.ToDictionary(ns => ns.Key, ns => (IReadOnlyDictionary<string, TypeData>)ns.Value.ToDictionary(t => t.Key, t => new TypeData(t.Key, t.Value), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        }

        public AssemblyNameData AssemblyName { get; }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, TypeData>> Types { get; }
    }
}