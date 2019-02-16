// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using PropertyDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.PropertyData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a property member on a .NET type.
    /// </summary>
    public class PropertyData
    {
        private readonly PropertyDataMut _propertyData;

        /// <summary>
        /// Create a new query object around collected .NET property data.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="propertyData">Collected property data.</param>
        public PropertyData(string name, PropertyDataMut propertyData)
        {
            Name = name;
            _propertyData = propertyData;
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The full name of the type of the property.
        /// </summary>
        public string Type => _propertyData.Type;

        /// <summary>
        /// List of the available accessors (get, set) on the property.
        /// </summary>
        public IReadOnlyList<AccessorType> Accessors => _propertyData.Accessors;
    }
}
