using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
    /// <summary>
    /// Denotes attributes or attribute
    /// flags that may be set on a parameter.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ParameterSetFlag
    {
	/// <summary>Indicates a mandatory parameter.</summary>
        [EnumMember]
        Mandatory,

	/// <summary>
	/// Indicates the parameter value may be passed
	/// in from the pipeline.
	/// </summary>
        [EnumMember]
        ValueFromPipeline,

	/// <summary>
	/// Indicates the parameter value may be passed
	/// in from the pipeline by property name.
	/// </summary>
        [EnumMember]
        ValueFromPipelineByPropertyName,

	/// <summary>
	/// Indicates the parameter may take its value
	/// from remaining arguments.
	/// </summary>
        [EnumMember]
        ValueFromRemainingArguments,
    }

    /// <summary>
    /// Describes the parameter set information
    /// attributed to a command variable.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ParameterSetData
    {
	/// <summary>
	/// The parameter set attributes or
	/// attribute flags assigned to a parameter
	/// in the parameter set.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ParameterSetFlag[] Flags { get; set; }

	/// <summary>
	/// The position of the parameter. If none is given,
	/// the default position of Int.MinValue is assumed.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Position { get; set; }
    }
}
