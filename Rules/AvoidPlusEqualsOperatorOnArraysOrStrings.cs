// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// Avoid using += operator on arrays or strings.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidPlusEqualsOperatorOnArraysOrStrings : IScriptRule
    {
        /// <summary>
        /// Checks for usage of += on variables of type array.
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>The diagnostic results of this rule</returns>
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> assignmentStatementAstsUsingPlusEqualsOperator = ast.FindAll(testAst => testAst is AssignmentStatementAst assignmentStatementAst &&
                assignmentStatementAst.Operator == TokenKind.PlusEquals, searchNestedScriptBlocks: true);

            foreach (AssignmentStatementAst assignmentStatementAstUsingPlusEqualsOperator in assignmentStatementAstsUsingPlusEqualsOperator)
            {
                var variableExpressionAst = assignmentStatementAstUsingPlusEqualsOperator.Left as VariableExpressionAst;
                if (variableExpressionAst != null)
                {
                    if (variableExpressionAst.StaticType.IsArray)
                    {
                        yield return Warning(variableExpressionAst.Extent, fileName);
                    }

                    var type = Helper.Instance.GetTypeFromInternalVariableAnalysis(variableExpressionAst, ast);
                    if (type != null && type.IsArray)
                    {
                        yield return Warning(variableExpressionAst.Extent, fileName);
                    }
                }
            }
        }

        private DiagnosticRecord Warning(IScriptExtent extent, string fileName)
        {
            return new DiagnosticRecord(Strings.AvoidPlusEqualsOperatorOnArraysError, extent, GetName(), DiagnosticSeverity.Warning, fileName);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidPlusEqualsOperatorOnArraysName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidPlusEqualsOperatorOnArraysCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidPlusEqualsOperatorOnArraysDescription);
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
