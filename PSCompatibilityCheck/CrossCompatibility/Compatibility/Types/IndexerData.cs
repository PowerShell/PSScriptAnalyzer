using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Describes an indexer on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class IndexerData
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
    }
}