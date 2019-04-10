// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.CrossCompatibility.Data
{
    /// <summary>
    /// Describes a method on a .NET type.
    /// </summary>
    [Serializable]
    [DataContract]
    public class MethodData : ICloneable
    {
        /// <summary>
        /// The full name of the type returned by the method.
        /// </summary>
        [DataMember]
        public string ReturnType { get; set; }

        /// <summary>
        /// The overloads of the method, with each element in
        /// the array representing one overload. Each overload
        /// has the full type names of each parameter in order.
        /// </summary>
        [DataMember]
        public string[][] OverloadParameters { get; set; }

        /// <summary>
        /// Create a deep clone of the method data object.
        /// </summary>
        public object Clone()
        {
            return new MethodData()
            {
                ReturnType = ReturnType,
                OverloadParameters = OverloadParameters.Select(op => (string[])op.Clone()).ToArray()
            };
        }
    }
}
