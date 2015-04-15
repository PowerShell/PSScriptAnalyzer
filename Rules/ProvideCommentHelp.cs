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
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Management.Automation;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ProvideCommentHelp: Analyzes ast to check that cmdlets have help.
    /// </summary>
    [Export(typeof(IScriptRule))]
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
            exportedFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> exportFunctionsCmdlet = Helper.Instance.CmdletNameAndAliases("export-modulemember");

            // find functions exported
            IEnumerable<Ast> cmdAsts = ast.FindAll(item => item is CommandAst
                && exportFunctionsCmdlet.Contains((item as CommandAst).GetCommandName(), StringComparer.OrdinalIgnoreCase), true);

            CommandInfo exportMM = Helper.Instance.GetCommandInfo("export-modulemember", CommandTypes.Cmdlet);

            // switch parameters
            IEnumerable<ParameterMetadata> switchParams = (exportMM != null) ? exportMM.Parameters.Values.Where<ParameterMetadata>(pm => pm.SwitchParameter) : Enumerable.Empty<ParameterMetadata>();

            if (exportMM == null)
            {
                return DiagnosticRecords;
            }

            foreach (CommandAst cmdAst in cmdAsts)
            {
                if (cmdAst.CommandElements == null || cmdAst.CommandElements.Count < 2)
                {
                    continue;
                }

                int i = 1;

                while (i < cmdAst.CommandElements.Count)
                {
                    CommandElementAst ceAst = cmdAst.CommandElements[i];
                    ExpressionAst exprAst = null;

                    if (ceAst is CommandParameterAst)
                    {
                        var paramAst = ceAst as CommandParameterAst;
                        var param = exportMM.ResolveParameter(paramAst.ParameterName);

                        if (param == null)
                        {
                            i += 1;
                            continue;
                        }

                        if (string.Equals(param.Name, "function", StringComparison.OrdinalIgnoreCase))
                        {
                            // checks for the case of -Function:"verb-nouns"
                            if (paramAst.Argument != null)
                            {
                                exprAst = paramAst.Argument;
                            }
                            // checks for the case of -Function "verb-nouns"
                            else if (i < cmdAst.CommandElements.Count - 1)
                            {
                                i += 1;
                                exprAst = cmdAst.CommandElements[i] as ExpressionAst;
                            }
                        }
                        // some other parameter. we just checks whether the one after this is positional
                        else if (i < cmdAst.CommandElements.Count - 1)
                        {
                            // the next element is a parameter like -module so just move to that one
                            if (cmdAst.CommandElements[i + 1] is CommandParameterAst)
                            {
                                i += 1;
                                continue;
                            }

                            // not a switch parameter so the next element is definitely the argument to this parameter
                            if (paramAst.Argument == null && !switchParams.Contains(param))
                            {
                                // skips the next element
                                i += 1;
                            }

                            i += 1;
                            continue;
                        }
                    }
                    else if (ceAst is ExpressionAst)
                    {
                        exprAst = ceAst as ExpressionAst;
                    }

                    if (exprAst != null)
                    {
                        // One string so just add this to the list
                        if (exprAst is StringConstantExpressionAst)
                        {
                            exportedFunctions.Add((exprAst as StringConstantExpressionAst).Value);
                        }
                        // Array of the form "v-n", "v-n1"
                        else if (exprAst is ArrayLiteralAst)
                        {
                            exportedFunctions.UnionWith(Helper.Instance.GetStringsFromArrayLiteral(exprAst as ArrayLiteralAst));
                        }
                        // Array of the form @("v-n", "v-n1")
                        else if (exprAst is ArrayExpressionAst)
                        {
                            ArrayExpressionAst arrExAst = exprAst as ArrayExpressionAst;
                            if (arrExAst.SubExpression != null && arrExAst.SubExpression.Statements != null)
                            {
                                foreach (StatementAst stAst in arrExAst.SubExpression.Statements)
                                {
                                    if (stAst is PipelineAst)
                                    {
                                        PipelineAst pipeAst = stAst as PipelineAst;
                                        if (pipeAst.PipelineElements != null)
                                        {
                                            foreach (CommandBaseAst cmdBaseAst in pipeAst.PipelineElements)
                                            {
                                                if (cmdBaseAst is CommandExpressionAst)
                                                {
                                                    exportedFunctions.UnionWith(Helper.Instance.GetStringsFromArrayLiteral((cmdBaseAst as CommandExpressionAst).Expression as ArrayLiteralAst));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    i += 1;
                }
            }

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
                            funcAst.Extent, GetName(), DiagnosticSeverity.Information, fileName));
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
