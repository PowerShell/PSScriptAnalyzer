// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell parameter data.
    /// </summary>
    public class ParameterData
    {
        private readonly Data.Modules.ParameterData _parameterData;

        private readonly Lazy<IReadOnlyDictionary<string, ParameterSetData>> _parameterSets;

        /// <summary>
        /// Create a new parameter query object from the parameter name and its data object.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterData">The parameter data object.</param>
        public ParameterData(string name, Data.Modules.ParameterData parameterData)
        {
            _parameterData = parameterData;
            _parameterSets = new Lazy<IReadOnlyDictionary<string, ParameterSetData>>(() => CreateParameterSetDictionary(_parameterData.ParameterSets));
            Name = name;
        }

        /// <summary>
        /// The parameter sets of the object.
        /// </summary>
        /// <value></value>
        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets => _parameterSets.Value;

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

        private static IReadOnlyDictionary<string, ParameterSetData> CreateParameterSetDictionary(
            IReadOnlyDictionary<string, Data.Modules.ParameterSetData> parameterSetData)
        {
            var dict = new Dictionary<string, ParameterSetData>(parameterSetData.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.Modules.ParameterSetData> parameterSet in parameterSetData)
            {
                dict[parameterSet.Key] = new ParameterSetData(parameterSet.Key, parameterSet.Value);
            }
            return dict;
        }
    }
}
