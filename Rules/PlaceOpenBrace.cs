// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// A formatting rule about whether braces should start on the same line or not.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class PlaceOpenBrace : ConfigurableRule
    {
        /// <summary>
        /// Indicates if an open brace should be on the same line or on the next line of its preceding keyword.
        ///
        /// Default value is true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool OnSameLine { get; protected set; }

        /// <summary>
        /// Indicates if a new line should or should not follow an open brace.
        ///
        /// Default value is true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool NewLineAfter { get; protected set; }

        /// <summary>
        /// Indicates if open braces in a one line block should be ignored or not.
        /// E.g. $x = if ($true) { "blah" } else { "blah blah" }
        /// In the above example, if the property is set to true then the rule will
        /// not fire a violation.
        ///
        /// Default value if true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool IgnoreOneLineBlock { get; protected set; }

        private List<Func<Token[], Ast, string, IEnumerable<DiagnosticRecord>>> violationFinders
            = new List<Func<Token[], Ast, string, IEnumerable<DiagnosticRecord>>>();

        private HashSet<Token> tokensToIgnore;

        /// <summary>
        /// Sets the configurable properties of this rule.
        /// </summary>
        /// <param name="paramValueMap">A dictionary that maps parameter name to it value. Must be non-null</param>
        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);
            if (OnSameLine)
            {
                violationFinders.Add(FindViolationsForBraceShouldBeOnSameLine);
            }
            else
            {
                violationFinders.Add(FindViolationsForBraceShouldNotBeOnSameLine);
            }

            if (NewLineAfter)
            {
                violationFinders.Add(FindViolationsForNoNewLineAfterBrace);
            }
        }

        /// <summary>
        /// Analyzes the given ast to find violations.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            // TODO Should have the following option
            // * no-empty-lines-after
            var diagnosticRecords = new List<DiagnosticRecord>();
            if (!Enable)
            {
                return diagnosticRecords;
            }

            var tokens = Helper.Instance.Tokens;

            // Ignore open braces that are part of arguments to a command
            // * E.g. get-process | % { "blah }
            // In the above case even if OnSameLine == false, we should not
            // flag the open brace as it would move the brace to the next line
            // and will invalidate the command
            var tokenOps = new TokenOperations(tokens, ast);
            tokensToIgnore = new HashSet<Token>(tokenOps.GetOpenBracesInCommandElements());

            // Ignore open braces that are part of a one line if-else statement
            // E.g. $x = if ($true) { "blah" } else { "blah blah" }
            if (IgnoreOneLineBlock)
            {
                foreach (var pair in tokenOps.GetBracePairsOnSameLine())
                {
                    tokensToIgnore.Add(pair.Item1);
                }
            }

            foreach (var violationFinder in violationFinders)
            {
                diagnosticRecords.AddRange(violationFinder(tokens, ast, fileName));
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceOpenBraceCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceOpenBraceDescription);
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
                Strings.PlaceOpenBraceName);
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

        private static string GetError(string errorString)
        {
            return string.Format(CultureInfo.CurrentCulture, errorString);
        }

        private IEnumerable<DiagnosticRecord> FindViolationsForBraceShouldBeOnSameLine(
            Token[] tokens,
            Ast ast,
            string fileName)
        {
            for (int k = 2; k < tokens.Length; k++)
            {
                if (tokens[k].Kind == TokenKind.LCurly
                    && tokens[k - 1].Kind == TokenKind.NewLine
                    && !tokensToIgnore.Contains(tokens[k]))
                {
                    var precedingExpression = tokens[k - 2];
                    Token optionalComment = null;
                    // If a comment is before the open brace, then take the token before the comment
                    if (precedingExpression.Kind == TokenKind.Comment && k > 2)
                    {
                        precedingExpression = tokens[k - 3];
                        optionalComment = tokens[k - 2];
                    }

                    yield return new DiagnosticRecord(
                        GetError(Strings.PlaceOpenBraceErrorShouldBeOnSameLine),
                        tokens[k].Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForBraceShouldBeOnSameLine(precedingExpression, optionalComment, tokens[k], fileName));
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindViolationsForNoNewLineAfterBrace(
            Token[] tokens,
            Ast ast,
            string fileName)
        {
            for (int k = 0; k < tokens.Length - 1; k++)
            {
                // typically the last element is of kind endofinput,
                // but this checks adds additional safeguard
                if (tokens[k].Kind == TokenKind.EndOfInput)
                {
                    break;
                }

                if (tokens[k].Kind == TokenKind.LCurly
                    && tokens[k + 1].Kind != TokenKind.NewLine
                    && !tokensToIgnore.Contains(tokens[k]))
                {
                    yield return new DiagnosticRecord(
                        GetError(Strings.PlaceOpenBraceErrorNoNewLineAfterBrace),
                        tokens[k].Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForNoNewLineAfterBrace(tokens, k, fileName));
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindViolationsForBraceShouldNotBeOnSameLine(
            Token[] tokens,
            Ast ast,
            string fileName)
        {
            for (int k = 1; k < tokens.Length; k++)
            {
                if (tokens[k].Kind == TokenKind.EndOfInput)
                {
                    break;
                }

                if (tokens[k].Kind == TokenKind.LCurly
                    && tokens[k - 1].Kind != TokenKind.NewLine
                    && !tokensToIgnore.Contains(tokens[k]))
                {
                    yield return new DiagnosticRecord(
                        GetError(Strings.PlaceOpenBraceErrorShouldNotBeOnSameLine),
                        tokens[k].Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForBraceShouldNotBeOnSameLine(tokens, k - 1, k, fileName));
                }
            }
        }

        private List<CorrectionExtent> GetCorrectionsForNoNewLineAfterBrace(
            Token[] tokens,
            int openBracePos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var extent = tokens[openBracePos].Extent;

            corrections.Add(
                new CorrectionExtent(
                    extent.StartLineNumber,
                    extent.EndLineNumber,
                    extent.StartColumnNumber,
                    extent.EndColumnNumber,
                    new StringBuilder().Append(extent.Text).Append(Environment.NewLine).ToString(),
                    fileName));
            return corrections;
        }

        private List<CorrectionExtent> GetCorrectionsForBraceShouldNotBeOnSameLine(
            Token[] tokens,
            int prevTokenPos,
            int closeBraceTokenPos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var prevToken = tokens[prevTokenPos];
            var closeBraceToken = tokens[closeBraceTokenPos];
            corrections.Add(
                new CorrectionExtent(
                    prevToken.Extent.StartLineNumber,
                    closeBraceToken.Extent.EndLineNumber,
                    prevToken.Extent.StartColumnNumber,
                    closeBraceToken.Extent.EndColumnNumber,
                    (new StringBuilder())
                        .Append(prevToken.Text)
                        .AppendLine()
                        .Append(GetIndentation(tokens, closeBraceTokenPos))
                        .Append(closeBraceToken.Text)
                        .ToString(),
                    fileName));
            return corrections;
        }

        private string GetIndentation(Token[] tokens, int refTokenPos)
        {
            return new String(' ', GetStartColumnNumberOfTokenLine(tokens, refTokenPos) - 1);
        }

        private int GetStartColumnNumberOfTokenLine(Token[] tokens, int refTokenPos)
        {
            var refToken = tokens[refTokenPos];
            for (int k = refTokenPos - 1; k >= 0; k--)
            {
                if (tokens[k].Extent.StartLineNumber != refToken.Extent.StartLineNumber)
                {
                    return tokens[k].Extent.StartColumnNumber;
                }
            }

            return refToken.Extent.StartColumnNumber;
        }

        private List<CorrectionExtent> GetCorrectionsForBraceShouldBeOnSameLine(
            Token precedingExpression,
            Token optionalCommentOfPrecedingExpression,
            Token lCurly,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var optionalComment = optionalCommentOfPrecedingExpression != null ? $" {optionalCommentOfPrecedingExpression}" : string.Empty;
            corrections.Add(
                new CorrectionExtent(
                    precedingExpression.Extent.StartLineNumber,
                    lCurly.Extent.EndLineNumber,
                    precedingExpression.Extent.StartColumnNumber,
                    lCurly.Extent.EndColumnNumber,
                    $"{precedingExpression.Text} {lCurly.Text}{optionalComment}",
                    fileName));
            return corrections;
        }
    }
}
