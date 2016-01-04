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
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// NullComparisonRule: Analyzes the ast to check that $null is on the left side of any equality comparisons.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class PossibleIncorrectComparisonWithNull : IScriptRule {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that $null is on the left side of any equality comparisons.
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>The diagnostic results of this rule</returns>
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> binExpressionAsts = ast.FindAll(testAst => testAst is BinaryExpressionAst, false);

            foreach (BinaryExpressionAst binExpressionAst in binExpressionAsts) {
                if ((binExpressionAst.Operator.Equals(TokenKind.Equals) || binExpressionAst.Operator.Equals(TokenKind.Ceq) 
                    || binExpressionAst.Operator.Equals(TokenKind.Cne) || binExpressionAst.Operator.Equals(TokenKind.Ine) || binExpressionAst.Operator.Equals(TokenKind.Ieq))
                    && binExpressionAst.Right.Extent.Text.Equals("$null", StringComparison.OrdinalIgnoreCase)) 
                {
                    if (IncorrectComparisonWithNull(binExpressionAst, ast))
                    {
                        yield return new DiagnosticRecord(Strings.PossibleIncorrectComparisonWithNullError, binExpressionAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }

            #if PSV3

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            #else

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true).Union(ast.FindAll(item => item is FunctionMemberAst, true));

            #endif

            foreach (Ast funcAst in funcAsts)
            {
                IEnumerable<Ast> binAsts = funcAst.FindAll(item => item is BinaryExpressionAst, true);
                foreach (BinaryExpressionAst binAst in binAsts)
                {
                    if (IncorrectComparisonWithNull(binAst, funcAst))
                    {
                        yield return new DiagnosticRecord(Strings.PossibleIncorrectComparisonWithNullError, binAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }
        }

        private bool IncorrectComparisonWithNull(BinaryExpressionAst binExpressionAst, Ast ast)
        {
            if ((binExpressionAst.Operator.Equals(TokenKind.Equals) || binExpressionAst.Operator.Equals(TokenKind.Ceq) 
                || binExpressionAst.Operator.Equals(TokenKind.Cne) || binExpressionAst.Operator.Equals(TokenKind.Ine) || binExpressionAst.Operator.Equals(TokenKind.Ieq))
                && binExpressionAst.Right.Extent.Text.Equals("$null", StringComparison.OrdinalIgnoreCase)) 
            {
                if (binExpressionAst.Left.StaticType.IsArray)
                {
                    return true;
                }
                else if (binExpressionAst.Left is VariableExpressionAst)
                {
                    // ignores if the variable is a special variable
                    if (!Helper.Instance.HasSpecialVars((binExpressionAst.Left as VariableExpressionAst).VariablePath.UserPath))
                    {
                        Type lhsType = Helper.Instance.GetTypeFromAnalysis(binExpressionAst.Left as VariableExpressionAst, ast);
                        if (lhsType == null)
                        {
                            return true;
                        }
                        else if (lhsType.IsArray || lhsType == typeof(object) || lhsType == typeof(Undetermined) || lhsType == typeof(Unreached))
                        {
                            return true;
                        }
                    }
                }
                else if (binExpressionAst.Left.StaticType == typeof(object))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName() {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.PossibleIncorrectComparisonWithNullName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectComparisonWithNullCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectComparisonWithNullDescription);
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
