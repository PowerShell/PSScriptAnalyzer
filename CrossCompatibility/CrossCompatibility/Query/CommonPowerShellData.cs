using System;
using System.Collections.Generic;
using System.Linq;
using CommonPowerShellDataMut = Microsoft.PowerShell.CrossCompatibility.Data.CommonPowerShellData;

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
        /// <param name="commonPowerShellDataMut">The mutable data object holding common data information.</param>
        public CommonPowerShellData(CommonPowerShellDataMut commonPowerShellDataMut)
        {
            Parameters = commonPowerShellDataMut.Parameters.ToDictionary(p => p.Key, p => new ParameterData(p.Key, p.Value), StringComparer.OrdinalIgnoreCase);
            ParameterAliases = commonPowerShellDataMut.ParameterAliases.ToDictionary(pa => pa.Key, pa => Parameters[pa.Value], StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Common parameters, present on all commands bound with the cmdlet binding.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> Parameters { get; }

        /// <summary>
        /// Aliases for common parameters.
        /// </summary>
        public IReadOnlyDictionary<string, ParameterData> ParameterAliases { get; }
    }
}