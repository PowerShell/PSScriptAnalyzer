using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell cmdlet from a module.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CmdletData : CommandData
    {
        public override object Clone()
        {
            return new CmdletData()
            {
                DefaultParameterSet = DefaultParameterSet,
                OutputType = (string[])OutputType?.Clone(),
                ParameterAliases = ParameterAliases?.ToDictionary(pa => pa.Key, pa => pa.Value),
                ParameterSets = (string[])ParameterSets?.Clone(),
                Parameters = Parameters?.ToDictionary(p => p.Key, p => (ParameterData)p.Value.Clone())
            };
        }
    }
}
