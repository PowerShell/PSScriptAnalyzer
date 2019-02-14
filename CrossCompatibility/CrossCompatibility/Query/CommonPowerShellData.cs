using System;
using System.Collections.Generic;
using System.Linq;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for common runtime data in a PowerShell runtime that is not specific to a single module.
    /// </summary>
    public class CommonPowerShellData
    {
        private readonly Lazy<Tuple<IReadOnlyDictionary<string, ParameterData>, IReadOnlyDictionary<string, ParameterData>>> _parameters;

        /// <summary>
        /// Create a new query object for common PowerShell data.
        /// </summary>
        /// <param name="commonPowerShellData">The mutable data object holding common data information.</param>
        public CommonPowerShellData(Data.CommonPowerShellData commonPowerShellData)
        {
            _parameters = new Lazy<Tuple<IReadOnlyDictionary<string, ParameterData>, IReadOnlyDictionary<string, ParameterData>>>(() => CreateParameterTable(commonPowerShellData.Parameters, commonPowerShellData.ParameterAliases));
        }

        /// <summary>
        /// Common parameters, present on all commands bound with the cmdlet binding.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> Parameters => _parameters.Value.Item1;

        /// <summary>
        /// Aliases for common parameters.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> ParameterAliases => _parameters.Value.Item2;

        private Tuple<IReadOnlyDictionary<string, ParameterData>, IReadOnlyDictionary<string, ParameterData>> CreateParameterTable(
            IReadOnlyDictionary<string, Data.Modules.ParameterData> parameters,
            IReadOnlyDictionary<string, string> parameterAliases)
        {
            var parameterDict = new Dictionary<string, ParameterData>(parameters.Count + parameterAliases.Count, StringComparer.OrdinalIgnoreCase);
            var parameterAliasDict = new Dictionary<string, ParameterData>(parameterAliases.Count, StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, Data.Modules.ParameterData> parameter in parameters)
            {
                parameterDict[parameter.Key] = new ParameterData(parameter.Key, parameter.Value);
            }

            foreach (KeyValuePair<string, string> parameterAlias in parameterAliases)
            {
                ParameterData aliasedParameter = parameterDict[parameterAlias.Value];
                parameterAliasDict[parameterAlias.Key] = aliasedParameter;
                parameterDict[parameterAlias.Key] = aliasedParameter;
            }

            return new Tuple<IReadOnlyDictionary<string, ParameterData>, IReadOnlyDictionary<string, ParameterData>>(
                parameterDict,
                parameterAliasDict);
        }
    }
}