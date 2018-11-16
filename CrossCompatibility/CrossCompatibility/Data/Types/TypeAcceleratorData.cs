using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    [Serializable]
    [DataContract]
    public class TypeAcceleratorData
    {
        [DataMember]
        public string Assembly { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}