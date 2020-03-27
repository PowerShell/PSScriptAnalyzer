// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object describing PowerShell parameter set metadata.
    /// </summary>
    public class ParameterSetData
    {
        /// <summary>
        /// Create a parameter set query object from the parameter set name and data object.
        /// </summary>
        /// <param name="name">The name of the parameter set.</param>
        /// <param name="parameterSetData">The parameter set data object.</param>
        public ParameterSetData(string name, Modules.ParameterSetData parameterSetData)
        {
            Name = name;
            Flags = parameterSetData.Flags;
            Position = parameterSetData.Position;
        }

        /// <summary>
        /// The name of the parameter set.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameter set flags that are set.
        /// </summary>
        public IReadOnlyCollection<ParameterSetFlag> Flags { get; }

        /// <summary>
        /// The position of the parameter.
        /// A negative position means no position is specified.
        /// </summary>
        public int Position { get; }
    }
}
