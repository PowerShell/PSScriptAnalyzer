// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class CmdletData : CommandData
    {
        public CmdletData(string name, Modules.CmdletData cmdletData)
            : base(name, cmdletData)
        {
        }

        public override bool IsCmdletBinding => true;
    }
}