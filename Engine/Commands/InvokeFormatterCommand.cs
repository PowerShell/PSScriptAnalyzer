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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    using PSSASettings = Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings;

    [Cmdlet(VerbsLifecycle.Invoke, "Formatter")]
    public class InvokeFormatterCommand : PSCmdlet, IOutputWriter
    {
        private const string defaultSettingsPreset = "CodeFormatting";
        private Settings defaultSettings;
        private Settings inputSettings;

        [ParameterAttribute(Mandatory = true)]
        [ValidateNotNull]
        public string ScriptDefinition { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        public object Settings { get; set; }

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

            Helper.Instance = new Helper(SessionState.InvokeCommand, this);
            Helper.Instance.Initialize();

            try
            {
                inputSettings = PSSASettings.Create(Settings, null, this);
                if (inputSettings == null)
                {
                    inputSettings = new PSSASettings(
                        defaultSettingsPreset,
                        PSSASettings.GetSettingPresetFilePath);
                }
            }
            catch
            {
                this.WriteWarning(String.Format(CultureInfo.CurrentCulture, Strings.SettingsNotParsable));
                return;
            }
        }

        protected override void ProcessRecord()
        {
            var text = Formatter.Format(ScriptDefinition, inputSettings, this);
            this.WriteObject(text);
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
