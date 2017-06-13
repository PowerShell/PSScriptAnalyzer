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
using System.Linq;
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

            try
            {
                SetRange();
            }
            catch (Exception e)
            {
                this.ThrowTerminatingError(new ErrorRecord(
                        e,
                        "RANGE_ERROR",
                        ErrorCategory.InvalidArgument,
                        Range));
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

        private void SetRange()
        {
            if (Range == null)
            {
                this.range = null;
                return;
            }

            // When the range object is constructed with `[range]::new syntax`, `Range as Range` cast works.
            // However, if the range object is constructed with `new-object "range"` syntax, then
            // we need to use the ImmediatedBaseObject property to cast.
            var range = (Range as Range ?? (Range as PSObject)?.ImmediateBaseObject as Range);
            if (range != null)
            {
                this.range = range;
                return;
            }

            var objArr = Range as object[];
            int[] intArr;
            if (objArr == null)
            {
                // todo check passing int[] casted parameter
                intArr = Range as int[];
                if (intArr == null)
                {
                    throw new ArgumentException(
                        "Argument should be of type Range or object[] or int[].",
                        nameof(Range));
                }
            }

            if (objArr.Length != 4)
            {
                throw new ArgumentException(
                    "Array should be of length 4.",
                    nameof(Range));
            }

            if (!objArr.All(x => x is int))
            {
                throw new ArgumentException(
                    "Array should contain integer elements.",
                    nameof(Range));
            }

            intArr = new int[objArr.Length];
            objArr.CopyTo(intArr, 0);
            this.range = new Range(intArr[0], intArr[1], intArr[2], intArr[3]);
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
