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
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
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

        protected override void BeginProcessing()
        {
            // todo move to a common initalize session method
            Helper.Instance = new Helper(SessionState.InvokeCommand, this);
            Helper.Instance.Initialize();
            object settingsFound;
            var settingsMode = PowerShell.ScriptAnalyzer.Settings.FindSettingsMode(
                Settings,
                null,
                out settingsFound);

            switch (settingsMode)
            {
                case SettingsMode.Auto:
                    this.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsNotProvided,
                            ""));
                    this.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsAutoDiscovered,
                            (string)settingsFound));
                    break;

                case SettingsMode.Preset:
                case SettingsMode.File:
                    this.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsUsingFile,
                            (string)settingsFound));
                    break;

                case SettingsMode.Hashtable:
                    this.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsUsingHashtable));
                    break;

                default: // case SettingsMode.None
                    this.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsCannotFindFile));
                    break;
            }

            try
            {
                defaultSettings = new Settings(defaultSettingsPreset);
                if (settingsMode != SettingsMode.None)
                {
                    inputSettings = new Settings(settingsFound);
                    ValidateInputSettings();
                }
                else
                {
                    inputSettings = defaultSettings;
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
            var ruleOrder = new string[]
            {
                "PSPlaceCloseBrace",
                "PSPlaceOpenBrace",
                "PSUseConsistentWhitespace",
                "PSUseConsistentIndentation",
                "PSAlignAssignmentStatement"
            };

            foreach (var rule in ruleOrder)
            {
                if (!inputSettings.RuleArguments.ContainsKey(rule))
                {
                    continue;
                }

                this.WriteVerbose("Running " + rule);
                var currentSettingsHashtable = new Hashtable();
                currentSettingsHashtable.Add("IncludeRules", new string[] { rule });
                var ruleSettings = new Hashtable();
                ruleSettings.Add(rule, new Hashtable(inputSettings.RuleArguments[rule]));
                currentSettingsHashtable.Add("Rules", ruleSettings);
                var currentSettings = new Settings(currentSettingsHashtable);
                ScriptAnalyzer.Instance.UpdateSettings(currentSettings);
                ScriptAnalyzer.Instance.Initialize(this, null, null, null, null, false, false);
            }
        }

        private void ValidateInputSettings()
        {
            // todo implement this
            return;
        }
    }
}
