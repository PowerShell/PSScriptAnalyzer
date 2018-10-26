using System;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
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