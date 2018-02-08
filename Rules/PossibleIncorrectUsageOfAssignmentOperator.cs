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

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Management.Automation.Language;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// PossibleIncorrectUsageOfAssignmentOperator: Warn if someone uses the '=' or '==' by accident in an if statement because in most cases that is not the intention.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class PossibleIncorrectUsageOfAssignmentOperator : AstVisitor, IScriptRule
    {
        /// <summary>
        /// The idea is to get all AssignmentStatementAsts and then check if the parent is an IfStatementAst, which includes if, elseif and else statements.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            var ifStatementAsts = ast.FindAll(testAst => testAst is IfStatementAst, searchNestedScriptBlocks: true);
            foreach (IfStatementAst ifStatementAst in ifStatementAsts)
            {
                foreach (var clause in ifStatementAst.Clauses)
                {
                    var assignmentStatementAst = clause.Item1.Find(testAst => testAst is AssignmentStatementAst, searchNestedScriptBlocks: false) as AssignmentStatementAst;
                    if (assignmentStatementAst != null)
                    {
                        // Check if someone used '==', which can easily happen when the person is used to coding a lot in C#.
                        // In most cases, this will be a runtime error because PowerShell will look for a cmdlet name starting with '=', which is technically possible to define
                        if (assignmentStatementAst.Right.Extent.Text.StartsWith("="))
                        {
                            yield return new DiagnosticRecord(
                                Strings.PossibleIncorrectUsageOfAssignmentOperatorError, assignmentStatementAst.Extent,
                                GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                        else
                        {
                            // If the right hand side contains a CommandAst at some point, then we do not want to warn
                            // because this could be intentional in cases like 'if ($a = Get-ChildItem){ }'
                            var commandAst = assignmentStatementAst.Right.Find(testAst => testAst is CommandAst, searchNestedScriptBlocks: true) as CommandAst;
                            if (commandAst == null)
                            {
                                yield return new DiagnosticRecord(
                                   Strings.PossibleIncorrectUsageOfAssignmentOperatorError, assignmentStatementAst.Extent,
                                   GetName(), DiagnosticSeverity.Information, fileName);
                            }
                        }
                    }

                    var fileRedirectionAst = clause.Item1.Find(testAst => testAst is FileRedirectionAst, searchNestedScriptBlocks: false) as FileRedirectionAst;
                    if (fileRedirectionAst != null)
                    {
                        yield return new DiagnosticRecord(
                            Strings.PossibleIncorrectUsageOfFileRedirectionOperatorError, fileRedirectionAst.Extent, // TODO: better error message string and rename rule
                            GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.PossibleIncorrectUsageOfAssignmentOperatorName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectUsageOfAssignmentOperatorCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWriteHostDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
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
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
