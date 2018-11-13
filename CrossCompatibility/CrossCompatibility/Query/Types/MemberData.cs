using System.Collections.Generic;
using System.Linq;
using MemberDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.MemberData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class MemberData
    {
        private readonly MemberDataMut _memberData;

        public MemberData(MemberDataMut memberData)
        {
            _memberData = memberData;
            Fields = _memberData.Fields.ToDictionary(f => f.Key, f => new FieldData(f.Key, f.Value));
            Properties = _memberData.Properties.ToDictionary(p => p.Key, p => new PropertyData(p.Key, p.Value));
            Indexers = _memberData.Indexers.Select(i => new IndexerData(i)).ToArray();
            Events = _memberData.Events.ToDictionary(e => e.Key, e => new EventData(e.Key, e.Value));
            NestedTypes = _memberData.NestedTypes.ToDictionary(t => t.Key, t => new TypeData(t.Key, t.Value));
        }

        public IReadOnlyList<IReadOnlyList<string>> Constructors => _memberData.Constructors;

        public IReadOnlyDictionary<string, FieldData> Fields { get; }

        public IReadOnlyDictionary<string, PropertyData> Properties { get; }

        public IReadOnlyList<IndexerData> Indexers { get; }

        public IReadOnlyDictionary<string, EventData> Events { get; }

        public IReadOnlyDictionary<string, TypeData> NestedTypes { get; }
    }
}