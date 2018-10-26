using System;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
{
    /// <summary>
    /// Describes a method on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MethodData
    {
        /// <summary>
        /// The full name of the type returned by the method.
        /// </summary>
        [DataMember]
        public string ReturnType { get; set; }

        /// <summary>
        /// The overloads of the method, with each element in
        /// the array representing one overload. Each overload
        /// has the full type names of each parameter in order.
        /// </summary>
        [DataMember]
        public string[][] OverloadParameters { get; set; }
    }
}