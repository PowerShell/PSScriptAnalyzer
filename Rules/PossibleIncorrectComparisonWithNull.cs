using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
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

            IEnumerable<Ast> binExpressionAsts = ast.FindAll(testAst => testAst is BinaryExpressionAst, true);

            if (binExpressionAsts != null) {
                foreach (BinaryExpressionAst binExpressionAst in binExpressionAsts) {
                    if ((binExpressionAst.Operator.Equals(TokenKind.Equals) || binExpressionAst.Operator.Equals(TokenKind.Ceq) 
                        || binExpressionAst.Operator.Equals(TokenKind.Cne) || binExpressionAst.Operator.Equals(TokenKind.Ine) || binExpressionAst.Operator.Equals(TokenKind.Ieq))
                        && binExpressionAst.Right.Extent.Text.Equals("$null", StringComparison.OrdinalIgnoreCase)) {
                            yield return new DiagnosticRecord(Strings.PossibleIncorrectComparisonWithNullError, binExpressionAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }
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
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }

}
