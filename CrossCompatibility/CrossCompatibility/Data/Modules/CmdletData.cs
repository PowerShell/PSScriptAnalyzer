using System;
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
                ParameterAliases = (JsonCaseInsensitiveStringDictionary<string>)ParameterAliases?.Clone(),
                ParameterSets = (string[])ParameterSets?.Clone(),
                Parameters = (JsonCaseInsensitiveStringDictionary<ParameterData>)Parameters?.Clone()
            };
        }
    }
}
