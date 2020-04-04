// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// Readonly query object for PowerShell function data.
    /// </summary>
    public class FunctionData : CommandData
    {
        /// <summary>
        /// Create a new function data query object from a function data object and its name.
        /// </summary>
        /// <param name="functionData">The function data object.</param>
        /// <param name="name">The name of the function.</param>
        public FunctionData(string name, Data.FunctionData functionData)
            : base(name, functionData)
        {
            IsCmdletBinding = functionData.CmdletBinding;
        }

        /// <summary>
        /// True if this is an advanced function (has a cmdlet binding), false otherwise.
        /// </summary>
        public override bool IsCmdletBinding { get; }
    }
}
