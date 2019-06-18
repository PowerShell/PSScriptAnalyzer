// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes features not distinct to a single module in a PowerShell runtime.
    /// </summary>
    [DataContract]
    public class CommonPowerShellData : ICloneable
    {
        /// <summary>
        /// Common parameters that appear on all commands with cmdlet binding in PowerShell.
        /// </summary>
        [DataMember]
        public JsonCaseInsensitiveStringDictionary<ParameterData> Parameters { get; set; }

        /// <summary>
        /// Aliases for common parameters.
        /// </summary>
        [DataMember]
        public JsonCaseInsensitiveStringDictionary<string> ParameterAliases { get; set; }

        /// <summary>
        /// Deep clone this object to get a new, independently mutable CommonPowerShellData object.
        /// </summary>
        public object Clone()
        {
            return new CommonPowerShellData()
            {
                ParameterAliases = (JsonCaseInsensitiveStringDictionary<string>)ParameterAliases.Clone(),
                Parameters = (JsonCaseInsensitiveStringDictionary<ParameterData>)Parameters.Clone()
            };
        }
    }
}
