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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingDoubleQuotesForConstantStrings: Checks if a string that uses double quotes contains a constant string, which could be simplified to a single quote.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingDoubleQuotesForConstantString : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of type <seealso cref="AvoidUsingDoubleQuotesForConstantStrings"/>.
        /// </summary>
        public AvoidUsingDoubleQuotesForConstantString()
        {
            Enable = false;  // Disabled by default
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var stringConstantExpressionAsts = ast.FindAll(testAst => testAst is StringConstantExpressionAst, searchNestedScriptBlocks: true);
            foreach (StringConstantExpressionAst stringConstantExpressionAst in stringConstantExpressionAsts)
            {
                switch (stringConstantExpressionAst.StringConstantType)
                {
                    case StringConstantType.DoubleQuoted:
                        yield return GetDiagnosticRecord(stringConstantExpressionAst,
                            $"'{stringConstantExpressionAst.Value}'");
                        break;

                    case StringConstantType.DoubleQuotedHereString:
                        yield return GetDiagnosticRecord(stringConstantExpressionAst,
                            $"@'{Environment.NewLine}{stringConstantExpressionAst.Value}{Environment.NewLine}'@");
                        break;

                    default:
                        break;
                }
            }
        }

        private DiagnosticRecord GetDiagnosticRecord(StringConstantExpressionAst stringConstantExpressionAst,
            string suggestedCorrection)
        {
            return new DiagnosticRecord(
                Strings.AvoidOverwritingBuiltInCmdletsError,
                stringConstantExpressionAst.Extent,
                GetName(),
                GetDiagnosticSeverity(),
                stringConstantExpressionAst.Extent.File,
                suggestedCorrections: new[] {
                    new CorrectionExtent(
                        stringConstantExpressionAst.Extent,
                        suggestedCorrection,
                        stringConstantExpressionAst.Extent.File
                    )
                }
            );
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDoubleQuotesForConstantStringCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingDoubleQuotesForConstantStringDescription);
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
                Strings.AvoidUsingDoubleQuotesForConstantStringName);
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