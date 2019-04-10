// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Data = Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    /// <summary>
    /// A readonly query object for PowerShell cmdlet data.
    /// </summary>
    public class CmdletData : CommandData
    {
        /// <summary>
        /// Create a cmdlet data query object from cmdlet data.
        /// </summary>
        /// <param name="name">The name of the cmdlet.</param>
        /// <param name="cmdletData">The cmdlet data.</param>
        public CmdletData(string name, Data.CmdletData cmdletData)
            : base(name, cmdletData)
        {
        }

        /// <summary>
        /// Indicates that this command has a cmdlet binding.
        /// </summary>
        public override bool IsCmdletBinding => true;
    }
}
