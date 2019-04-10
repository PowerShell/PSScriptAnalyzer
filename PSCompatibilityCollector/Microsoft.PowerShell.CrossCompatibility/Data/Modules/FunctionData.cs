// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
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

        /// <summary>
        /// Create a deep clone of the function data object.
        /// </summary>
        public override object Clone()
        {
            return new FunctionData()
            {
                CmdletBinding = CmdletBinding,
                DefaultParameterSet = DefaultParameterSet,
                OutputType = (string[])OutputType?.Clone(),
                ParameterSets = (string[])ParameterSets?.Clone(),
                ParameterAliases = (JsonCaseInsensitiveStringDictionary<string>)ParameterAliases?.Clone(),
                Parameters = (JsonCaseInsensitiveStringDictionary<ParameterData>)Parameters?.Clone()
            };
        }
    }
}
