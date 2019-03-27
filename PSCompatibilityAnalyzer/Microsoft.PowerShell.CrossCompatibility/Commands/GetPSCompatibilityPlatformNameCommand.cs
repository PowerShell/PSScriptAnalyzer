using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsCommon.Get, CommandUtilities.MODULE_PREFIX + "PlatformName")]
    public class GetPSCompatibilityPlatformNameCommand : Cmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public PlatformData[] PlatformData { get; set; }

        protected override void BeginProcessing()
        {
            if (PlatformData == null || PlatformData.Length == 0)
            {
                using (SMA.PowerShell pwsh = SMA.PowerShell.Create())
                using (var platformInfoCollector = new PlatformInformationCollector(pwsh))
                {
                    PlatformData = new PlatformData[] { platformInfoCollector.GetPlatformData() };
                }
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