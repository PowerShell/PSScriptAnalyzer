using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsCommon.Get, CommandUtilities.MODULE_PREFIX + "ProfileData")]
    public class GetPSCompatibilityProfileDataCommand : Cmdlet
    {
        protected override void EndProcessing()
        {
            using (var pwsh = System.Management.Automation.PowerShell.Create())
            using (var runtimeDataCollector = new CompatibilityProfileCollector(pwsh))
            {
                CompatibilityProfileData runtimeProfileData = runtimeDataCollector.GetCompatibilityData(out IEnumerable<Exception> errors);

                foreach (Exception error in errors)
                {
                    WriteError(CommandUtilities.CreateCompatibilityErrorRecord(error));
                }

                WriteObject(runtimeProfileData);
            }
        }
    }
}