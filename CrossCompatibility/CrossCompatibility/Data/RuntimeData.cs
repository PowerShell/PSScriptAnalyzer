using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.PowerShell.CrossCompatibility.Data.Types;
using Microsoft.PowerShell.CrossCompatibility.Data.Modules;
using System.Linq;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes what commands and types are available on
    /// a particular PowerShell platform/installation.
    /// </summary>
    [Serializable]
    [DataContract]
    public class RuntimeData : ICloneable
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

        public object Clone()
        {
            return new RuntimeData()
            {
                Types = (AvailableTypeData)Types.Clone(),
                Modules = Modules.ToDictionary(m => m.Key, m => (ModuleData)m.Value.Clone())
            };
        }
    }
}