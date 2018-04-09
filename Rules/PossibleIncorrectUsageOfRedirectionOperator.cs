// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    /// PossibleIncorrectUsageOfRedirectionOperator: Warn if someone uses '>' or '>=' inside an if, elseif, while or do-while statement because in most cases that is not the intention.
    /// The origin of this rule is that people often forget that operators change when switching between different languages such as C# and PowerShell.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class PossibleIncorrectUsageOfRedirectionOperator : AstVisitor, IScriptRule
    {
        /// <summary>
        /// The idea is to get all FileRedirectionAst inside all IfStatementAst clauses.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            var ifStatementAsts = ast.FindAll(testAst => testAst is IfStatementAst, searchNestedScriptBlocks: true);
            foreach (IfStatementAst ifStatementAst in ifStatementAsts)
            {
                foreach (var clause in ifStatementAst.Clauses)
                {
                    var fileRedirectionAst = clause.Item1.Find(testAst => testAst is FileRedirectionAst, searchNestedScriptBlocks: false) as FileRedirectionAst;
                    if (fileRedirectionAst != null)
                    {
                        yield return new DiagnosticRecord(
                            Strings.PossibleIncorrectUsageOfRedirectionOperatorError, fileRedirectionAst.Extent,
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.PossibleIncorrectUsageOfRedirectionOperatorName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectUsageOfRedirectionOperatorCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PossibleIncorrectUsageOfRedirectionOperatorDescription);
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
