// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;

#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    /// <summary>
    /// Rule that informs the user when they create variables with dynamic names in the general variable scope.
    /// This might lead to conflicts with other variables.
    /// </summary>
    public class AvoidDynamicallyCreatingVariableNames : IScriptRule
    {
        /// <summary>
        /// Analyzes the PowerShell AST for uses of "New-Variable" command with a dynamic name argument.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>

        readonly HashSet<string> cmdList = new HashSet<string>(Helper.Instance.CmdletNameAndAliases("New-Variable"), StringComparer.OrdinalIgnoreCase);
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all "New-Variable" commands in the Ast
            IEnumerable<CommandAst> newVariableAsts = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdList.Contains(cmdAst.GetCommandName()),
                true
            ).Cast<CommandAst>();

            foreach (CommandAst newVariableAst in newVariableAsts)
            {
                // Use StaticParameterBinder to reliably get parameter values
                var bindingResult = StaticParameterBinder.BindCommand(newVariableAst, true);
                if (!bindingResult.BoundParameters.ContainsKey("Name")) { continue; }
                var nameBindingResult = bindingResult.BoundParameters["Name"];
                // Dynamic parameters return null for the ConstantValue property
                if (nameBindingResult.ConstantValue != null) { continue; }
                string variableName = nameBindingResult.Value.ToString();
                if (variableName.StartsWith("\"") && variableName.EndsWith("\""))
                {
                    variableName = variableName.Substring(1, variableName.Length - 2);
                }
                yield return new DiagnosticRecord(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.AvoidDynamicallyCreatingVariableNamesError,
                        variableName),
                    newVariableAst.Parent.Extent,
                    GetName(),
                    DiagnosticSeverity.Information,
                    fileName,
                    variableName
                );
            }
        }

        public string GetCommonName() => Strings.AvoidDynamicallyCreatingVariableNamesCommonName;

        public string GetDescription() => Strings.AvoidDynamicallyCreatingVariableNamesDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidDynamicallyCreatingVariableNamesName);

        public RuleSeverity GetSeverity() => RuleSeverity.Information;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}