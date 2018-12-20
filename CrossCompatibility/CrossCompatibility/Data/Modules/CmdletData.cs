using System;
using System.Linq;
using System.Runtime.Serialization;
using CrossCompatibility.Common;

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
                ParameterAliases = (JsonDictionary<string, string>)ParameterAliases?.Clone(),
                ParameterSets = (string[])ParameterSets?.Clone(),
                Parameters = (JsonDictionary<string, ParameterData>)Parameters?.Clone()
            };
        }
    }
}
