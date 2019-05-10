// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes what commands and types are available on
    /// a particular PowerShell platform/installation.
    /// </summary>
    [Serializable]
    [DataContract]
    public class RuntimeData : ICloneable
    {
        /// <summary>
        /// Describes the types available on a particular
        /// PowerShell platform.
        /// </summary>
        [DataMember]
        public AvailableTypeData Types { get; set; }

        /// <summary>
        /// Describes the modules and commands available
        /// on a particular PowerShell platform.
        /// </summary>
        [DataMember]
        public JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>> Modules { get; set; }

        /// <summary>
        /// Describes PowerShell runtime features that are not confined to a particular module
        /// </summary>
        [DataMember]
        public CommonPowerShellData Common { get; set; }

        /// <summary>
        /// Describes native applications available to the PowerShell platform.
        /// </summary>
        [DataMember]
        public JsonCaseInsensitiveStringDictionary<NativeCommandData[]> NativeCommands { get; set; }

        /// <summary>
        /// Create a deep clone of the runtime data object.
        /// </summary>
        public object Clone()
        {
            return new RuntimeData()
            {
                Types = (AvailableTypeData)Types.Clone(),
                Modules = (JsonCaseInsensitiveStringDictionary<JsonDictionary<Version, ModuleData>>)Modules.Clone(),
                NativeCommands = (JsonCaseInsensitiveStringDictionary<NativeCommandData[]>)NativeCommands.Clone(),
            };
        }
    }
}
