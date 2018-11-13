using TypeDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.TypeData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class TypeData
    {
        private readonly TypeDataMut _typeData;

        public TypeData(string name, TypeDataMut typeData)
        {
            Name = name;
            _typeData = typeData;
            Instance = new MemberData(typeData.Instance);
            Static = new MemberData(typeData.Static);
        }

        public string Name { get; }

        public MemberData Instance { get; }

        public MemberData Static { get; }
    }
}