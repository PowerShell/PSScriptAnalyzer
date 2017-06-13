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

        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        public object Range { get; set; }
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

            if (Range != null)
            {
                try
                {
                    range = GetRange(Range);
                }
                catch (Exception e)
                {
                    this.ThrowTerminatingError(new ErrorRecord(
                            e,
                            "RANGE_ERROR",
                            ErrorCategory.InvalidArgument,
                            Range));
                }
            }

            try
            {
                inputSettings = PSSASettings.Create(Settings, this.MyInvocation.PSScriptRoot, this);
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

        private static Range GetRange(object rangeObj)
        {
            var range = rangeObj as Range;
            if (range == null)
            {
                var intArr = rangeObj as int[];
                if (intArr == null)
                {
                    throw new ArgumentException("Argument should be of type Range or int[].");
                }

                if (intArr.Length != 4)
                {
                    throw new ArgumentException("Integer array should be of length 4.");
                }

                range = new Range(intArr[0], intArr[1], intArr[2], intArr[3]);
            }

            return range;
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
