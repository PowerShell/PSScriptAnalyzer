// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidSemicolonsAsLineTerminators: Checks for lines that end with a semicolon.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidSemicolonsAsLineTerminators : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of AvoidSemicolonsAsLineTerminators type.
        /// </summary>
        public AvoidSemicolonsAsLineTerminators()
        {
            Enable = false;
        }

        /// <summary>
        /// Checks for lines that end with a semicolon.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>The diagnostic results of this rule</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }


            var diagnosticRecords = new List<DiagnosticRecord>();

            IEnumerable<Ast> assignmentStatements = ast.FindAll(item => item is AssignmentStatementAst, true);

            var tokens = Helper.Instance.Tokens;
            for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {

                var token = tokens[tokenIndex];
                var semicolonTokenExtent = token.Extent;

                var isSemicolonToken = token.Kind is TokenKind.Semi;
                if (!isSemicolonToken)
                {
                    continue;
                }

                var isPartOfAnyAssignmentStatement = assignmentStatements.Any(assignmentStatement => (assignmentStatement.Extent.EndOffset == semicolonTokenExtent.StartOffset + 1));
                if (isPartOfAnyAssignmentStatement)
                {
                    continue;
                }

                var nextTokenIndex = tokenIndex + 1;
                var isNextTokenIsNewLine = tokens[nextTokenIndex].Kind is TokenKind.NewLine;
                var isNextTokenIsEndOfInput = tokens[nextTokenIndex].Kind is TokenKind.EndOfInput;

                if (!isNextTokenIsNewLine && !isNextTokenIsEndOfInput)
                {
                    continue;
                }

                var violationExtent = new ScriptExtent(
                new ScriptPosition(
                    ast.Extent.File,
                    semicolonTokenExtent.StartLineNumber,
                    semicolonTokenExtent.StartColumnNumber,
                    semicolonTokenExtent.StartScriptPosition.Line
                ),
                new ScriptPosition(
                    ast.Extent.File,
                    semicolonTokenExtent.EndLineNumber,
                    semicolonTokenExtent.EndColumnNumber,
                    semicolonTokenExtent.EndScriptPosition.Line
                ));

                var suggestedCorrections = new List<CorrectionExtent>();
                suggestedCorrections.Add(new CorrectionExtent(
                            violationExtent,
                            string.Empty,
                            ast.Extent.File
                        ));

                var record = new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidSemicolonsAsLineTerminatorsError),
                        violationExtent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        ast.Extent.File,
                        null,
                        suggestedCorrections
                    );
                diagnosticRecords.Add(record);
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidSemicolonsAsLineTerminatorsCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidSemicolonsAsLineTerminatorsDescription);
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
                Strings.AvoidSemicolonsAsLineTerminatorsName);
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
        /// Retrieves the type of the rule: builtin, managed, or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
