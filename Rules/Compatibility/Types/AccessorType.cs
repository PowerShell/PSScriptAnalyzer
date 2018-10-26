using System;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
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