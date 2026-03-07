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
    /// UseConstrainedLanguageMode: Checks for patterns that indicate Constrained Language Mode should be considered.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConstrainedLanguageMode : ConfigurableRule
    {
        /// <summary>
        /// Analyzes the script to check for patterns that may require Constrained Language Mode.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var diagnosticRecords = new List<DiagnosticRecord>();

            // Check for Add-Type usage (not allowed in Constrained Language Mode)
            var addTypeCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("Add-Type", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in addTypeCommands)
            {
                diagnosticRecords.Add(
                    new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeAddTypeError),
                        cmd.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    ));
            }

            // Check for New-Object with COM objects (not allowed in Constrained Language Mode)
            var newObjectCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("New-Object", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in newObjectCommands)
            {
                // Check if -ComObject parameter is used
                var comObjectParam = cmd.CommandElements.OfType<CommandParameterAst>()
                    .FirstOrDefault(p => p.ParameterName.Equals("ComObject", StringComparison.OrdinalIgnoreCase));

                if (comObjectParam != null)
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeComObjectError),
                            cmd.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            // Check for XAML usage (not allowed in Constrained Language Mode)
            var xamlPatterns = ast.FindAll(testAst =>
                testAst is StringConstantExpressionAst strAst &&
                strAst.Value.Contains("<") && strAst.Value.Contains("xmlns"),
                true);

            foreach (StringConstantExpressionAst xamlAst in xamlPatterns)
            {
                if (xamlAst.Value.Contains("http://schemas.microsoft.com/winfx"))
                {
                    diagnosticRecords.Add(
                        new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeXamlError),
                            xamlAst.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName
                        ));
                }
            }

            // Check for dot-sourcing - PowerShell doesn't have a specific DotSourceExpressionAst
            // We look for patterns where a script block or file is dot-sourced
            // This is best detected through token analysis, but for simplicity we'll check for common patterns
            var scriptBlocks = ast.FindAll(testAst => testAst is ScriptBlockExpressionAst, true);
            
            foreach (ScriptBlockExpressionAst sbAst in scriptBlocks)
            {
                // Check if preceded by a dot token (basic heuristic for dot-sourcing)
                // More sophisticated detection would require token analysis
                var parent = sbAst.Parent;
                if (parent is CommandAst cmdAst)
                {
                    // Check if this looks like a dot-source pattern
                    var cmdName = cmdAst.GetCommandName();
                    if (cmdName != null && cmdName.StartsWith("."))
                    {
                        diagnosticRecords.Add(
                            new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeDotSourceError),
                                sbAst.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                fileName
                            ));
                    }
                }
            }

            // Check for Invoke-Expression usage (restricted in Constrained Language Mode)
            var invokeExpressionCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("Invoke-Expression", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in invokeExpressionCommands)
            {
                diagnosticRecords.Add(
                    new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeInvokeExpressionError),
                        cmd.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName
                    ));
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConstrainedLanguageModeDescription);
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
                Strings.UseConstrainedLanguageModeName);
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
