// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes an indexer on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class IndexerData : ICloneable
    {
        /// <summary>
        /// The type of the indexed item itself; the return type of the getter
        /// of the indexer or the receiving type of the setter.
        /// </summary>
        [DataMember]
        public string ItemType { get; set; }

        /// <summary>
        /// The types of the parameters of the indexer; the parameters of
        /// the getter.
        /// </summary>
        [DataMember]
        public string[] Parameters { get; set; }

        /// <summary>
        /// Lists the accessors present on the indexer.
        /// </summary>
        [DataMember]
        public AccessorType[] Accessors { get; set; }

        /// <summary>
        /// Create a deep clone of the indexer data object.
        /// </summary>
        public object Clone()
        {
            return new IndexerData()
            {
                ItemType = ItemType,
                Parameters = (string[])Parameters.Clone(),
                Accessors = (AccessorType[])Accessors.Clone()
            };
        }
    }
}
