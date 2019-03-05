// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TypeAcceleratorDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeAcceleratorData;

namespace Microsoft.PowerShell.CrossCompatibility.Query.Types
{
    /// <summary>
    /// Readonly query object for a PowerShell type accelerator.
    /// </summary>
    public class TypeAcceleratorData
    {
        private readonly TypeAcceleratorDataMut _typeAcceleratorData;

        /// <summary>
        /// Create a new query object around collected PowerShell type accelerator data.
        /// </summary>
        /// <param name="name">The name of the type accelerator (the accelerator string).</param>
        /// <param name="typeAcceleratorData">Collected type accelerator data.</param>
        public TypeAcceleratorData(string name, TypeAcceleratorDataMut typeAcceleratorData)
        {
            Name = name;
            _typeAcceleratorData = typeAcceleratorData;
        }

        /// <summary>
        /// The name of the type accelerator; the key used for type lookup.
        /// For example "psmoduleinfo" for [psmoduleinfo].
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The assembly carrying the type the type accelerator refers to.
        /// </summary>
        public string Assembly => _typeAcceleratorData.Assembly;

        /// <summary>
        /// The full name of the type the type accelerator refers to.
        /// </summary>
        public string Type => _typeAcceleratorData.Type;
    }
}
