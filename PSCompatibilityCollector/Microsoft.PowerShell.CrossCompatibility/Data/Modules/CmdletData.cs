// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a PowerShell cmdlet from a module.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CmdletData : CommandData
    {
        /// <summary>
        /// Create a deep clone of the cmdlet data object.
        /// </summary>
        public override object Clone()
        {
            return new CmdletData()
            {
                DefaultParameterSet = DefaultParameterSet,
                OutputType = (string[])OutputType?.Clone(),
                ParameterAliases = (JsonCaseInsensitiveStringDictionary<string>)ParameterAliases?.Clone(),
                ParameterSets = (string[])ParameterSets?.Clone(),
                Parameters = (JsonCaseInsensitiveStringDictionary<ParameterData>)Parameters?.Clone()
            };
        }
    }
}
