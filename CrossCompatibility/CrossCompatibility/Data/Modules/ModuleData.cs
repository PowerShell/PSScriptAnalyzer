using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell module as it
    /// is available on a given PowerShell platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ModuleData
    {
	/// <summary>
	/// The version of the module, if specified.
	/// </summary>
        [DataMember]
        public Version Version { get; set; }

	/// <summary>
	/// The GUID of the module.
	/// </summary>
        [DataMember]
        public Guid Guid { get; set; }

	/// <summary>
	/// Cmdlets exported by the module, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, CmdletData> Cmdlets { get; set; }

	/// <summary>
	/// Functions exported by the module, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, FunctionData> Functions { get; set; }

	/// <summary>
	/// Variables exported by the module.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] Variables { get; set; }

	/// <summary>
	/// Aliases exported by the module, keyed by alias
	/// name, with values being the resolved commands.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, string> Aliases { get; set; }
    }
}
