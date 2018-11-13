using FieldDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.FieldData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class FieldData
    {
        private readonly FieldDataMut _fieldData;

        public FieldData(string name, FieldDataMut fieldData)
        {
            Name = name;
            _fieldData = fieldData;
        }

        public string Name { get; }

        public string Type => _fieldData.Type;
    }
}