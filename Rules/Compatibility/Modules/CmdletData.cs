using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
{
    /// <summary>
    /// Describes a PowerShell cmdlet from a module.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CmdletData
    {
	/// <summary>
	/// The output types given by the cmdlet
	/// in type hints, if any.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] OutputType { get; set; }

	/// <summary>
	/// The parameter sets of the cmdlet.
	/// If null, indicates the default "__AllParmeterSets"
	/// parameter set.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] ParameterSets { get; set; }

	/// <summary>
	/// The default parameter set indicated by
	/// the cmdlet, if any.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultParameterSet { get; set; }

	/// <summary>
	/// The parameters of the cmdlet, if any, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, ParameterData> Parameters { get; set; }

	/// <summary>
	/// Lookup table of parameter aliases to their corresponding
	/// full parameter names on the cmdlet, if any. Keys are
	/// parameter aliases, values are parameter names.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, string> ParameterAliases { get; set; }
    }
}
