using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Holds a structured form of a full assembly name.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AssemblyNameData
    {
        /// <summary>
        /// The simple name of the assembly.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The version of the assembly.
        /// </summary>
        [DataMember]
        public Version Version { get; set; }

        /// <summary>
        /// The culture of the assembly.
        /// This should not be null, but null
        /// should be considered the same as "neutral".
        /// </summary>
        [DataMember]
        public string Culture { get; set; }

        /// <summary>
        /// The public key token of the assembly.
        /// This may be null if the assembly has no public
        /// key token (i.e. is unsigned).
        /// </summary>
        [DataMember]
        public byte[] PublicKeyToken { get; set; }
    }
}