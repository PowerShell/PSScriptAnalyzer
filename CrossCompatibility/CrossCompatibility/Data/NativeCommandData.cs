// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    [DataContract]
    public class NativeCommandData : ICloneable
    {
        [DataMember(EmitDefaultValue = false)]
        public Version Version { get; set; }

        [DataMember]
        public string Path { get; set; }

        public object Clone()
        {
            return new NativeCommandData()
            {
                Version = Version,
                Path = Path
            };
        }
    }
}