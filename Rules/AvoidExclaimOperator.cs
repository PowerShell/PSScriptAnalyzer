// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidExclaimOperator: Checks for use of the exclaim operator
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidExclaimOperator : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidExclaimOperator type.
        /// </summary>
        public AvoidExclaimOperator() {
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

            var diagnosticRecords = new List<DiagnosticRecord>();

            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is UnaryExpressionAst, true);
            if (foundAsts != null) {
                var correctionDescription = Strings.AvoidExclaimOperatorCorrectionDescription;
                foreach (UnaryExpressionAst unaryExpressionAst in foundAsts) {
                    if (unaryExpressionAst.TokenKind == TokenKind.Exclaim) {
                        var replaceWith = "-not";
                        // The UnaryExpressionAST should have a single child, the argument that the unary operator is acting upon.
                        // If the child's extent starts 1 after the parent's extent then there's no whitespace between the exclaim
                        // token and any variable/expression; in that case the replacement -not should include a space
                        if (unaryExpressionAst.Child != null && unaryExpressionAst.Child.Extent.StartColumnNumber == unaryExpressionAst.Extent.StartColumnNumber + 1) {
                            replaceWith = "-not ";
                        }
                        var corrections = new List<CorrectionExtent> {
                            new CorrectionExtent(
                                unaryExpressionAst.Extent.StartLineNumber,
                                unaryExpressionAst.Extent.EndLineNumber,
                                unaryExpressionAst.Extent.StartColumnNumber,
                                unaryExpressionAst.Extent.StartColumnNumber + 1,
                                replaceWith,
                                fileName,
                                correctionDescription
                            )
                        };
                        diagnosticRecords.Add(new DiagnosticRecord(
                                string.Format(
                                    CultureInfo.CurrentCulture, 
                                    Strings.AvoidExclaimOperatorError
                                ), 
                                unaryExpressionAst.Extent, 
                                GetName(),
                                GetDiagnosticSeverity(), 
                                fileName,
                                suggestedCorrections: corrections
                        ));
                    }
                }
            }
            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidExclaimOperatorCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidExclaimOperatorDescription);
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
                Strings.AvoidExclaimOperatorName);
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

