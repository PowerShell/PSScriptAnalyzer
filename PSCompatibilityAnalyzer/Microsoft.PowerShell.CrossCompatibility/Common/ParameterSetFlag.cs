// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// Denotes attributes or attribute
    /// flags that may be set on a parameter.
    /// </summary>
    [Serializable]
    [DataContract]
    public enum ParameterSetFlag
    {
        /// <summary>Indicates a mandatory parameter.</summary>
        [EnumMember]
        Mandatory,

        /// <summary>
        /// Indicates the parameter value may be passed
        /// in from the pipeline.
        /// </summary>
        [EnumMember]
        ValueFromPipeline,

        /// <summary>
        /// Indicates the parameter value may be passed
        /// in from the pipeline by property name.
        /// </summary>
        [EnumMember]
        ValueFromPipelineByPropertyName,

        /// <summary>
        /// Indicates the parameter may take its value
        /// from remaining arguments.
        /// </summary>
        [EnumMember]
        ValueFromRemainingArguments,
    }
}
