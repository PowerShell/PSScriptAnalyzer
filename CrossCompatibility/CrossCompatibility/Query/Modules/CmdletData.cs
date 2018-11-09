using Modules = Microsoft.PowerShell.CrossCompatibility.Data.Modules;

namespace Microsoft.PowerShell.CrossCompatibility.Query
{
    public class CmdletData : CommandData
    {
        public CmdletData(Modules.CmdletData cmdletData, string name)
            : base(cmdletData, name)
        {
        }

        public override bool IsCmdletBinding => true;
    }
}