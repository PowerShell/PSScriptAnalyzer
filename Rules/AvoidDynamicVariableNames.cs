// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Rule that warns when reserved words are used as function names
    /// </summary>
    public class AvoidDynamicVariableNames : IScriptRule
    {
        /// <summary>
        /// Analyzes the PowerShell AST for uses of reserved words as function names.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all FunctionDefinitionAst in the Ast
            var newVariableAsts = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                (
                    String.Equals(cmdAst.GetCommandName(), "New-Variable", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(cmdAst.GetCommandName(), "Set-Variable", StringComparison.OrdinalIgnoreCase)
                ),
                true
            );

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
                        Strings.AvoidDynamicVariableNamesError,
                        variableName),
                    newVariableAst.Parent.Extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName,
                    variableName
                );
            }
        }

        public string GetCommonName() => Strings.AvoidDynamicVariableNamesCommonName;

        public string GetDescription() => Strings.AvoidDynamicVariableNamesDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidDynamicVariableNamesName);

        public RuleSeverity GetSeverity() => RuleSeverity.Warning;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}