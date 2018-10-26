using System;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
{
    /// <summary>
    /// Describes a PowerShell runtime,
    /// as reported by $PSVersionTable.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PowerShellData
    {
	/// <summary>
	/// The self-reported version of PowerShell.
	/// From $PSVersionTable.PSVersion.
	/// </summary>
        [DataMember]
        public Version Version { get; set; }

	/// <summary>
	/// The edition of PowerShell, from
	/// $PSVersionTable.PSEdition.
	/// </summary>
        [DataMember]
        public string Edition { get; set; }

	/// <summary>
	/// The compatible downlevel versions
	/// of PowerShell, from $PSVersionTable.PSCompatibleVersions.
	/// </summary>
        [DataMember]
        public Version[] CompatibleVersions { get; set; }

	/// <summary>
	/// The PowerShell remoting protocol version supported,
	/// from $PSVersionTable.PSRemotingProtocolVersion.
	/// </summary>
        [DataMember]
        public Version RemotingProtocolVersion { get; set; }
        
	/// <summary>
	/// The PowerShell serialization protocol version
	/// supported, from $PSVersionTable.SerializationVersion.
	/// </summary>
        [DataMember]
        public Version SerializationVersion { get; set; }

	/// <summary>
	/// The supported WSMan stack version, from
	/// $PSVersionTable.WSManStackVersion.
	/// </summary>
        [DataMember]
        public Version WSManStackVersion { get; set; }

	/// <summary>
	/// The git commit ID of the PowerShell runtime
	/// if it differs from the reported PowerShell version.
	/// From $PSVersionTable.GitCommitId.
	/// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GitCommitId { get; set; }
    }
}
