// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    /// <summary>
    /// Class defining the Get-PSCompatibilityPlatformName cmdlet.
    /// Gets the canonical profile ID of the current platform PowerShell is executing on.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, CommandUtilities.MODULE_PREFIX + "PlatformName")]
    [OutputType(typeof(string))]
    public class GetPSCompatibilityPlatformNameCommand : Cmdlet
    {
        /// <summary>
        /// The platform data object to generate the platform name from.
        /// If this is not set, it is generated from the current platform.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true)]
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