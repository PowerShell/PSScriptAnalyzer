using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
{
    /// <summary>
    /// Describes what commands and types are available on
    /// a particular PowerShell platform/installation.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CompatibilityData
    {
        /// <summary>
        /// Describes the types available on a particular
        /// PowerShell platform.
        /// </summary>
        [DataMember]
        public AvailableTypeData Types { get; set; }

        /// <summary>
        /// Describes the modules and commands available
        /// on a particular PowerShell platform.
        /// </summary>
        [DataMember]
        public IDictionary<string, ModuleData> Modules { get; set; }
    }
}