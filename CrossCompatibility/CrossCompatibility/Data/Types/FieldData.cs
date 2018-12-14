using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Describes a field on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FieldData : ICloneable
    {
        /// <summary>
        /// The type of the field.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        public object Clone()
        {
            return new FieldData()
            {
                Type = Type
            };
        }
    }
}