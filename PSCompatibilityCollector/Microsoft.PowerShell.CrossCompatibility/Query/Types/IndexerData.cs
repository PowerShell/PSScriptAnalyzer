// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using IndexerDataMut = Microsoft.PowerShell.CrossCompatibility.Data.IndexerData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for collected .NET indexer data.
    /// </summary>
    public class IndexerData
    {
        private readonly IndexerDataMut _indexerData;

        /// <summary>
        /// Create a new query object around a .NET indexer.
        /// </summary>
        /// <param name="indexerData">The collected indexer data.</param>
        public IndexerData(IndexerDataMut indexerData)
        {
            _indexerData = indexerData;
        }

        /// <summary>
        /// The full name of the return type of the indexer; what the index retrieves.
        /// </summary>
        public string ItemType => _indexerData.ItemType;

        /// <summary>
        /// Full names of the types of parameters of the indexer.
        /// </summary>
        public IReadOnlyList<string> Parameters => _indexerData.Parameters;

        /// <summary>
        /// Which accessors (get, set) are available on the indexer.
        /// </summary>
        public IReadOnlyList<AccessorType> Accessors => _indexerData.Accessors;
    }
}
