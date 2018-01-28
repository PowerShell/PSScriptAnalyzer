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
        List<DiagnosticRecord> records;
        string fileName;

        /// <summary>
        /// AnalyzeScript: 
        /// The idea is to get all AssignmentStatementAsts and then check if the parent is an IfStatementAst, which includes if, elseif and else statements.
        /// This is just a simple smoke check to catch cases like 'if (($a=$b)){}' but was not made too complex avoid false positives.
        /// It does not catch cases with expressions inside the if staement like e.g. 'if (($a=$b)){}'
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> ifStatementAsts = ast.FindAll(testAst => testAst is AssignmentStatementAst, searchNestedScriptBlocks: true);
            foreach (var aast in ifStatementAsts)
            {
                if (aast.Parent is IfStatementAst)
                {
                    yield return new DiagnosticRecord(
                        Strings.PossibleIncorrectUsageOfAssignmentOperatorError, aast.Extent,
                        GetName(), DiagnosticSeverity.Information, fileName);
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
