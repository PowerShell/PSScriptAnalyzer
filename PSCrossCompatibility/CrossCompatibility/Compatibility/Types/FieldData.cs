using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Describes a field on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FieldData
    {
        /// <summary>
        /// The type of the field.
        /// </summary>
        [DataMember]
        public string Type { get; set; }
    }
}