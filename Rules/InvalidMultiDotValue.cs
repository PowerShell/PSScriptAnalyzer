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
    /// Rule that warns when an unquoted value contains multiple dots,
    /// which is likely an attempt to construct a version number (e.g., 1.2.3)
    /// that is not properly quoted and thus misinterpreted as a double with member access.
    /// </summary>
    public class InvalidMultiDotValue : IScriptRule
    {
        /// <summary>
        /// Analyzes the PowerShell unquoted values that contain multiple dots.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all FunctionDefinitionAst in the Ast
            IEnumerable<Ast> invalidAsts = ast.FindAll(testAst =>
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
            );

            if (invalidAsts != null) {
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
        }

        public string GetCommonName() => Strings.InvalidMultiDotValueCommonName;

        public string GetDescription() => Strings.InvalidMultiDotValueDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.InvalidMultiDotValueName);

        public RuleSeverity GetSeverity() => RuleSeverity.Error;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}