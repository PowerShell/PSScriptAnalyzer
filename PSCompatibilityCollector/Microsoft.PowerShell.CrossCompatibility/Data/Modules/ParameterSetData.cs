// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes the parameter set information
    /// attributed to a command variable.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ParameterSetData : ICloneable
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

        /// <summary>
        /// Create a deep clone of the parameter set data object.
        /// </summary>
        public object Clone()
        {
            return new ParameterSetData()
            {
                Flags = (ParameterSetFlag[])Flags?.Clone(),
                Position = Position
            };
        }
    }
}
