// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using System.ComponentModel;


#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    /// <summary>
    /// Rule that warns when the ArrayList class is used in a PowerShell script.
    /// </summary>
    public class AvoidUsingArrayListAsFunctionNames : IScriptRule
    {

        /// <summary>
        /// Analyzes the PowerShell AST for uses of the ArrayList class.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) { throw new ArgumentNullException(Strings.NullAstErrorMessage); }

            // If there is an using statement for the Collections namespace, check for the full typename.
            // Otherwise also check for the bare ArrayList name.
            Regex ArrayListName = null;
            var sbAst = ast as ScriptBlockAst;
            foreach (UsingStatementAst usingAst in sbAst.UsingStatements)
            {
                if (
                    usingAst.UsingStatementKind == UsingStatementKind.Namespace &&
                    (
                        usingAst.Name.Value.Equals("Collections", StringComparison.OrdinalIgnoreCase) ||
                        usingAst.Name.Value.Equals("System.Collections", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    ArrayListName = new Regex(@"^((System\.)?Collections\.)?ArrayList$", RegexOptions.IgnoreCase);
                    break;
                }
            }
            if (ArrayListName == null) { ArrayListName = new Regex(@"^(System\.)?Collections\.ArrayList$", RegexOptions.IgnoreCase); }

            // Find all type initializers that create a new instance of the ArrayList class.
            IEnumerable<Ast> typeAsts = ast.FindAll(testAst =>
                (
                    testAst is ConvertExpressionAst convertAst &&
                    convertAst.StaticType != null &&
                    convertAst.StaticType.FullName == "System.Collections.ArrayList"
                ) ||
                (
                    testAst is TypeExpressionAst typeAst &&
                    typeAst.TypeName != null &&
                    ArrayListName.IsMatch(typeAst.TypeName.Name) &&
                    typeAst.Parent is InvokeMemberExpressionAst parentAst &&
                    parentAst.Member != null &&
                    parentAst.Member is StringConstantExpressionAst memberAst &&
                    memberAst.Value.Equals("new", StringComparison.OrdinalIgnoreCase)
                ),
                true
            );

            foreach (Ast typeAst in typeAsts)
            {
                yield return new DiagnosticRecord(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.AvoidUsingArrayListError,
                        typeAst.Parent.Extent.Text),
                    typeAst.Extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName
                );
            }

            // Find all New-Object cmdlets that create a new instance of the ArrayList class.
            var newObjectCommands = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("New-Object", StringComparison.OrdinalIgnoreCase),
                true);

            foreach (CommandAst cmd in newObjectCommands)
            {
                // Use StaticParameterBinder to reliably get parameter values
                var bindingResult = StaticParameterBinder.BindCommand(cmd, true);

                // Check for -TypeName parameter
                if (
                    bindingResult.BoundParameters.ContainsKey("TypeName") &&
                    ArrayListName.IsMatch(bindingResult.BoundParameters["TypeName"].ConstantValue as string)
                )
                {
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.AvoidUsingArrayListError,
                            cmd.Extent.Text),
                        bindingResult.BoundParameters["TypeName"].Value.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName
                    );
                }

            }


        }

        public string GetCommonName() => Strings.AvoidUsingArrayListCommonName;

        public string GetDescription() => Strings.AvoidUsingArrayListDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidUsingArrayListName);

        public RuleSeverity GetSeverity() => RuleSeverity.Warning;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}