// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a PowerShell command exported by a module.
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class CommandData : ICloneable
    {
        /// <summary>
        /// The output types given by the command
        /// in type hints, if any.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] OutputType { get; set; }

        /// <summary>
        /// The parameter sets of the command.
        /// If null, indicates the default "__AllParmeterSets"
        /// parameter set.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string[] ParameterSets { get; set; }

        /// <summary>
        /// The default parameter set indicated by
        /// the command, if any.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DefaultParameterSet { get; set; }

        /// <summary>
        /// The parameters of the command, if any, keyed by name.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonCaseInsensitiveStringDictionary<ParameterData> Parameters { get; set; }

        /// <summary>
        /// Lookup table of parameter aliases to their corresponding
        /// full parameter names on the command, if any. Keys are
        /// parameter aliases, values are parameter names.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public JsonCaseInsensitiveStringDictionary<string> ParameterAliases { get; set; }

        /// <summary>
        /// Create a deep clone of the command data object.
        /// </summary>
        public abstract object Clone();
    }
}
