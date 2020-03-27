// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell parameter data.
    /// </summary>
    public class ParameterData
    {
        /// <summary>
        /// Create a new parameter query object from the parameter name and its data object.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterData">The parameter data object.</param>
        public ParameterData(string name, Data.ParameterData parameterData)
        {
            Name = name;
            Type = parameterData.Type;
            IsDynamic = parameterData.Dynamic;
            if (parameterData.ParameterSets != null)
            {
                ParameterSets = CreateParameterSetDictionary(parameterData.ParameterSets);
            }
        }

        /// <summary>
        /// The parameter sets of the object.
        /// </summary>
        /// <value></value>
        public IReadOnlyDictionary<string, ParameterSetData> ParameterSets { get; }

        /// <summary>
        /// The name of the type of the object.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// True if this is a dynamic parameter, false otherwise.
        /// </summary>
        public bool IsDynamic { get; }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; }

        private static IReadOnlyDictionary<string, ParameterSetData> CreateParameterSetDictionary(
            IReadOnlyDictionary<string, Data.ParameterSetData> parameterSetData)
        {
            var dict = new Dictionary<string, ParameterSetData>(parameterSetData.Count, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, Data.ParameterSetData> parameterSet in parameterSetData)
            {
                dict[parameterSet.Key] = new ParameterSetData(parameterSet.Key, parameterSet.Value);
            }
            return dict;
        }
    }
}
