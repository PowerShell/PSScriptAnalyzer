using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Types
{
    /// <summary>
    /// Describes all the members on a .NET type,
    /// broken up into static and instance members.
    /// </summary>
    [Serializable]
    [DataContract]
    public class TypeData
    {
        /// <summary>
        /// The static members on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MemberData Static { get; set; }

        /// <summary>
        /// The instance members on the type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MemberData Instance { get; set; }
    }
}