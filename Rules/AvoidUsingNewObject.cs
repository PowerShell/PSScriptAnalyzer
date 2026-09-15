// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using System.Linq;

#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    /// <summary>
    /// Rule that reports a warning when the New-Object cmdlet is used in a script.
    /// The rule implements a correction that suggests using type-casting or type constructor.
    ///
    /// Note:
    /// In most cases if there isn't an automatic correction available,
    /// the rule won't report any violation either.
    /// This is because if there isn't an automatic correction available, it generally means
    /// that there isn't a simple type-casting or type constructor that can be used that would
    /// be more efficient or has a better syntax than using New-Object.
    /// In other words, if the common `-Verbose` parameter is used, or both the parameters
    /// `-ArgumentList` and `-Property` are used, there won't be a simple type initializer
    /// available and the rule won't report any violation for the `New-Object` cmdlet.
    ///
    /// Nevertheless, there are still some cases where the `New-Object` cmdlet might be
    /// replaceable with a type initializer that would be more efficient or has a better syntax,
    /// but an automatic correction can't be provided.
    /// For example if the `-ArgumentList` parameter is used with a variable,
    /// the rule will report a violation, but won't be able to provide a correction,
    /// as it's not possible to determine from the AST alone whether the variable contains a
    /// single value that can be used in a type initializer,
    /// or if it contains multiple values that would require splatting.
    /// </summary>
    public class AvoidUsingNewObject : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidUsingNewObject type.
        /// </summary>
        public AvoidUsingNewObject() {
            Enable = false;
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>An enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<CommandAst> newObjectAsts = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                (cmdAst.GetCommandName() as string) is string commandName &&
                commandName.Equals("New-Object", StringComparison.OrdinalIgnoreCase),
                true
            ).Cast<CommandAst>();

            foreach (CommandAst cmdAst in newObjectAsts)
            {
                // Use StaticParameterBinder to reliably get parameter values
                var bindingResult = StaticParameterBinder.BindCommand(cmdAst, true);

                // Check for `-TypeName` and either a `-ArgumentList` or `-Property`.
                // But not both, as that would mean there isn't a simple
                // type initializer available as a replacement.
                if (
                    bindingResult.BoundParameters.Count <= 2 &&
                    bindingResult.BoundParameters.TryGetValue("TypeName", out ParameterBindingResult asTypeName) &&
                    asTypeName.ConstantValue is string typeName
                ) {
                    Boolean isProperty = bindingResult.BoundParameters.TryGetValue("Property", out ParameterBindingResult boundResult);
                    if (!isProperty)
                    {
                        Boolean isArgument = bindingResult.BoundParameters.TryGetValue("ArgumentList", out ParameterBindingResult argumentResult);
                        if (isArgument) { boundResult = argumentResult; }
                    }

                    string correction = null;
                    if (boundResult == null)
                    {
                        // No `-Property` or `-ArgumentList` parameter was used, so we suggest a parameterless constructor call.
                        correction = "[" + typeName + "]::new()";
                    }
                    else if (isProperty)
                    {
                        correction = "[" + typeName + "]" + boundResult.Value.Extent.Text;
                    }
                    else if (boundResult.ConstantValue != null)
                    {
                        string valueText = boundResult.Value.Extent.Text;
                        if (
                            boundResult.Value is StringConstantExpressionAst stringConstant &&
                            stringConstant.StringConstantType == StringConstantType.BareWord
                        )
                        {
                            valueText = '"' + valueText.Replace("\"", string.Empty) + '"'; // Test""123 --> "Test123"
                        }
                        correction = "[" + typeName + "]" + valueText;
                    }
                    else if (
                        boundResult.Value is ParenExpressionAst parenExpressionAst &&
                        !parenExpressionAst.Pipeline.Extent.Text.StartsWith(",")
                    )
                    {
                        correction = "[" +typeName + "]::new" + parenExpressionAst.Extent.Text;
                    }
                    else if (
                        boundResult.Value is ArrayExpressionAst arrayExpressionAst &&
                        !arrayExpressionAst.SubExpression.Extent.Text.StartsWith(",")
                    )
                    {
                        correction = "[" + typeName + "]::new(" + arrayExpressionAst.SubExpression.Extent.Text + ")";
                    }
                    else if (
                        boundResult.Value is SubExpressionAst subExpressionAst &&
                        !subExpressionAst.SubExpression.Extent.Text.StartsWith(",")
                    )
                    {
                        correction = "[" + typeName + "]::new(" + subExpressionAst.SubExpression.Extent.Text + ")";
                    }
                    else if (boundResult.Value is VariableExpressionAst)
                    {
                        // correction == $null

                        // The correction is inconclusive, as we can't be sure if the variable contains
                        // a single value that can be used in a type initializer,
                        // or if it contains multiple values that would require splatting,
                        // which can't be automatically determined from the AST.
                    }
                    else
                    {
                        correction = "[" + typeName + "]::new(" + boundResult.Value.Extent.Text + ")";
                    }

                    DiagnosticRecord diagnosticRecord = new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.AvoidUsingNewObjectError,
                            typeName
                        ),
                        cmdAst.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        typeName
                    );

                    if (correction != null)
                    {
                        diagnosticRecord.SuggestedCorrections = new List<CorrectionExtent> {
                            new CorrectionExtent(
                                cmdAst.Extent.StartLineNumber,
                                cmdAst.Extent.EndLineNumber,
                                cmdAst.Extent.StartColumnNumber,
                                cmdAst.Extent.EndColumnNumber,
                                correction,
                                fileName,
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.AvoidUsingNewObjectCorrectionDescription,
                                    typeName
                                )
                            )
                        };
                    }

                    yield return diagnosticRecord;
                }
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectDescription);
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
                Strings.AvoidUsingNewObjectName);
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

