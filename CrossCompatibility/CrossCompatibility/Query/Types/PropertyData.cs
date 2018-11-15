using System.Collections.Generic;
using PropertyDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.PropertyData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class PropertyData
    {
        private readonly PropertyDataMut _propertyData;

        public PropertyData(string name, PropertyDataMut propertyData)
        {
            Name = name;
            _propertyData = propertyData;
        }

        public string Name { get; }

        public string Type => _propertyData.Type;

        public IReadOnlyList<AccessorType> Accessors => _propertyData.Accessors;
    }
}