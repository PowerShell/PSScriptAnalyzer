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
    /// <summary>
    /// Rule that informs the user when they create variables with dynamic names in the general variable scope.
    /// This might lead to conflicts with other variables.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidDynamicallyCreatingVariableNames : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidDynamicallyCreatingVariableNames type.
        /// </summary>
        public AvoidDynamicallyCreatingVariableNames() {
            Enable = false;
        }

        readonly HashSet<string> cmdList = new HashSet<string>(Helper.Instance.CmdletNameAndAliases("New-Variable"), StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
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
                    newVariableAst.Extent,
                    GetName(),
                    DiagnosticSeverity.Information,
                    fileName,
                    variableName
                );
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDynamicallyCreatingVariableNamesCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDynamicallyCreatingVariableNamesDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidDynamicallyCreatingVariableNamesName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Information;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
