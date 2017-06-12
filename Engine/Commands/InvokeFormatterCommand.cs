//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

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
    public class InvokeFormatterCommand : PSCmdlet, IOutputWriter
    {
        private const string defaultSettingsPreset = "CodeFormatting";
        private Settings inputSettings;

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

#if DEBUG
        [Parameter(Mandatory = false)]
        public Range Range { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "NoRange")]
        public int StartLineNumber { get; set; } = -1;
        [Parameter(Mandatory = false, ParameterSetName = "NoRange")]
        public int StartColumnNumber { get; set; } = -1;
        [Parameter(Mandatory = false, ParameterSetName = "NoRange")]
        public int EndLineNumber { get; set; } = -1;
        [Parameter(Mandatory = false, ParameterSetName = "NoRange")]
        public int EndColumnNumber { get; set; } = -1;

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

            try
            {
                inputSettings = PSSASettings.Create(Settings, this.MyInvocation.PSScriptRoot, this);
            }
            catch (Exception e)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                        e,
                        "SETTNGS_ERROR",
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
#if DEBUG
            var range = Range;
            if (this.ParameterSetName.Equals("NoRange"))
            {
                range = new Range(StartLineNumber, StartColumnNumber, EndLineNumber, EndColumnNumber);
            }

            formattedScriptDefinition = Formatter.Format(ScriptDefinition, inputSettings, range, this);
#endif // DEBUG

            formattedScriptDefinition = Formatter.Format(ScriptDefinition, inputSettings, null, this);
            this.WriteObject(formattedScriptDefinition);
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
