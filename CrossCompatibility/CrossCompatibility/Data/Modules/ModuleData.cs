using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Describes a PowerShell module as it
    /// is available on a given PowerShell platform.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ModuleData : ICloneable
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

        public object Clone()
        {
            return new ModuleData()
            {
                Guid = Guid,
                Version = Version,
                Variables = (string[])Variables.Clone(),
                Aliases = Aliases.ToDictionary(a => a.Key, a => a.Value),
                Cmdlets = Cmdlets.ToDictionary(c => c.Key, c => (CmdletData)c.Value.Clone()),
                Functions = Functions.ToDictionary(f => f.Key, f => (FunctionData)f.Value.Clone())
            };
        }
    }
}
