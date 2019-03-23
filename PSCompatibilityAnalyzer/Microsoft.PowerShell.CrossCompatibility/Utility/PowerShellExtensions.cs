using System.Collections.ObjectModel;
using System.Management.Automation;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    internal static class PowerShellExtensions
    {
        public static Collection<PSObject> InvokeAndClear(this SMA.PowerShell pwsh)
        {
            Collection<PSObject> result = pwsh.Invoke();
            pwsh.Commands.Clear();
            return result;
        }

        public static Collection<T> InvokeAndClear<T>(this SMA.PowerShell pwsh)
        {
            Collection<T> result = pwsh.Invoke<T>();
            pwsh.Commands.Clear();
            return result;
        }
    }
}