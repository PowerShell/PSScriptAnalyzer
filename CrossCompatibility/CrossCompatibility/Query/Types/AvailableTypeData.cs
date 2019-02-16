// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Types
{
    /// <summary>
    /// Readonly query object for all .NET type information in a PowerShell runtime.
    /// </summary>
    public class AvailableTypeData
    {
        private readonly Lazy<Tuple<IReadOnlyDictionary<string, TypeAcceleratorData>, IReadOnlyDictionary<string, string>>> _typeAccelerators;

        private readonly Lazy<IReadOnlyDictionary<string, AssemblyData>> _assemblies;

        private readonly Lazy<IReadOnlyDictionary<string, TypeData>> _types;

        /// <summary>
        /// Create a new query object around collected .NET type information.
        /// </summary>
        /// <param name="availableTypeData">The .NET type data object to query.</param>
        public AvailableTypeData(Data.Types.AvailableTypeData availableTypeData)
        {
            _assemblies = new Lazy<IReadOnlyDictionary<string, AssemblyData>>(() => CreateAssemblyTable(availableTypeData.Assemblies));
            _typeAccelerators = new Lazy<Tuple<IReadOnlyDictionary<string, TypeAcceleratorData>, IReadOnlyDictionary<string, string>>>(() => CreateTypeAcceleratorTables(availableTypeData.TypeAccelerators));
            _types = new Lazy<IReadOnlyDictionary<string, TypeData>>(() => CreateTypeLookupTable(Assemblies.Values));
        }

        /// <summary>
        /// Type accelerators in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, TypeAcceleratorData> TypeAccelerators => _typeAccelerators.Value.Item1;

        /// <summary>
        /// Assemblies loaded in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, AssemblyData> Assemblies => _assemblies.Value;

        /// <summary>
        /// Types, keyed by full type name, loaded in the PowerShell runtime.
        /// </summary>
        public IReadOnlyDictionary<string, TypeData> Types => _types.Value;

        /// <summary>
        /// Type accelerator lookup table linking type accelerators to their full type names.
        /// </summary>
        public IReadOnlyDictionary<string, string> TypeAcceleratorNames => _typeAccelerators.Value.Item2;

        private static IReadOnlyDictionary<string, AssemblyData> CreateAssemblyTable(
            IReadOnlyDictionary<string, Data.Types.AssemblyData> assemblies)
        {
            var dict = new Dictionary<string, AssemblyData>(assemblies.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.Types.AssemblyData> assembly in assemblies)
            {
                dict[assembly.Key] = new AssemblyData(assembly.Value);
            }
            return dict;
        }

        private static Tuple<IReadOnlyDictionary<string, TypeAcceleratorData>, IReadOnlyDictionary<string, string>> CreateTypeAcceleratorTables(
            IReadOnlyDictionary<string, Data.Types.TypeAcceleratorData> typeAccelerators)
        {
            var typeAcceleratorDict = new Dictionary<string, TypeAcceleratorData>(typeAccelerators.Count, StringComparer.OrdinalIgnoreCase);
            var typeAcceleratorNames = new Dictionary<string, string>(typeAccelerators.Count, StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, Data.Types.TypeAcceleratorData> typeAccelerator in typeAccelerators)
            {
                typeAcceleratorDict[typeAccelerator.Key] = new TypeAcceleratorData(typeAccelerator.Key, typeAccelerator.Value);
                typeAcceleratorNames[typeAccelerator.Key] = typeAccelerator.Value.Type;
            }

            return new Tuple<IReadOnlyDictionary<string, TypeAcceleratorData>, IReadOnlyDictionary<string, string>>(
                typeAcceleratorDict,
                typeAcceleratorNames);
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
