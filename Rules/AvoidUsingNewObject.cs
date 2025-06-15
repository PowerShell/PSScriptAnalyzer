// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingNewObject: Check to make sure that New-Object is not used.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingNewObject : IScriptRule
    {
        private const string _cmdletName = "New-Object";
        private const string _comObjectParameterName = "ComObject";
        private const string _typeNameParameterName = "TypeName";

        private Ast _rootAst;

        /// <summary>
        /// AnalyzeScript: Analyzes the given Ast and returns DiagnosticRecords based on the analysis.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Required to query New-Object Parameter Splats / Variable reassignments
            _rootAst = ast;

            IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            foreach (CommandAst cmdAst in commandAsts)
            {
                if (cmdAst.GetCommandName() == null) continue;

                if (!string.Equals(cmdAst.GetCommandName(), _cmdletName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsComObject(cmdAst))
                {
                    continue;
                }

                yield return new DiagnosticRecord(
                        GetError(),
                        cmdAst.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        cmdAst.GetCommandName());
            }
        }

        /// <summary>
        /// Determines if the New-Object command is creating a COM object.
        /// </summary>
        /// <param name="cmdAst">The CommandAst representing the New-Object command</param>
        /// <returns>True if the command is creating a COM object, false otherwise</returns>
        private bool IsComObject(CommandAst cmdAst)
        {
            foreach (var element in cmdAst.CommandElements)
            {
                if (element is CommandParameterAst cmdParameterAst)
                {
                    if (string.Equals(cmdParameterAst.ParameterName, _comObjectParameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else if (string.Equals(cmdParameterAst.ParameterName, _typeNameParameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    continue;
                }
                else if (element is VariableExpressionAst splattedVarAst && splattedVarAst.Splatted)
                {
                    return ProcessParameterSplat(splattedVarAst);
                }
                else if (element is UsingExpressionAst usingExprAst && usingExprAst.SubExpression is VariableExpressionAst usingVarAst)
                {
                    if (!usingVarAst.Splatted)
                    {
                        continue;
                    }

                    return ProcessParameterSplat(usingVarAst);
                }
            }

            return false;
        }

        /// <summary>
        /// Processes a Parameter Splat to determine if it contains COM object parameter.
        /// </summary>
        /// <param name="splattedVarAst">The VariableExpressionAst representing the splatted variable</param>
        /// <returns>True if the variable contains COM object parameter, false otherwise</returns>
        private bool ProcessParameterSplat(VariableExpressionAst splattedVarAst)
        {
            var variableName = Helper.Instance.VariableNameWithoutScope(splattedVarAst.VariablePath);
            var visitedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return ProcessVariable(variableName, visitedVariables);
        }

        /// <summary>
        /// Recursively processes variable assignments to trace back to the original hashtable definition.
        /// </summary>
        /// <param name="variableName">The name of the variable to process</param>
        /// <param name="visitedVariables">Set of already visited variables to prevent infinite recursion</param>
        /// <returns>True if the variable chain leads to a COM object parameter, false otherwise</returns>
        private bool ProcessVariable(string variableName, HashSet<string> visitedVariables)
        {
            // Prevent infinite recursion
            if (visitedVariables.Contains(variableName))
            {
                return false;
            }

            visitedVariables.Add(variableName);

            var assignments = FindAssignmentsInScope(variableName);

            foreach (var assignment in assignments)
            {
                var hashtableAst = GetHashtableFromAssignment(assignment);
                if (hashtableAst != null)
                {
                    return CheckHashtableForComObjectKey(hashtableAst);
                }

                var sourceVariable = GetVariableFromAssignment(assignment);
                if (sourceVariable != null)
                {
                    var result = ProcessVariable(sourceVariable, visitedVariables);

                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extracts the source variable name from a variable-to-variable assignment.
        /// </summary>
        /// <param name="assignmentAst">The AssignmentStatementAst to analyze</param>
        /// <returns>The name of the source variable, or null if not a variable assignment</returns>
        private static string GetVariableFromAssignment(AssignmentStatementAst assignmentAst)
        {
            var rightHandSide = assignmentAst.Right;

            if (rightHandSide is PipelineAst pipelineAst &&
                pipelineAst.PipelineElements.Count == 1 &&
                pipelineAst.PipelineElements[0] is CommandExpressionAst cmdExpr &&
                cmdExpr.Expression is VariableExpressionAst varExpr)
            {
                return Helper.Instance.VariableNameWithoutScope(varExpr.VariablePath);
            }

            if (rightHandSide is CommandExpressionAst commandExpr &&
                commandExpr.Expression is VariableExpressionAst directVarExpr)
            {
                return Helper.Instance.VariableNameWithoutScope(directVarExpr.VariablePath);
            }

            return null;
        }

        /// <summary>
        /// Finds all assignment statements in the current scope that assign to the specified variable.
        /// </summary>
        /// <param name="variableName">The name of the variable to search for</param>
        /// <returns>A list of AssignmentStatementAst objects that assign to the variable</returns>
        private List<AssignmentStatementAst> FindAssignmentsInScope(string variableName)
        {
            var assignments = new List<AssignmentStatementAst>();

            var allAssignments = _rootAst.FindAll(ast => ast is AssignmentStatementAst, true);

            foreach (var assignment in allAssignments.Cast<AssignmentStatementAst>())
            {
                VariableExpressionAst leftVarAst = null;

                // Handle direct variable assignment: $var = ...
                if (assignment.Left is VariableExpressionAst leftVar)
                {
                    leftVarAst = leftVar;
                }
                // Handle typed assignment: [type]$var = ...
                else if (assignment.Left is ConvertExpressionAst convertAst &&
                         convertAst.Child is VariableExpressionAst typedVar)
                {
                    leftVarAst = typedVar;
                }

                if (leftVarAst == null)
                {
                    continue;
                }

                var leftVarName = Helper.Instance.VariableNameWithoutScope(leftVarAst.VariablePath);

                if (string.Equals(leftVarName, variableName, StringComparison.OrdinalIgnoreCase))
                {
                    assignments.Add(assignment);
                }
            }

            return assignments;
        }

        /// <summary>
        /// Checks if a hashtable contains a ComObject key or TypeName key to determine if it's for COM object creation.
        /// </summary>
        /// <param name="hashtableAst">The HashtableAst to examine</param>
        /// <returns>True if the hashtable contains a ComObject key, false if it contains TypeName or neither</returns>
        private static bool CheckHashtableForComObjectKey(HashtableAst hashtableAst)
        {
            foreach (var keyValuePair in hashtableAst.KeyValuePairs)
            {
                if (keyValuePair.Item1 is StringConstantExpressionAst keyAst)
                {
                    // If the key is ComObject, it's a COM object
                    if (string.Equals(keyAst.Value, _comObjectParameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    // If the key is TypeName, it's not a COM object
                    else if (string.Equals(keyAst.Value, _typeNameParameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Extracts a hashtable from an assignment statement's right-hand side.
        /// </summary>
        /// <param name="assignmentAst">The AssignmentStatementAst to analyze</param>
        /// <returns>The HashtableAst if found, null otherwise</returns>
        private static HashtableAst GetHashtableFromAssignment(AssignmentStatementAst assignmentAst)
        {
            var rightHandSide = assignmentAst.Right;

            HashtableAst hashtable = ExtractHashtableFromStatement(rightHandSide);

            if (hashtable != null)
            {
                return hashtable;
            }

            var hashtableNodes = rightHandSide.FindAll(ast => ast is HashtableAst, true);
            return hashtableNodes.FirstOrDefault() as HashtableAst;
        }

        /// <summary>
        /// Extracts a hashtable from various statement types (PipelineAst, CommandExpressionAst).
        /// </summary>
        /// <param name="statement">The StatementAst to analyze</param>
        /// <returns>The HashtableAst if found, null otherwise</returns>
        private static HashtableAst ExtractHashtableFromStatement(StatementAst statement)
        {
            switch (statement)
            {
                case PipelineAst pipelineAst when pipelineAst.PipelineElements.Count >= 1:
                    if (pipelineAst.PipelineElements[0] is CommandExpressionAst cmdExpr &&
                        cmdExpr.Expression is HashtableAst hashtableFromPipeline)
                    {
                        return hashtableFromPipeline;
                    }
                    break;

                case CommandExpressionAst commandExpr when commandExpr.Expression is HashtableAst hashtableFromCommand:
                    return hashtableFromCommand;
            }

            return null;
        }

        /// <summary>
        /// GetError: Retrieves the error message
        /// </summary>
        /// <returns>The error message</returns>
        public string GetError()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectError);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingNewObjectName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity:Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns>The severity of the rule</returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// DiagnosticSeverity: Retrieves the severity of the rule of type DiagnosticSeverity: error, warning or information.
        /// </summary>
        /// <returns>The diagnostic severity of the rule</returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}