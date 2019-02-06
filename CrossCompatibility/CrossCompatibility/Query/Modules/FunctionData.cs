// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

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
        public FunctionData(Modules.FunctionData functionData, string name)
            : base(name, functionData)
        {
        }

        /// <summary>
        /// True if this is an advanced function (has a cmdlet binding), false otherwise.
        /// </summary>
        public override bool IsCmdletBinding => ((Modules.FunctionData)_commandData).CmdletBinding;
    }
}