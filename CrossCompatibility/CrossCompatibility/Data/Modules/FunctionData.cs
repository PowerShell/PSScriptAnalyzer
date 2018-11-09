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
    public class FunctionData : CommandData
    {
	/// <summary>
	/// True if the function has the CmdletBinding attribute
	/// specified, false otherwise.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool CmdletBinding { get; set; }
    }
}
