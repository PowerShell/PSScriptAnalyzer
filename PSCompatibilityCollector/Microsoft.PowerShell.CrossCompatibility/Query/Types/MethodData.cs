// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using MethodDataMut = Microsoft.PowerShell.CrossCompatibility.Data.MethodData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for a method member on a .NET type.
    /// </summary>
    public class MethodData
    {
        /// <summary>
        /// Create a new query object around collected .NET method data.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <param name="methodData">Collected method data.</param>
        public MethodData(string name, MethodDataMut methodData)
        {
            Name = name;
            ReturnType = methodData.ReturnType;
            if (methodData.OverloadParameters != null)
            {
                OverloadParameters = new List<IReadOnlyList<string>>(methodData.OverloadParameters);
            }
        }

        /// <summary>
        /// The name of the method.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The full name of the return type of the method.
        /// </summary>
        public string ReturnType { get; }

        /// <summary>
        /// The overloads of the method, an array of arrays of full type names.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> OverloadParameters { get; }
    }
}
