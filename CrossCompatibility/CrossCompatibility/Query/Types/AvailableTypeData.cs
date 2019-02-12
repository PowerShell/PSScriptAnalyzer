// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using AvailableTypeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.AvailableTypeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Types
{
    /// <summary>
    /// Readonly query object for all .NET type information in a PowerShell runtime.
    /// </summary>
    public class AvailableTypeData
    {
        private readonly Lazy<IReadOnlyDictionary<string, TypeData>> _types;

        private readonly Lazy<IReadOnlyDictionary<string, string>> _typeAcceleratorNames;

        /// <summary>
        /// Create a new query object around collected .NET type information.
        /// </summary>
        /// <param name="availableTypeData">The .NET type data object to query.</param>
        public AvailableTypeData(AvailableTypeDataMut availableTypeData)
        {
            TypeAccelerators = availableTypeData.TypeAccelerators.ToDictionary(ta => ta.Key, ta => new TypeAcceleratorData(ta.Key, ta.Value), StringComparer.OrdinalIgnoreCase);
            Assemblies = availableTypeData.Assemblies.ToDictionary(asm => asm.Key, asm => new AssemblyData(asm.Value));
            _types = new Lazy<IReadOnlyDictionary<string, TypeData>>(() => CreateTypeLookupTable(Assemblies));
            _typeAcceleratorNames = new Lazy<IReadOnlyDictionary<string, string>>(() => TypeAccelerators.ToDictionary(ta => ta.Key, ta => ta.Value.Type, StringComparer.OrdinalIgnoreCase));
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
        public IReadOnlyDictionary<string, TypeData> Types => _types.Value;

        /// <summary>
        /// Type accelerator lookup table linking type accelerators to their full type names.
        /// </summary>
        public IReadOnlyDictionary<string, string> TypeAcceleratorNames => _typeAcceleratorNames.Value;

        /// <summary>
        /// Builds the lookup table for full type names.
        /// </summary>
        /// <param name="assemblies">The assembly lookup table in the data object.</param>
        private static IReadOnlyDictionary<string, TypeData> CreateTypeLookupTable(IReadOnlyDictionary<string, AssemblyData> assemblies)
        {
            var typeDict = new Dictionary<string, TypeData>(StringComparer.OrdinalIgnoreCase);

            foreach (AssemblyData asm in assemblies.Values)
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