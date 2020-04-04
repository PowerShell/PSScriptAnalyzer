// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using FieldDataMut = Microsoft.PowerShell.CrossCompatibility.Data.FieldData;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for .NET field member data.
    /// </summary>
    public class FieldData
    {
        /// <summary>
        /// Create a new query object around collected field member information.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldData">The collected field information.</param>
        public FieldData(string name, FieldDataMut fieldData)
        {
            Name = name;
            Type = fieldData.Type;
        }

        /// <summary>
        /// The name of the field member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the field.
        /// </summary>
        public string Type { get; }
    }
}
