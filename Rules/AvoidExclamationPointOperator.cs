// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidExclamationPointOperator: Checks for use of the exclamation point operator
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidExclamationPointOperator : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidExclamationPointOperator type.
        /// </summary>
        public AvoidExclamationPointOperator() {
            Enable = false;
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            var diagnosticRecords = new List<DiagnosticRecord>();

            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is UnaryExpressionAst, true);
            if (foundAsts != null) {
                var CorrectionDescription = Strings.AvoidExclamationPointOperatorCorrectionDescription;
                foreach (UnaryExpressionAst foundAst in foundAsts) {
                    if (foundAst.TokenKind == TokenKind.Exclaim) {
                        // If the exclaim is not followed by a space, add one
                        var replaceWith = "-not";
                        if (foundAst.Child != null && foundAst.Child.Extent.StartColumnNumber == foundAst.Extent.StartColumnNumber + 1) {
                            replaceWith = "-not ";
                        }
                        var corrections = new List<CorrectionExtent> {
                            new CorrectionExtent(
                                foundAst.Extent.StartLineNumber,
                                foundAst.Extent.EndLineNumber,
                                foundAst.Extent.StartColumnNumber,
                                foundAst.Extent.StartColumnNumber + 1,
                                replaceWith,
                                fileName,
                                CorrectionDescription
                            )
                        };
                        diagnosticRecords.Add(new DiagnosticRecord(
                                string.Format(
                                    CultureInfo.CurrentCulture, 
                                    Strings.AvoidExclamationPointOperatorError
                                ), 
                                foundAst.Extent, 
                                GetName(),
                                GetDiagnosticSeverity(), 
                                fileName,
                                suggestedCorrections: corrections
                        ));
                    }
                }
            }
            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidExclamationPointOperatorCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidExclamationPointOperatorDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidExclamationPointOperatorName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
