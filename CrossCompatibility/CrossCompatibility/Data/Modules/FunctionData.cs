using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CrossCompatibility.Common;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell function
    /// on a particular platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FunctionData : CommandData
    {
        /// <summary>
        /// True if the function has the CmdletBinding attribute
        /// specified, false otherwise.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool CmdletBinding { get; set; }

        public override object Clone()
        {
            return new FunctionData()
            {
                CmdletBinding = CmdletBinding,
                DefaultParameterSet = DefaultParameterSet,
                OutputType = (string[])OutputType?.Clone(),
                ParameterSets = (string[])ParameterSets?.Clone(),
                ParameterAliases = (JsonDictionary<string, string>)ParameterAliases?.Clone(),
                Parameters = (JsonDictionary<string, ParameterData>)Parameters?.Clone()
            };
        }
    }
}
