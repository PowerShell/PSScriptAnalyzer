// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for common runtime data in a PowerShell runtime that is not specific to a single module.
    /// </summary>
    public class CommonPowerShellData
    {
        /// <summary>
        /// Create a new query object for common PowerShell data.
        /// </summary>
        /// <param name="commonPowerShellData">The mutable data object holding common data information.</param>
        public CommonPowerShellData(Data.CommonPowerShellData commonPowerShellData)
        {
            Parameters = CreateParameterTable(commonPowerShellData.Parameters, commonPowerShellData.ParameterAliases, out IReadOnlyDictionary<string, ParameterData> parameterAliases);
            ParameterAliases = parameterAliases;
        }

        /// <summary>
        /// Common parameters, present on all commands bound with the cmdlet binding.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> Parameters { get; }

        /// <summary>
        /// Aliases for common parameters.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> ParameterAliases { get; }

        private IReadOnlyDictionary<string, ParameterData> CreateParameterTable(
            IReadOnlyDictionary<string, Data.ParameterData> parameters,
            IReadOnlyDictionary<string, string> parameterAliases,
            out IReadOnlyDictionary<string, ParameterData> queryAliases)
        {
            var parameterDict = new Dictionary<string, ParameterData>(parameters.Count + parameterAliases.Count, StringComparer.OrdinalIgnoreCase);
            var parameterAliasDict = new Dictionary<string, ParameterData>(parameterAliases.Count, StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, Data.ParameterData> parameter in parameters)
            {
                parameterDict[parameter.Key] = new ParameterData(parameter.Key, parameter.Value);
            }

            foreach (KeyValuePair<string, string> parameterAlias in parameterAliases)
            {
                ParameterData aliasedParameter = parameterDict[parameterAlias.Value];
                parameterAliasDict[parameterAlias.Key] = aliasedParameter;
                parameterDict[parameterAlias.Key] = aliasedParameter;
            }

            queryAliases = parameterAliasDict;
            return parameterDict;
        }
    }
}
