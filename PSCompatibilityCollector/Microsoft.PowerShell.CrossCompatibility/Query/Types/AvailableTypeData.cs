// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for all .NET type information in a PowerShell runtime.
    /// </summary>
    public class AvailableTypeData
    {
        /// <summary>
        /// Create a new query object around collected .NET type information.
        /// </summary>
        /// <param name="availableTypeData">The .NET type data object to query.</param>
        public AvailableTypeData(Data.AvailableTypeData availableTypeData)
        {
            Assemblies = CreateAssemblyTable(availableTypeData.Assemblies);
            TypeAccelerators = CreateTypeAcceleratorTables(availableTypeData.TypeAccelerators, out IReadOnlyDictionary<string, string> typeAcceleratorNames);
            Types = CreateTypeLookupTable(Assemblies.Values);
        }

        /// <summary>
        /// Type accelerators in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, TypeAcceleratorData> TypeAccelerators { get; }

        /// <summary>
        /// Assemblies loaded in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, AssemblyData> Assemblies { get; }

        /// <summary>
        /// Types, keyed by full type name, loaded in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, TypeData> Types { get; }

        /// <summary>
        /// Type accelerator lookup table linking type accelerators to their full type names.
        /// </summary>
        public IReadOnlyDictionary<string, string> TypeAcceleratorNames { get; }

        private static IReadOnlyDictionary<string, AssemblyData> CreateAssemblyTable(
            IReadOnlyDictionary<string, Data.AssemblyData> assemblies)
        {
            var dict = new Dictionary<string, AssemblyData>(assemblies.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.AssemblyData> assembly in assemblies)
            {
                dict[assembly.Key] = new AssemblyData(assembly.Value);
            }
            return dict;
        }

        private static IReadOnlyDictionary<string, TypeAcceleratorData> CreateTypeAcceleratorTables(
            IReadOnlyDictionary<string, Data.TypeAcceleratorData> typeAccelerators,
            out IReadOnlyDictionary<string, string> typeAcceleratorNames)
        {
            var typeAcceleratorDict = new Dictionary<string, TypeAcceleratorData>(typeAccelerators.Count, StringComparer.OrdinalIgnoreCase);
            var typeAcceleratorNamesDict = new Dictionary<string, string>(typeAccelerators.Count, StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, Data.TypeAcceleratorData> typeAccelerator in typeAccelerators)
            {
                typeAcceleratorDict[typeAccelerator.Key] = new TypeAcceleratorData(typeAccelerator.Key, typeAccelerator.Value);
                typeAcceleratorNamesDict[typeAccelerator.Key] = typeAccelerator.Value.Type;
            }

            typeAcceleratorNames = typeAcceleratorNamesDict;
            return typeAcceleratorDict;
        }

        /// <summary>
        /// Builds the lookup table for full type names.
        /// </summary>
        /// <param name="assemblies">The assembly lookup table in the data object.</param>
        private static IReadOnlyDictionary<string, TypeData> CreateTypeLookupTable(IEnumerable<AssemblyData> assemblies)
        {
            var typeDict = new Dictionary<string, TypeData>(StringComparer.OrdinalIgnoreCase);

            foreach (AssemblyData asm in assemblies)
            {
                if (asm.Types == null)
                {
                    continue;
                }

                foreach (KeyValuePair<string, IReadOnlyDictionary<string, TypeData>> nspace in asm.Types)
                {
                    foreach (TypeData type in nspace.Value.Values)
                    {
                        if (TypeNaming.IsGenericName(type.Name))
                        {
                            string strippedTypeName = TypeNaming.StripGenericQuantifiers(type.Name);
                            string strippedTypeFullName = TypeNaming.AssembleFullName(nspace.Key, strippedTypeName);
                            typeDict[strippedTypeFullName] = type;
                        }

                        string typeFullName = TypeNaming.AssembleFullName(nspace.Key, type.Name);
                        typeDict[typeFullName] = type;
                    }
                }
            }

            return typeDict;
        }
    }
}
