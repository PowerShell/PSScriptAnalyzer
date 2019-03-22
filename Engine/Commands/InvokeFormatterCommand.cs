// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    using PSSASettings = Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings;

    /// <summary>
    /// A cmdlet to format a PowerShell script text.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "Formatter")]
    [OutputType(typeof(string))]
    public class InvokeFormatterCommand : PSCmdlet, IOutputWriter
    {
        private const string defaultSettingsPreset = "CodeFormatting";
        private Settings inputSettings;
        private Range range;

        /// <summary>
        /// The script text to be formated.
        ///
        /// *NOTE*: Unlike ScriptBlock parameter, the ScriptDefinition parameter require a string value.
        /// </summary>
        [ParameterAttribute(Mandatory = true, Position = 1)]
        [ValidateNotNull]
        public string ScriptDefinition { get; set; }

        /// <summary>
        /// A settings hashtable or a path to a PowerShell data file (.psd1) file that contains the settings.
        /// </summary>
        [Parameter(Mandatory = false, Position = 2)]
        [ValidateNotNull]
        public object Settings { get; set; } = defaultSettingsPreset;

        /// <summary>
        /// The range within which formatting should take place.
        ///
        /// The parameter is an array of integers of length 4 such that the first, second, third and last
        /// elements correspond to the start line number, start column number, end line number and
        /// end column number. These numbers must be greater than 0.
        /// </summary>
        /// <returns></returns>
        [Parameter(Mandatory = false, Position = 3)]
        [ValidateNotNull]
        [ValidateCount(4, 4)]
        public int[] Range { get; set; }

#if DEBUG
        /// <summary>
        /// Attaches to an instance of a .Net debugger
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter AttachAndDebug
        {
            get { return attachAndDebug; }
            set { attachAndDebug = value; }
        }

        private bool attachAndDebug = false;
#endif

        protected override void BeginProcessing()
        {
#if DEBUG
            if (attachAndDebug)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                else
                {
                    System.Diagnostics.Debugger.Launch();
                }
            }
#endif

            this.range = Range == null ? null : new Range(Range[0], Range[1], Range[2], Range[3]);
            try
            {
                inputSettings = PSSASettings.Create(Settings, this.MyInvocation.PSScriptRoot, this, GetResolvedProviderPathFromPSPath);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                        e,
                        "SETTINGS_ERROR",
                        ErrorCategory.InvalidData,
                        Settings));
            }

            if (inputSettings == null)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException(String.Format(
                        CultureInfo.CurrentCulture,
                        Strings.SettingsNotParsable)),
                    "SETTINGS_ERROR",
                    ErrorCategory.InvalidArgument,
                    Settings));
            }
        }

        protected override void ProcessRecord()
        {
            // todo add tests to check range formatting
            string formattedScriptDefinition;
            formattedScriptDefinition = Formatter.Format(ScriptDefinition, inputSettings, range, this);
            this.WriteObject(formattedScriptDefinition);
        }

        protected override void EndProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.EndProcessing();
        }

        protected override void StopProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.StopProcessing();
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
