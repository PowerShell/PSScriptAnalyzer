using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Utility;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    [Cmdlet(VerbsLifecycle.Assert, CommandUtilities.MODULE_PREFIX + "ProfileIsValid")]
    public class AssertPSCompatibilityProfileIsValid : Cmdlet
    {
        private ProfileValidator _validator;

        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        [ValidateNotNull]
        public CompatibilityProfileData[] CompatibilityProfile { get; set; }

        protected override void BeginProcessing()
        {
            _validator = new QuickCheckValidator();
        }

        protected override void ProcessRecord()
        {
            foreach (CompatibilityProfileData profile in CompatibilityProfile)
            {
                _validator.Reset();
                if (_validator.IsProfileValid(profile, out IEnumerable<Exception> errors))
                {
                    continue;
                }

                foreach (Exception error in errors)
                {
                    // Writing errors will set $? = $false
                    this.WriteExceptionAsError(error);
                }
            }
        }
    }
}