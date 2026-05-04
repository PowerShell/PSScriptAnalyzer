// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingArrayList: Checks for use of the ArrayList class
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingArrayList : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidUsingArrayList type.
        /// </summary>
        public AvoidUsingArrayList() {
            Enable = true;
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) { throw new ArgumentNullException(Strings.NullAstErrorMessage); }

            // If there is an using statement for the Collections namespace, check for the full typename.
            // Otherwise also check for the bare ArrayList name.
            Regex arrayListName = null;
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
                    arrayListName = new Regex(@"^((System\.)?Collections\.)?ArrayList$", RegexOptions.IgnoreCase);
                    break;
                }
            }
            if (arrayListName == null) { arrayListName = new Regex(@"^(System\.)?Collections\.ArrayList$", RegexOptions.IgnoreCase); }

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
                    arrayListName.IsMatch(typeAst.TypeName.Name) &&
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
                    typeAst.Parent.Extent,
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
                    bindingResult.BoundParameters["TypeName"].ConstantValue != null &&
                    arrayListName.IsMatch(bindingResult.BoundParameters["TypeName"].ConstantValue as string)
                )
                {
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.AvoidUsingArrayListError,
                            cmd.Extent.Text),
                        cmd.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName
                    );
                }
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingArrayListCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingArrayListDescription);
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
                Strings.AvoidUsingArrayListName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
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

