using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules.Compatibility
{
    /// <summary>
    /// Describes the machine PowerShell
    /// runs on.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MachineData
    {
	/// <summary>
	/// The chipset architecture of the machine.
	/// </summary>
        [DataMember]
        public string Architecture { get; set; }

	/// <summary>
	/// The width of a machine word in bits on
	/// the machine.
	/// </summary>
        [DataMember]
        public int Bitness { get; set; }
    }
}
