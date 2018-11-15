using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Modules
{
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
        public int Position { get; set; } = int.MinValue;
    }
}
