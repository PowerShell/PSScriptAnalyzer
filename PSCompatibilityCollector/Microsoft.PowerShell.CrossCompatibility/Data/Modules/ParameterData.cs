// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a command parameter available on
    /// a command in a particular PowerShell runtime.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ParameterData : ICloneable
    {
        /// <summary>
        /// The parameter sets to which the parameter belongs,
        /// keyed by the parameter set names.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonCaseInsensitiveStringDictionary<ParameterSetData> ParameterSets { get; set; }

        /// <summary>
        /// The .NET type of the parameter.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// True if the parameter is dynamic, false otherwise.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool Dynamic { get; set; }

        /// <summary>
        /// Create a deep clone of the parameter data object.
        /// </summary>
        public object Clone()
        {
            return new ParameterData()
            {
                Type = Type,
                Dynamic = Dynamic,
                ParameterSets = (JsonCaseInsensitiveStringDictionary<ParameterSetData>)ParameterSets?.Clone()
            };
        }
    }
}
