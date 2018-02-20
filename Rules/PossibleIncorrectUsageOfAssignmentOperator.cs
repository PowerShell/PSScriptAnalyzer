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
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// PossibleIncorrectUsageOfAssignmentOperator: Warn if someone uses '>', '=' or '==' operators inside an if or elseif statement because in most cases that is not the intention.
    /// The origin of this rule is that people often forget that operators change when switching between different languages such as C# and PowerShell.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class PossibleIncorrectUsageOfComparisonOperator : AstVisitor, IScriptRule
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
                    var ifStatementCondition = clause.Item1;
                    var assignmentStatementAst = ifStatementCondition.Find(testAst => testAst is AssignmentStatementAst, searchNestedScriptBlocks: false) as AssignmentStatementAst;
                    if (assignmentStatementAst != null)
                    {
                        // Check if someone used '==', which can easily happen when the person is used to coding a lot in C#.
                        // In most cases, this will be a runtime error because PowerShell will look for a cmdlet name starting with '=', which is technically possible to define
                        if (assignmentStatementAst.Right.Extent.Text.StartsWith("="))
                        {
                            yield return PossibleIncorrectUsageOfComparisonOperatorAssignmentOperatorError(assignmentStatementAst.ErrorPosition, fileName);
                        }
                        else
                        {
                            var leftVariableExpressionAstOfAssignment = assignmentStatementAst.Left as VariableExpressionAst;
                            if (leftVariableExpressionAstOfAssignment != null)
                            {
                                var statementBlockOfIfStatenent = clause.Item2;
                                var variableExpressionAstsInStatementBlockOfIfStatement = statementBlockOfIfStatenent.FindAll(testAst =>
                                                                                                testAst is VariableExpressionAst, searchNestedScriptBlocks: true);
                                if (variableExpressionAstsInStatementBlockOfIfStatement == null) // no variable usages at all
                                {
                                    yield return PossibleIncorrectUsageOfComparisonOperatorAssignmentOperatorError(assignmentStatementAst.ErrorPosition, fileName);
                                }
                                else
                                {
                                    var variableOnLHSIsBeingUsed = variableExpressionAstsInStatementBlockOfIfStatement.Where(x => x is VariableExpressionAst)
                                        .Any(x => ((VariableExpressionAst)x).VariablePath.UserPath.Equals(
                                            leftVariableExpressionAstOfAssignment.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase));

                                    if (!variableOnLHSIsBeingUsed)
                                    {
                                        yield return PossibleIncorrectUsageOfComparisonOperatorAssignmentOperatorError(assignmentStatementAst.ErrorPosition, fileName);
                                    }
                                }
                            }
                        }

                    }

                    var fileRedirectionAst = clause.Item1.Find(testAst => testAst is FileRedirectionAst, searchNestedScriptBlocks: false) as FileRedirectionAst;
                    if (fileRedirectionAst != null)
                    {
                        yield return new DiagnosticRecord(
                            Strings.PossibleIncorrectUsageOfComparisonOperatorFileRedirectionOperatorError, fileRedirectionAst.Extent,
                            GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }
        }

        private DiagnosticRecord PossibleIncorrectUsageOfComparisonOperatorAssignmentOperatorError(IScriptExtent errorPosition, string fileName)
        {
            return new DiagnosticRecord(Strings.PossibleIncorrectUsageOfComparisonOperatorAssignmentOperatorError, errorPosition, GetName(), DiagnosticSeverity.Warning, fileName);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.PossibleIncorrectUsageOfComparisonOperatorName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectUsageOfComparisonOperatorCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectUsageOfComparisonOperatorDescription);
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
            return RuleSeverity.Warning;
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
