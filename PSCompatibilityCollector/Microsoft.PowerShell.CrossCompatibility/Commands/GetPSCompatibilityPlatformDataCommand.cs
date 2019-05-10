// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    /// <summary>
    /// Class defining the Get-PSCompatibilityPlatformData cmdlet.
    /// Assembles an object describing the current platform PowerShell is running on.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, CommandUtilities.MODULE_PREFIX + "PlatformData")]
    [OutputType(typeof(PlatformData))]
    public class GetPSCompatibilityPlatformDataCommand : Cmdlet
    {
        /// <summary>
        /// The PowerShell data object to use. If not set, this is generated.
        /// </summary>
        [Parameter]
        public PowerShellData PowerShell { get; set; }

        /// <summary>
        /// The operating system data object to use. If not set, this is generated. 
        /// </summary>
        [Parameter]
        public OperatingSystemData OperatingSystem { get; set; }

        /// <summary>
        /// The .NET data object to use. If not set, this is generated.
        /// </summary>
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