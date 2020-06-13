// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// ReviewUnusedParameter: Check that all declared parameters are used in the script body.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class ReviewUnusedParameter : IScriptRule
    {
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(oneAst => oneAst is ScriptBlockAst, true);
            if (scriptBlockAsts == null)
            {
                yield break;
            }

            foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts)
            {
                // bail out if PS bound parameter used.
                if (scriptBlockAst.Find(IsBoundParametersReference, searchNestedScriptBlocks: false) != null)
                {
                    continue;
                }

                // find all declared parameters
                IEnumerable<Ast> parameterAsts = scriptBlockAst.FindAll(oneAst => oneAst is ParameterAst, false);

                // list all variables
                IDictionary<string, int> variableCount = scriptBlockAst.FindAll(oneAst => oneAst is VariableExpressionAst, false)
                    .Select(variableExpressionAst => ((VariableExpressionAst)variableExpressionAst).VariablePath.UserPath)
                    .GroupBy(variableName => variableName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(variableName => variableName.Key, variableName => variableName.Count(), StringComparer.OrdinalIgnoreCase);

                foreach (ParameterAst parameterAst in parameterAsts)
                {
                    // there should be at least two usages of the variable since the parameter declaration counts as one
                    variableCount.TryGetValue(parameterAst.Name.VariablePath.UserPath, out int variableUsageCount);
                    if (variableUsageCount >= 2)
                    {
                        continue;
                    }

                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.ReviewUnusedParameterError, parameterAst.Name.VariablePath.UserPath),
                        parameterAst.Name.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        parameterAst.Name.VariablePath.UserPath
                    );
                }
            }
        }

        /// <summary>
        /// Checks for PS bound parameter reference.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <returns>Boolean true indicating that given AST has PS bound parameter reference, otherwise false</returns>
        private static bool IsBoundParametersReference(Ast ast)
        {
            // $PSBoundParameters
            if (ast is VariableExpressionAst variableAst
                && variableAst.VariablePath.UserPath.Equals("PSBoundParameters", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (ast is MemberExpressionAst memberAst
                && memberAst.Member is StringConstantExpressionAst memberStringAst
                && memberStringAst.Value.Equals("BoundParameters", StringComparison.OrdinalIgnoreCase))
            {
                // $MyInvocation.BoundParameters
                if (memberAst.Expression is VariableExpressionAst veAst
                    && veAst.VariablePath.UserPath.Equals("MyInvocation", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // $PSCmdlet.MyInvocation.BoundParameters
                if (memberAst.Expression is MemberExpressionAst meAstNested)
                {
                    if (meAstNested.Expression is VariableExpressionAst veAstNested
                        && veAstNested.VariablePath.UserPath.Equals("PSCmdlet", StringComparison.OrdinalIgnoreCase)
                        && meAstNested.Member is StringConstantExpressionAst sceAstNested
                        && sceAstNested.Value.Equals("MyInvocation", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ReviewUnusedParameterName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReviewUnusedParameterCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReviewUnusedParameterDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, builtin, managed or module.
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
