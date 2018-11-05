using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Describes a property on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PropertyData
    {
        /// <summary>
        /// Lists the accessors available on this property.
        /// </summary>
        [DataMember]
        public AccessorType[] Accessors { get; set; }

        /// <summary>
        /// The full name of the type of the property.
        /// </summary>
        [DataMember]
        public string Type { get; set; }
    }
}