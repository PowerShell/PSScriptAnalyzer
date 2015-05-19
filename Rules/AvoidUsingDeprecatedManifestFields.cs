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
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingDeprecatedManifestFields: Run Test Module Manifest to check that no deprecated fields are being used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidUsingDeprecatedManifestFields : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Run Test Module Manifest to check that no deprecated fields are being used.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            if (String.Equals(System.IO.Path.GetExtension(fileName), ".psd1", StringComparison.OrdinalIgnoreCase))
            {
                var ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
                IEnumerable<PSObject> result = null;
                try
                {
                    ps.AddCommand("Test-ModuleManifest");
                    ps.AddParameter("Path", fileName);

                    // Suppress warnings emitted during the execution of Test-ModuleManifest
                    // ModuleManifest rule must catch any violations (warnings/errors) and generate DiagnosticRecord(s)
                    ps.AddParameter("WarningAction", ActionPreference.SilentlyContinue);
                    ps.AddParameter("WarningVariable", "Message");
                    ps.AddScript("$Message");
                    result = ps.Invoke();

                }
                catch
                {}

                if (result != null)
                {
                    foreach (var warning in result)
                    {
                        if (warning.BaseObject != null)
                        {
                            yield return
                                new DiagnosticRecord(
                                    String.Format(CultureInfo.CurrentCulture, warning.BaseObject.ToString()), ast.Extent,
                                    GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                    }
                }

            }

        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingDeprecatedManifestFieldsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDeprecatedManifestFieldsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDeprecatedManifestFieldsDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
