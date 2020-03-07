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
    /// UseUsingScopeModifierInNewRunspaces: Analyzes the ast to check that variables in script blocks that run in new run spaces are properly initialized or passed in with '$using:(varName)'.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseUsingScopeModifierInNewRunspaces : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check variables in script blocks that will run in new runspaces are properly initialized or passed in with $using:
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of results from this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            
            var visitor = new SyntaxCompatibilityVisitor(this, fileName);
            ast.Visit(visitor);
            return visitor.GetDiagnosticRecords();
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(),
                Strings.UseUsingScopeModifierInNewRunspacesName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseUsingScopeModifierInNewRunspacesCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseUsingScopeModifierInNewRunspacesDescription);
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

#if !(PSV3 || PSV4)
    internal class SyntaxCompatibilityVisitor : AstVisitor2
#else
        private class SyntaxCompatibilityVisitor : AstVisitor
#endif
    {
        // Is there a way to make sure this is only called when needed?
        private readonly IEnumerable<string> _jobCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Start-Job");

        private readonly IEnumerable<string> _threadJobCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Start-ThreadJob");

        private readonly IEnumerable<string> _inlineScriptCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("InlineScript");

        private readonly IEnumerable<string> _foreachObjectCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Foreach-Object");

        private readonly IEnumerable<string> _invokeCommandCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Invoke-Command");

        private readonly UseUsingScopeModifierInNewRunspaces _rule;

        private readonly List<DiagnosticRecord> _diagnosticAccumulator;
        
        private readonly List<VariableExpressionAst> _nonAssignedNonUsingVars = new List<VariableExpressionAst>();
        
        private readonly string _analyzedFilePath;

        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;


        public SyntaxCompatibilityVisitor(UseUsingScopeModifierInNewRunspaces rule, string analyzedScriptPath)
        {
            _diagnosticAccumulator = new List<DiagnosticRecord>();
            _rule = rule;
            _analyzedFilePath = analyzedScriptPath;
        }

        public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
        {
            return _diagnosticAccumulator;
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            if (!(scriptBlockExpressionAst.Parent is CommandAst commandAst))
                return AstVisitAction.Continue;

            var cmdName = commandAst.GetCommandName();
            var scriptBlockParameterAst = commandAst.CommandElements[
                    commandAst.CommandElements.IndexOf(scriptBlockExpressionAst) - 1] as
                CommandParameterAst;

            if (!IsInlineScriptBlock(cmdName) && 
                !IsJobScriptBlock(cmdName, scriptBlockParameterAst) &&
                !IsForeachScriptBlock(cmdName, scriptBlockParameterAst) &&
                !IsInvokeCommandComputerScriptBlock(cmdName, commandAst) &&
                !IsInvokeCommandSessionScriptBlock(cmdName, commandAst))
                return AstVisitAction.Continue;
            
            AnalyzeScriptBlock(scriptBlockExpressionAst);
            
            return AstVisitAction.SkipChildren;
        }

        private bool IsInvokeCommandSessionScriptBlock(string cmdName, CommandAst commandAst)
        {
            //TODO: finish refactor of invoke-command -session stuff
            
            //nonAssignedNonUsingVarAsts.AddRange(
            //    ProcessInvokeCommandSessionAsts(scriptBlockExpressionAst));
            return false;
        }

        private bool IsInvokeCommandComputerScriptBlock(string cmdName, CommandAst commandAst)
        {
            return _invokeCommandCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) &&
                   commandAst.CommandElements.Any(
                       e => e is CommandParameterAst parameterAst &&
                            parameterAst.ParameterName.StartsWith("com", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsForeachScriptBlock(string cmdName, CommandParameterAst scriptBlockParameterAst)
        {
            return _foreachObjectCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) &&
                   (scriptBlockParameterAst != null && scriptBlockParameterAst.ParameterName.StartsWith("pa", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsJobScriptBlock(string cmdName, CommandParameterAst scriptBlockParameterAst)
        {
            return (_jobCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) ||
                    _threadJobCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase)) &&
                   !(scriptBlockParameterAst != null && scriptBlockParameterAst.ParameterName.StartsWith("ini", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsInlineScriptBlock(string cmdName)
        {
            return _inlineScriptCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase);
        }

        private void AnalyzeScriptBlock(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            var nonAssignedNonUsingVarAsts = FindNonAssignedNonUsingVarAsts(scriptBlockExpressionAst,
                FindVarsInAssignmentAsts(scriptBlockExpressionAst)).ToList();

            foreach (var variableExpression in nonAssignedNonUsingVarAsts)
            {
                if (variableExpression == null)
                    continue;

                _diagnosticAccumulator.Add(new DiagnosticRecord(
                    message: string.Format(CultureInfo.CurrentCulture,
                        Strings.UseUsingScopeModifierInNewRunspacesError, variableExpression.ToString()),
                    extent: variableExpression.Extent,
                    ruleName: _rule.GetName(),
                    severity: Severity,
                    scriptPath: _analyzedFilePath,
                    ruleId: variableExpression.ToString(),
                    suggestedCorrections: GetSuggestedCorrections(ast: variableExpression)));
            }
        }

        private static IEnumerable<VariableExpressionAst> FindVarsInAssignmentAsts(Ast ast)
        {
            // Find all variables that are assigned within this ast
            return ast.FindAll(
                    predicate: a => a is VariableExpressionAst varExpr &&
                                    varExpr.Parent is AssignmentStatementAst assignment &&
                                    assignment
                                        .Left
                                        .Equals(varExpr),
                    searchNestedScriptBlocks: true)
                .Select(a => a as VariableExpressionAst);
        }

        private static IEnumerable<VariableExpressionAst> FindNonAssignedNonUsingVarAsts(
            Ast ast, IEnumerable<VariableExpressionAst> varsInAssignments)
        {
            // Find all variables that are not locally assigned, and don't have $using: scope modifier
            return ast.FindAll(
                    predicate: a => a is VariableExpressionAst varAst &&
                                    !(varAst.Parent is UsingExpressionAst) &&
                                    varsInAssignments.All(
                                        b => b.VariablePath.UserPath != varAst.VariablePath.UserPath) &&
                                    !Helper
                                        .Instance
                                        .HasSpecialVars(varAst.VariablePath.UserPath),
                    searchNestedScriptBlocks: true)
                .Select(a => a as VariableExpressionAst);
        }

        private static IEnumerable<CorrectionExtent> GetSuggestedCorrections(VariableExpressionAst ast)
        {
            var varWithUsing = "$using:" + ast.VariablePath.UserPath;
            var description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseUsingScopeModifierInNewRunspacesCorrectionDescription,
                ast.Extent.Text,
                varWithUsing);
            var corrections = new List<CorrectionExtent>()
            {
                new CorrectionExtent(
                    startLineNumber: ast.Extent.StartLineNumber,
                    endLineNumber: ast.Extent.EndLineNumber,
                    startColumnNumber: ast.Extent.StartColumnNumber,
                    endColumnNumber: ast.Extent.EndColumnNumber,
                    text: varWithUsing,
                    file: ast.Extent.File,
                    description: description
                )
            };

            return corrections;
        }

        private static IEnumerable<VariableExpressionAst> ProcessInvokeCommandSessionAsts(Ast scriptBlockAst)
        {
            // TODO: finish refactor to reanimate this logic
            
            var invokeCommandCmdletNamesAndAliases = Helper.Instance.CmdletNameAndAliases("Invoke-Command");
            var sessionDictionary = new Dictionary<string, List<Ast>>();
            var result = new List<VariableExpressionAst>();

            // The shortest unambiguous name for parameter -Session is 'session' (SessionName and SessionOption) also exist.
            var scriptBlockExpressionAstsToAnalyze = scriptBlockAst.FindAll(
                predicate: a => a is ScriptBlockExpressionAst scriptBlockExpressionAst &&
                                scriptBlockExpressionAst.Parent is CommandAst commandAst &&
                                invokeCommandCmdletNamesAndAliases
                                    .Contains(commandAst.GetCommandName(), StringComparer.OrdinalIgnoreCase) &&
                                commandAst.CommandElements.Any(
                                    e => e is CommandParameterAst parameterAst &&
                                         parameterAst.ParameterName.Equals(
                                             "session",
                                             StringComparison
                                                 .OrdinalIgnoreCase)), 
                searchNestedScriptBlocks: false);

            // Match ScriptBlocks together that belong to the same session
            foreach (var ast in scriptBlockExpressionAstsToAnalyze)
            {
                if (!(ast.Parent is CommandAst commandAst))
                    continue;

                // Find session parameter
                //   The shortest unambiguous name for parameter -Session is 'session' (SessionName and SessionOption) also exist.
                if (!(commandAst.CommandElements.First<Ast>(
                        e => e is CommandParameterAst parameterAst &&
                             parameterAst.
                                 ParameterName.
                                 Equals(
                                     "session",
                                     StringComparison
                                         .OrdinalIgnoreCase)) is CommandParameterAst sessionParameterAst))
                    continue;

                // Group script blocks by session name in a dictionary
                var sessionParamAstIndex = commandAst.CommandElements.IndexOf(sessionParameterAst);
                if (commandAst.CommandElements.Count <= sessionParamAstIndex)
                    continue;

                var sessionName = commandAst
                    .CommandElements[sessionParamAstIndex + 1]
                    .Extent
                    .Text
                    .Trim();

                if (!sessionDictionary.ContainsKey(sessionName))
                    sessionDictionary.Add(sessionName, new List<Ast>());

                sessionDictionary[sessionName].Add(ast);
            }


            foreach (var session in sessionDictionary)
            {
                // Find all variables that are assigned within these ScriptBlocks that are part of one session
                var varsInAssignments = new List<VariableExpressionAst>();
                foreach (var ast in session.Value)
                {
                    varsInAssignments.AddRange(
                        FindVarsInAssignmentAsts(ast));
                }

                // Find all variables that are not locally assigned, and don't have $using: scope modifier
                foreach (var ast in session.Value)
                {
                    result.AddRange(
                        FindNonAssignedNonUsingVarAsts(ast, varsInAssignments));
                }
            }

            return result;
        }
    }
}
