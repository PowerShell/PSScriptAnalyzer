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
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideCommentHelp: Analyzes ast to check that cmdlets have help.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class ProvideCommentHelp : SkipTypeDefinition, IScriptRule
    {
        private HashSet<string> exportedFunctions;

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that cmdlets have help.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            DiagnosticRecords.Clear();
            this.fileName = fileName;
            exportedFunctions = Helper.Instance.GetExportedFunction(ast);

            ast.Visit(this);

            return DiagnosticRecords;
        }

        /// <summary>
        /// Visit function and checks that it has comment help
        /// </summary>
        /// <param name="funcAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst funcAst)
        {
            if (funcAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (exportedFunctions.Contains(funcAst.Name))
            {
                if (funcAst.GetHelpContent() == null)
                {
                    DiagnosticRecords.Add(
                        new DiagnosticRecord(
                            string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpError, funcAst.Name),
                            Helper.Instance.GetScriptExtentForFunctionName(funcAst),
                            GetName(),
                            DiagnosticSeverity.Information,
                            fileName));
                }
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ProvideCommentHelpName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.ProvideCommentHelpDescription);
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
            return RuleSeverity.Information;
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




