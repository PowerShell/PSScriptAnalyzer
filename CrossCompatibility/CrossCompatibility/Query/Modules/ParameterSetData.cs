// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object describing PowerShell parameter set metadata.
    /// </summary>
    public class ParameterSetData
    {
        private readonly Modules.ParameterSetData _parameterSet;

        /// <summary>
        /// Create a parameter set query object from the parameter set name and data object.
        /// </summary>
        /// <param name="name">The name of the parameter set.</param>
        /// <param name="parameterSetData">The parameter set data object.</param>
        public ParameterSetData(string name, Modules.ParameterSetData parameterSetData)
        {
            Name = name;
            _parameterSet = parameterSetData;
        }

        /// <summary>
        /// The name of the parameter set.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameter set flags that are set.
        /// </summary>
        public IReadOnlyCollection<ParameterSetFlag> Flags => _parameterSet.Flags;

        /// <summary>
        /// The position of the parameter.
        /// A negative position means no position is specified.
        /// </summary>
        public int Position => _parameterSet.Position;
    }
}