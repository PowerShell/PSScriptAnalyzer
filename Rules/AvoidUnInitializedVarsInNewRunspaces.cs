// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUnInitializedVarsInNewRunspaces: Analyzes the ast to check that variables in script blocks that run in new run spaces are properly initialized or passed in with '$using:(varName)'.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidUnInitializedVarsInNewRunspaces : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check variables in script blocks that will run in new runspaces are properly initialized or passed in with $using:
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of results from this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            var scriptBlockAsts = ast.FindAll(x => x is ScriptBlockAst, true);
            if (scriptBlockAsts == null)
            {
                yield break;
            }

            foreach (var scriptBlockAst in scriptBlockAsts)
            {
                var sbAst = scriptBlockAst as ScriptBlockAst;
                foreach (var diagnosticRecord in AnalyzeScriptBlockAst(sbAst, fileName))
                {
                    yield return diagnosticRecord;
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUnInitializedVarsInNewRunspacesName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUnInitializedVarsInNewRunspacesCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUnInitializedVarsInNewRunspacesDescription);
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

        /// <summary>
        /// Checks if a variable is initialized and referenced in either its assignment or children scopes
        /// </summary>
        /// <param name="scriptBlockAst">Ast of type ScriptBlock</param>
        /// <param name="fileName">Name of file containing the ast</param>
        /// <returns>An enumerable containing diagnostic records</returns>
        private IEnumerable<DiagnosticRecord> AnalyzeScriptBlockAst(ScriptBlockAst scriptBlockAst, string fileName)
        {
            // TODO: add other Cmdlets like invoke-command later?
            var foreachObjectCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Foreach-Object");

            // Find all commandAst objects for `Foreach-Object -Parallel`. As for parameter name matching, there are three
            // parameters starting with a 'p': Parallel, PipelineVariable and Process, so we use startsWith 'pa' as the shortest unambiguous form.
            // Because we are already going trough all ScriptBlockAst objects, we do not need to look for nested script blocks here.
            var foreachObjectParallelCommandAsts = scriptBlockAst.FindAll(
                predicate: c => c is CommandAst commandAst &&
                                foreachObjectCmdletNamesAndAliases.Contains(
                                    commandAst.GetCommandName(), StringComparer.OrdinalIgnoreCase) &&
                                    commandAst.CommandElements.Any(
                                        e => e is CommandParameterAst parameterAst && 
                                             parameterAst.ParameterName.StartsWith("pa", StringComparison.OrdinalIgnoreCase)), 
                searchNestedScriptBlocks: false).Select(a=>a as CommandAst);
            
            foreach (var commandAst in foreachObjectParallelCommandAsts)
            {
                if (commandAst == null) 
                    yield break;
                
                // Find all variables that are assigned within this ScriptBlock
                var varsInAssignments = commandAst.FindAll(
                    predicate: a => a is VariableExpressionAst varExpr &&
                                    varExpr.Parent is AssignmentStatementAst assignment &&
                                    assignment.Left.Equals(varExpr),
                    searchNestedScriptBlocks: true).
                        Select(a => a as VariableExpressionAst);

                // Find all variables that are not locally assigned, and don't have $using: directive
                var nonAssignedNonUsingVars = commandAst.CommandElements.
                    SelectMany(a => a.FindAll(
                                    predicate: aa => aa is VariableExpressionAst varAst &&
                                                     !(varAst.Parent is UsingExpressionAst) &&
                                                     !varsInAssignments.Contains(varAst) &&
                                                     !Helper.Instance.HasSpecialVars(varAst.VariablePath.UserPath),
                                    searchNestedScriptBlocks: true).
                                        Select(aaa => aaa as VariableExpressionAst));

                foreach (var variableExpression in nonAssignedNonUsingVars)
                {
                    yield return new DiagnosticRecord(
                        message: string.Format(CultureInfo.CurrentCulture,
                            Strings.AvoidUnInitializedVarsInNewRunspacesError, variableExpression.ToString()),
                        extent: variableExpression.Extent,
                        ruleName: GetName(),
                        severity: DiagnosticSeverity.Warning,
                        scriptPath: fileName,
                        ruleId: variableExpression.ToString());
                }
            }
        }
    }
}
