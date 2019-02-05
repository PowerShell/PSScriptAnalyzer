// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data.Types
{
    [Serializable]
    [DataContract]
    public class TypeAcceleratorData : ICloneable
    {
        [DataMember]
        public string Assembly { get; set; }

        [DataMember]
        public string Type { get; set; }

        public object Clone()
        {
            return new TypeAcceleratorData()
            {
                Assembly = Assembly,
                Type = Type
            };
        }
    }
}