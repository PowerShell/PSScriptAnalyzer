using System;
using System.Linq;
using System.Runtime.Serialization;
using CrossCompatibility.Common;

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
	/// The GUID of the module.
	/// </summary>
        [DataMember]
        public Guid Guid { get; set; }

	/// <summary>
	/// Cmdlets exported by the module, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, CmdletData> Cmdlets { get; set; }

	/// <summary>
	/// Functions exported by the module, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonDictionary<string, FunctionData> Functions { get; set; }

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
        public JsonDictionary<string, string> Aliases { get; set; }

        public object Clone()
        {
            return new ModuleData()
            {
                Guid = Guid,
                Variables = (string[])Variables?.Clone(),
                Aliases = (JsonDictionary<string, string>)Aliases?.Clone(),
                Cmdlets = (JsonDictionary<string, CmdletData>)Cmdlets?.Clone(),
                Functions = (JsonDictionary<string, FunctionData>)Functions?.Clone(),
            };
        }
    }
}
