using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Denotes a .NET property accessor.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum AccessorType
    {
        /// <summary>A property getter.</summary>
        [EnumMember]
        Get,

        /// <summary>A property setter.</summary>
        [EnumMember]
        Set
    }
}