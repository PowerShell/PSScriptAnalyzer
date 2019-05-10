// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Management.Automation;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    internal static class PowerShellExtensions
    {
        public static Collection<PSObject> InvokeAndClear(this SMA.PowerShell pwsh)
        {
            try
            {
                return pwsh.Invoke();
            }
            finally
            {
                pwsh.Commands.Clear();
            }
        }

        public static Collection<T> InvokeAndClear<T>(this SMA.PowerShell pwsh)
        {
            try
            {
                return pwsh.Invoke<T>();
            }
            finally
            {
                pwsh.Commands.Clear();
            }
        }
    }
}