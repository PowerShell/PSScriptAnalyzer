using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    /// <summary>
    /// Describes all the members on a .NET type,
    /// broken up into static and instance members.
    /// </summary>
    [Serializable]
    [DataContract]
    public class TypeData : ICloneable
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

        public object Clone()
        {
            return new TypeData()
            {
                Static = (MemberData)Static.Clone(),
                Instance = (MemberData)Instance.Clone()
            };
        }
    }
}