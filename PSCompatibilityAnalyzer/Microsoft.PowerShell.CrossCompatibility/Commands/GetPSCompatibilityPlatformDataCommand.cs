using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsCommon.Get, CommandUtilities.MODULE_PREFIX + "PlatformData")]
    public class GetPSCompatibilityPlatformDataCommand : Cmdlet
    {
        [Parameter]
        public PowerShellData PowerShell { get; set; }

        [Parameter]
        public OperatingSystemData OperatingSystem { get; set; }

        [Parameter]
        public DotnetData DotNet { get; set; }

        protected override void EndProcessing()
        {
            var platformData = new PlatformData();

            using (var pwsh = System.Management.Automation.PowerShell.Create())
            using (var platformDataCollector = new PlatformInformationCollector(pwsh))
            {
                platformData.Dotnet = DotNet ?? platformDataCollector.GetDotNetData();
                platformData.OperatingSystem = OperatingSystem ?? platformDataCollector.GetOperatingSystemData();
                platformData.PowerShell = PowerShell ?? platformDataCollector.GetPowerShellData();
            }

            WriteObject(platformData);
        }
    }
}