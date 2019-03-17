using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Utility;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsCommon.Get, CommandUtilities.ModulePrefix + "PlatformName")]
    public class GetPSCompatibilityPlatformNameCommand : Cmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PlatformData[] PlatformData { get; set; }

        protected override void BeginProcessing()
        {
            if (PlatformData == null || PlatformData.Length == 0)
            {
                // TODO: Collect platform data
            }
        }

        protected override void ProcessRecord()
        {
            foreach (PlatformData platform in PlatformData)
            {
                WriteObject(PlatformNaming.GetPlatformName(platform));
            }
        }
    }
}