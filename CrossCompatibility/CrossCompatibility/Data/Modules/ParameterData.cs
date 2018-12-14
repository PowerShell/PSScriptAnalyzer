using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a command parameter available on
    /// a command in a particular PowerShell runtime.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ParameterData : ICloneable
    {
        /// <summary>
        /// The parameter sets to which the parameter belongs,
        /// keyed by the parameter set names.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, ParameterSetData> ParameterSets { get; set; }

        /// <summary>
        /// The .NET type of the parameter.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// True if the parameter is dynamic, false otherwise.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool Dynamic { get; set; }

        public object Clone()
        {
            return new ParameterData()
            {
                Type = Type,
                Dynamic = Dynamic,
                ParameterSets = ParameterSets.ToDictionary(p => p.Key, p => (ParameterSetData)p.Value.Clone())
            };
        }
    }
}
