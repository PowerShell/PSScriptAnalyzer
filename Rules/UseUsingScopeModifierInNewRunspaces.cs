// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
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

#if !(PSV3 || PSV4)
        private class SyntaxCompatibilityVisitor : AstVisitor2
#else
        private class SyntaxCompatibilityVisitor : AstVisitor
#endif
        {
            private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

            private static readonly string[] s_dscScriptResourceCommandNames = {"GetScript", "TestScript", "SetScript"};

            private static readonly IEnumerable<string> s_jobCmdletNamesAndAliases =
                Helper.Instance.CmdletNameAndAliases("Start-Job");

            private static readonly IEnumerable<string> s_threadJobCmdletNamesAndAliases =
                Helper.Instance.CmdletNameAndAliases("Start-ThreadJob");

            private static readonly IEnumerable<string> s_inlineScriptCmdletNamesAndAliases =
                Helper.Instance.CmdletNameAndAliases("InlineScript");

            private static readonly IEnumerable<string> s_foreachObjectCmdletNamesAndAliases =
                Helper.Instance.CmdletNameAndAliases("Foreach-Object");

            private static readonly IEnumerable<string> s_invokeCommandCmdletNamesAndAliases =
                Helper.Instance.CmdletNameAndAliases("Invoke-Command");

            private readonly Dictionary<string, HashSet<string>> _varsDeclaredPerSession;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;
        
            private readonly UseUsingScopeModifierInNewRunspaces _rule;
        
            private readonly string _analyzedFilePath;
        
            public SyntaxCompatibilityVisitor(UseUsingScopeModifierInNewRunspaces rule, string analyzedScriptPath)
            {
                _diagnosticAccumulator = new List<DiagnosticRecord>();
                _varsDeclaredPerSession = new Dictionary<string, HashSet<string>>();
                _rule = rule;
                _analyzedFilePath = analyzedScriptPath;
            }

            /// <summary>
            /// GetDiagnosticRecords: Retrieves all Diagnostic Records that were generated during visiting
            /// </summary>
            public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }

            /// <summary>
            /// VisitScriptBlockExpression: When a ScriptBlockExpression is visited, see if it belongs to a command that needs its variables
            /// prefixed with the 'Using' scope modifier. If so, analyze the block and generate diagnostic records for variables where it is missing.
            /// </summary>
            /// <param name="scriptBlockExpressionAst"></param>
            /// <returns>
            /// AstVisitAction.Continue or AstVisitAction.SkipChildren, depending on what we found. Diagnostic records are saved in `_diagnosticAccumulator`.
            /// </returns>
            public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
            {
                if (!(scriptBlockExpressionAst.Parent is CommandAst commandAst))
                {
                    return AstVisitAction.Continue;
                }

                string cmdName = commandAst.GetCommandName();
                if (cmdName == null)
                {
                    // Skip for situations where command name cannot be resolved like `& $commandName -ComputerName -ScriptBlock { $foo }` 
                    return AstVisitAction.SkipChildren;
                }
                
                // We need this information, because some cmdlets can have more than one ScriptBlock parameter
                var scriptBlockParameterAst = commandAst.CommandElements[
                        commandAst.CommandElements.IndexOf(scriptBlockExpressionAst) - 1] as CommandParameterAst;

                if (IsInlineScriptBlock(cmdName) || 
                    IsJobScriptBlock(cmdName, scriptBlockParameterAst) ||
                    IsForeachScriptBlock(cmdName, scriptBlockParameterAst) ||
                    IsInvokeCommandComputerScriptBlock(cmdName, commandAst) ||
                    IsDSCScriptResource(cmdName, commandAst))
                {
                    AnalyzeScriptBlock(scriptBlockExpressionAst);
                    return AstVisitAction.SkipChildren;
                }

                if (IsInvokeCommandSessionScriptBlock(cmdName, commandAst))
                {
                    if (!TryGetSessionNameFromInvokeCommand(commandAst, out var sessionName))
                    {
                        return AstVisitAction.Continue;
                    }

                    IReadOnlyCollection<string> varsInLocalAssignments = FindVarsInAssignmentAsts(scriptBlockExpressionAst);
                    if (varsInLocalAssignments != null)
                    {
                        AddAssignedVarsToSession(sessionName, varsInLocalAssignments);
                    }

                    GenerateDiagnosticRecords(
                        FindNonAssignedNonUsingVarAsts(
                            scriptBlockExpressionAst,
                            GetAssignedVarsInSession(sessionName)));

                    return AstVisitAction.SkipChildren;
                }
                
                return AstVisitAction.Continue;
            }

            /// <summary>
            /// FindVarsInAssignmentAsts: Retrieves all assigned variables from an Ast:
            /// Example: `$foo = "foo"` ==> the VariableExpressionAst for $foo is returned
            /// </summary>
            /// <param name="ast"></param>
            private static IReadOnlyCollection<string> FindVarsInAssignmentAsts(Ast ast)
            {
                HashSet<string> variableDictionary =
                    new HashSet<string>();

                // Find all variables that are assigned within this ast
                foreach (AssignmentStatementAst statementAst in ast.FindAll(IsAssignmentStatementAst, true))
                {
                    if (TryGetVariableFromExpression(statementAst.Left, out VariableExpressionAst variable))
                    {
                        string variableName = string.Format(variable.VariablePath.UserPath,
                            StringComparer.OrdinalIgnoreCase);
                        variableDictionary.Add(variableName);
                    }
                };

                return variableDictionary;
            }

            /// <summary>
            /// TryGetVariableFromExpression: extracts the variable from an expression like an assignment
            /// </summary>
            /// <param name="expression"></param>
            /// <param name="variableExpressionAst"></param>
            private static bool TryGetVariableFromExpression(ExpressionAst expression, out VariableExpressionAst variableExpressionAst)
            {
                switch (expression)
                {
                    case VariableExpressionAst variable:
                        variableExpressionAst = variable;
                        return true; 

                    case AttributedExpressionAst attributedAst:
                        return TryGetVariableFromExpression(attributedAst.Child, out variableExpressionAst);

                    default:
                        variableExpressionAst = null;
                        return false;
                }
            }

            /// <summary>
            /// IsAssignmentStatementAst: helper function to prevent allocation of closures for FindAll predicate.
            /// </summary>
            /// <param name="ast"></param>
            private static bool IsAssignmentStatementAst(Ast ast)
            {
                return ast is AssignmentStatementAst;
            }

            /// <summary>
            /// FindNonAssignedNonUsingVarAsts: Retrieve variables that are:
            /// - not assigned before
            /// - not prefixed with the 'Using' scope modifier
            /// - not a PowerShell special variable
            /// </summary>
            /// <param name="ast"></param>
            /// <param name="varsInAssignments"></param>
            private static IEnumerable<VariableExpressionAst> FindNonAssignedNonUsingVarAsts(
                Ast ast, IReadOnlyCollection<string> varsInAssignments)
            {
                // Find all variables that are not locally assigned, and don't have $using: scope modifier
                foreach (VariableExpressionAst variable in ast.FindAll(IsNonUsingNonSpecialVariableExpressionAst, true))
                {
                    var varName = string.Format(variable.VariablePath.UserPath, StringComparer.OrdinalIgnoreCase);
                    
                    if (varsInAssignments.Contains(varName))
                    {
                        yield break;    
                    }
                    
                    yield return variable;
                }
            }

            /// <summary>
            /// IsNonUsingNonSpecialVariableExpressionAst: helper function to prevent allocation of closures for FindAll predicate.
            /// </summary>
            /// <param name="ast"></param>
            private static bool IsNonUsingNonSpecialVariableExpressionAst(Ast ast)
            {
                return ast is VariableExpressionAst variable && 
                       !(variable.Parent is UsingExpressionAst) && 
                       !Helper.Instance.HasSpecialVars(variable.VariablePath.UserPath);
            }

            /// <summary>
            /// GetSuggestedCorrections: Retrieves a CorrectionExtent for a given variable
            /// </summary>
            /// <param name="ast"></param>
            private static IEnumerable<CorrectionExtent> GetSuggestedCorrections(VariableExpressionAst ast)
            {
                string varWithUsing = $"$using:{ast.VariablePath.UserPath}";

                var description = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UseUsingScopeModifierInNewRunspacesCorrectionDescription,
                    ast.Extent.Text,
                    varWithUsing);

                return new[]
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
            }

            /// <summary>
            /// TryGetSessionNameFromInvokeCommand:  Retrieves the name of the session (that Invoke-Command is run with).
            /// </summary>
            /// <param name="invokeCommandAst"></param>
            /// <param name="sessionName"></param>
            /// <returns></returns>
            private static bool TryGetSessionNameFromInvokeCommand(CommandAst invokeCommandAst, out string sessionName)
            {
                // Sift through Invoke-Command parameters to find the value of the -Session parameter
                // Start at 1 to skip the command name
                for (int i = 1; i < invokeCommandAst.CommandElements.Count; i++)
                {
                    // We need a parameter
                    if (!(invokeCommandAst.CommandElements[i] is CommandParameterAst parameterAst))
                    {
                        continue;
                    }

                    // The parameter must be called "Session"
                    if (!parameterAst.ParameterName.Equals("Session", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // If we have a partial AST, ensure we don't crash
                    if (i + 1 >= invokeCommandAst.CommandElements.Count)
                    {
                        break;
                    }

                    // The -Session parameter expects an argument of type [System.Management.Automation.Runspaces.PSSession].
                    // It will typically be provided in the form of a variable. We do not support other scenarios at this time.
                    if (!(invokeCommandAst.CommandElements[i + 1] is VariableExpressionAst variableAst))
                    {
                        break;
                    }

                    sessionName = variableAst.VariablePath.UserPath;
                    return true;
                }

                sessionName = null;
                return false;
            }

            /// <summary>
            /// GetAssignedVarsInSession: Retrieves all previously declared vars for a given session (as in Invoke-Command -Session $session).
            /// </summary>
            /// <param name="sessionName"></param>
            private IReadOnlyCollection<string> GetAssignedVarsInSession(string sessionName)
            {
                return _varsDeclaredPerSession[sessionName];
            }

            /// <summary>
            /// AddAssignedVarsToSession: Adds variables to the list of assigned variables for a given Invoke-Command session.
            /// </summary>
            /// <param name="sessionName"></param>
            /// <param name="variablesToAdd"></param>
            private void AddAssignedVarsToSession(string sessionName, IReadOnlyCollection<string> variablesToAdd)
            {
                if (!_varsDeclaredPerSession.ContainsKey(sessionName))
                {
                    _varsDeclaredPerSession.Add(sessionName, new HashSet<string>());
                }

                foreach (var item in variablesToAdd)
                {
                    if (!_varsDeclaredPerSession[sessionName].Contains(item))
                    {
                        _varsDeclaredPerSession[sessionName].Add(item);
                    }
                }
            }

            /// <summary>
            /// AnalyzeScriptBlock: Generate a Diagnostic Record for each incorrectly used variable inside a given ScriptBlock.
            /// </summary>
            /// <param name="scriptBlockExpressionAst"></param>
            private void AnalyzeScriptBlock(ScriptBlockExpressionAst scriptBlockExpressionAst)
            {
                IEnumerable<VariableExpressionAst> nonAssignedNonUsingVarAsts = FindNonAssignedNonUsingVarAsts(
                    scriptBlockExpressionAst,
                    FindVarsInAssignmentAsts(scriptBlockExpressionAst));

                GenerateDiagnosticRecords(nonAssignedNonUsingVarAsts);
            }

            /// <summary>
            /// GenerateDiagnosticRecords: Add Diagnostic Records to the internal list for each given variable
            /// </summary>
            /// <param name="nonAssignedNonUsingVarAsts"></param>
            private void GenerateDiagnosticRecords(IEnumerable<VariableExpressionAst> nonAssignedNonUsingVarAsts)
            {
                foreach (VariableExpressionAst variableExpression in nonAssignedNonUsingVarAsts)
                {
                    string diagnosticMessage = string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.UseUsingScopeModifierInNewRunspacesError,
                        variableExpression.ToString());

                    _diagnosticAccumulator.Add(
                        new DiagnosticRecord(
                            diagnosticMessage,
                            variableExpression.Extent,
                            _rule.GetName(),
                            Severity,
                            _analyzedFilePath,
                            ruleId: _rule.GetName(),
                            GetSuggestedCorrections(ast: variableExpression)));
                }
            }

            /// <summary>
            /// IsInvokeCommandSessionScriptBlock: Returns true if:
            /// - command is 'Invoke-Command' (or alias)
            /// - parameter '-Session' is present
            /// </summary>
            /// <param name="cmdName"></param>
            /// <param name="commandAst"></param>
            private bool IsInvokeCommandSessionScriptBlock(string cmdName, CommandAst commandAst)
            {
                return s_invokeCommandCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) &&
                    commandAst.CommandElements.Any(
                        e => e is CommandParameterAst parameterAst &&
                                parameterAst.ParameterName.Equals("session", StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// IsInvokeCommandComputerScriptBlock: Returns true if:
            /// - command is 'Invoke-Command' (or alias)
            /// - parameter '-Computer' is present
            /// </summary>
            /// <param name="cmdName"></param>
            /// <param name="commandAst"></param>
            private bool IsInvokeCommandComputerScriptBlock(string cmdName, CommandAst commandAst)
            {
                // 'com' is the shortest unambiguous form for the '-Computer' parameter
                return s_invokeCommandCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) &&
                    commandAst.CommandElements.Any(
                        e => e is CommandParameterAst parameterAst &&
                                parameterAst.ParameterName.StartsWith("com", StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// IsForeachScriptBlock: Returns true if:
            /// - command is 'Foreach-Object' (or alias)
            /// - parameter '-Parallel' is present
            /// </summary>
            /// <param name="cmdName"></param>
            /// <param name="scriptBlockParameterAst"></param>
            private bool IsForeachScriptBlock(string cmdName, CommandParameterAst scriptBlockParameterAst)
            {
                // 'pa' is the shortest unambiguous form for the '-Parallel' parameter
                return s_foreachObjectCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) &&
                    (scriptBlockParameterAst != null && 
                        scriptBlockParameterAst.ParameterName.StartsWith("pa", StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// IsJobScriptBlock: Returns true if:
            /// - command is 'Start-Job' or 'Start-ThreadJob' (or alias)
            /// - parameter name for this ScriptBlock not '-InitializationScript'
            /// </summary>
            /// <param name="cmdName"></param>
            /// <param name="scriptBlockParameterAst"></param>
            private bool IsJobScriptBlock(string cmdName, CommandParameterAst scriptBlockParameterAst)
            {
                // 'ini' is the shortest unambiguous form for the '-InitializationScript' parameter
                return (s_jobCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase) ||
                        s_threadJobCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase)) &&
                    !(scriptBlockParameterAst != null && 
                        scriptBlockParameterAst.ParameterName.StartsWith("ini", StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// IsInlineScriptBlock: Returns true if:
            /// - command is 'InlineScript' (or alias)
            /// </summary>
            /// <param name="cmdName"></param>
            private bool IsInlineScriptBlock(string cmdName)
            {
                return s_inlineScriptCmdletNamesAndAliases.Contains(cmdName, StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// IsDSCScriptResource: Returns true if:
            /// - command is 'GetScript', 'TestScript' or 'SetScript'
            /// </summary>
            /// <param name="commandAst"></param>
            private bool IsDSCScriptResource(string cmdName, CommandAst commandAst)
            {
                // Inside DSC Script resource, GetScript is of the form 'Script foo { GetScript = {} }'
                // If we reach this point in the code, we are sure there are at least two CommandElements, so the index of [1] will not fail.
                return s_dscScriptResourceCommandNames.Contains(cmdName, StringComparer.OrdinalIgnoreCase) && 
                    commandAst.CommandElements[1].ToString() == "=";
            }
        }
    }
}
