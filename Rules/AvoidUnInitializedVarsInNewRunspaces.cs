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
            var foreachObjectCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Foreach-Object");

            // Find all commandAst objects for `Foreach-Object -Parallel`. As for parametername matching, there are three
            // parameters starting with a 'p': Parallel, PipelineVariable and Process, so we use startsWith 'pa' as the shortest unambiguous form.
            // Because we are already going trough all ScriptBlockAst objects, we do not need to look for nested script blocks here.
            if (!(scriptBlockAst.FindAll(
                predicate: c => c is CommandAst commandAst &&
                                foreachObjectCmdletNamesAndAliases.Contains(commandAst.GetCommandName(), StringComparer.OrdinalIgnoreCase) &&
                                commandAst.CommandElements.Any(
                                    e => e is CommandParameterAst parameterAst && 
                                         parameterAst.ParameterName.StartsWith("pa", StringComparison.OrdinalIgnoreCase)), 
                searchNestedScriptBlocks: true) is IEnumerable<Ast> foreachObjectParallelAsts))
            {
                yield break;
            }

            foreach (var ast in foreachObjectParallelAsts)
            {
                var commandAst = ast as CommandAst;

                if (commandAst == null)
                {
                    continue;
                }

                var varsInAssignments = commandAst.FindAll(
                    predicate: a => a is AssignmentStatementAst assignment && 
                                    assignment.Left.FindAll(
                                        predicate: aa => aa is VariableExpressionAst, 
                                        searchNestedScriptBlocks: true) != null, 
                    searchNestedScriptBlocks: true);

                var commandElements = commandAst.CommandElements;
                var nonAssignedNonUsingVars = new List<Ast>() { };
                foreach (var cmdEl in commandElements)
                {
                    nonAssignedNonUsingVars.AddRange(
                        cmdEl.FindAll(
                            predicate: aa => aa is VariableExpressionAst varAst && 
                            !(varAst.Parent is UsingExpressionAst) &&
                            !varsInAssignments.Contains(varAst), true));
                }

                foreach (var variableExpression in nonAssignedNonUsingVars)
                {
                    var _temp  = variableExpression as VariableExpressionAst;

                    yield return new DiagnosticRecord(
                        message: string.Format(CultureInfo.CurrentCulture,
                            Strings.UseDeclaredVarsMoreThanAssignmentsError, _temp?.ToString()),
                        extent: _temp?.Extent,
                        ruleName: GetName(),
                        severity: DiagnosticSeverity.Warning,
                        scriptPath: fileName,
                        ruleId: _temp?.ToString());
                }
            }
        }
    }
}
