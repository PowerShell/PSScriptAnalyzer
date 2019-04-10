// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
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
        public JsonCaseInsensitiveStringDictionary<CmdletData> Cmdlets { get; set; }

	/// <summary>
	/// Functions exported by the module, keyed by name.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonCaseInsensitiveStringDictionary<FunctionData> Functions { get; set; }

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
        public JsonCaseInsensitiveStringDictionary<string> Aliases { get; set; }

        /// <summary>
        /// Create a deep clone of the module data object.
        /// </summary>
        public object Clone()
        {
            return new ModuleData()
            {
                Guid = Guid,
                Variables = (string[])Variables?.Clone(),
                Aliases = (JsonCaseInsensitiveStringDictionary<string>)Aliases?.Clone(),
                Cmdlets = (JsonCaseInsensitiveStringDictionary<CmdletData>)Cmdlets?.Clone(),
                Functions = (JsonCaseInsensitiveStringDictionary<FunctionData>)Functions?.Clone(),
            };
        }
    }
}
