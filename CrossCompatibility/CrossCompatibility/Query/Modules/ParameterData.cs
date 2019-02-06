// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell parameter data.
    /// </summary>
    public class ParameterData
    {
        private readonly Modules.ParameterData _parameterData;

        /// <summary>
        /// Create a new parameter query object from the parameter name and its data object.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterData">The parameter data object.</param>
        public ParameterData(string name, Modules.ParameterData parameterData)
        {
            _parameterData = parameterData;
            ParameterSets = parameterData.ParameterSets?.ToDictionary(p => p.Key, p => new ParameterSetData(p.Key, p.Value));
            Name = name;
        }

        /// <summary>
        /// The parameter sets of the object.
        /// </summary>
        /// <value></value>
        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets { get; }

        /// <summary>
        /// The name of the type of the object.
        /// </summary>
        public string Type => _parameterData.Type;

        /// <summary>
        /// True if this is a dynamic parameter, false otherwise.
        /// </summary>
        public bool IsDynamic => _parameterData.Dynamic;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; }
    }
}