using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell function
    /// on a particular platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FunctionData
    {
	/// <summary>
	/// The indicated output types of the
	/// function, if any.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] OutputType { get; set; }

	/// <summary>
	/// The parameter sets of the function.
	/// A null value indicates the default
	/// "__AllParameterSets" parameter set.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] ParameterSets { get; set; }

	/// <summary>
	/// The default parameter set of the
	/// function, if any.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultParameterSet { get; set; }

	/// <summary>
	/// True if the function has the CmdletBinding attribute
	/// specified, false otherwise.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool CmdletBinding { get; set; }

	/// <summary>
	/// Parameters of the function if any, keyed by parameter name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, ParameterData> Parameters { get; set; }

	/// <summary>
	/// Lookup table for parameter aliases in the function, if any.
	/// Keys are alias names, values are parameter names.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, string> ParameterAliases { get; set; }
    }
}
