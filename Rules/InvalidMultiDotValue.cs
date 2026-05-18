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
    /// Rule that reports an error when an unquoted value contains multiple dots,
    /// which is likely an attempt to construct a version number (e.g., 1.2.3)
    /// that is not properly quoted and thus misinterpreted as a double with member access.
    /// </summary>
    public class InvalidMultiDotValue : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of InvalidMultiDotValue type.
        /// </summary>
        public InvalidMultiDotValue() {
            Enable = false;
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all MemberExpressionAst nodes representing invalid unquoted multi-dot values
            IEnumerable<MemberExpressionAst> invalidAsts = ast.FindAll(testAst =>
                // An expression with 3 or more dots is seen as a double with an additional property
                testAst is MemberExpressionAst memberAst &&
                // The first two values are seen as a double
                memberAst.Expression.StaticType == typeof(double) &&
                // the rest is seen as a member of type int or double
                memberAst.Member is ConstantExpressionAst constantAst &&
                (
                    constantAst.StaticType == typeof(int) || // e.g.: [Version]1.2.3
                    constantAst.StaticType == typeof(double) // e.g.: [Version]1.2.3.4
                ),
                true
            ).Cast<MemberExpressionAst>();

            var correctionDescription = Strings.InvalidMultiDotValueCorrectionDescription;
            foreach (MemberExpressionAst invalidAst in invalidAsts)
            {
                var corrections = new List<CorrectionExtent> {
                    new CorrectionExtent(
                        invalidAst.Extent.StartLineNumber,
                        invalidAst.Extent.EndLineNumber,
                        invalidAst.Extent.StartColumnNumber,
                        invalidAst.Extent.EndColumnNumber,
                        "'" + invalidAst.Extent.Text + "'",
                        fileName,
                        correctionDescription
                    )
                };
                yield return new DiagnosticRecord(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.InvalidMultiDotValueError,
                        invalidAst.Extent.Text
                    ),
                    invalidAst.Extent,
                    GetName(),
                    DiagnosticSeverity.Error,
                    fileName,
                    invalidAst.Extent.Text,
                    suggestedCorrections: corrections
                );
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.InvalidMultiDotValueCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.InvalidMultiDotValueDescription);
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
                Strings.InvalidMultiDotValueName);
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

