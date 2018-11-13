using System.Collections.Generic;
using AccessorType = Microsoft.PowerShell.CrossCompatibility.Data.Types.AccessorType;
using IndexerDataMut = Microsoft.PowerShell.CrossCompatibility.Data.Types.IndexerData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class IndexerData
    {
        private readonly IndexerDataMut _indexerData;

        public IndexerData(IndexerDataMut indexerData)
        {
            _indexerData = indexerData;
        }

        public string ItemType => _indexerData.ItemType;

        public IReadOnlyList<string> Parameters => _indexerData.Parameters;

        public IReadOnlyList<AccessorType> Accessors => _indexerData.Accessors;
    }
}