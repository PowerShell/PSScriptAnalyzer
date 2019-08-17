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
    /// PlaceCloseBrace: Indicates if there should or should not be an empty line before a close brace.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class PlaceCloseBrace : ConfigurableRule
    {
        private HashSet<Token> tokensToIgnore;
        private List<Func<Token[], int, int, string, DiagnosticRecord>> violationFinders
            = new List<Func<Token[], int, int, string, DiagnosticRecord>>();

        /// <summary>
        /// Indicates if there should or should not be an empty line before a close brace.
        ///
        /// Default value is false.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: false)]
        public bool NoEmptyLineBefore { get; protected set; }

        /// <summary>
        /// Indicates if close braces in a one line block should be ignored or not.
        /// E.g. $x = if ($true) { "blah" } else { "blah blah" }
        /// In the above example, if the property is set to true then the rule will
        /// not fire a violation.
        ///
        /// Default value is true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool IgnoreOneLineBlock { get; protected set; }

        /// <summary>
        /// Indicates if a new line should follow a close brace.
        ///
        /// If set to true a close brace should be followed by a new line.
        ///
        /// Default value is true.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: true)]
        public bool NewLineAfter { get; protected set; }

        /// <summary>
        /// Sets the configurable properties of this rule.
        /// </summary>
        /// <param name="paramValueMap">A dictionary that maps parameter name to it value. Must be non-null</param>
        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);
            violationFinders.Add(GetViolationForBraceShouldBeOnNewLine);
            if (NoEmptyLineBefore)
            {
                violationFinders.Add(GetViolationForBraceShouldNotFollowEmptyLine);
            }

            if (NewLineAfter)
            {
                violationFinders.Add(GetViolationForBraceShouldHaveNewLineAfter);
            }
            else
            {
                violationFinders.Add(GetViolationsForUncuddledBranches);
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

            if (!Enable)
            {
                return Enumerable.Empty<DiagnosticRecord>();
            }

            // TODO Should have the following options
            // * no-empty-lines-before

            var tokens = Helper.Instance.Tokens;
            var diagnosticRecords = new List<DiagnosticRecord>();
            var curlyStack = new Stack<Tuple<Token, int>>();

            // TODO move part common with PlaceOpenBrace to one place
            var tokenOps = new TokenOperations(tokens, ast);
            tokensToIgnore = new HashSet<Token>(tokenOps.GetCloseBracesInCommandElements());

            // Ignore close braces that are part of a one line if-else statement
            // E.g. $x = if ($true) { "blah" } else { "blah blah" }
            if (IgnoreOneLineBlock)
            {
                foreach (var pair in tokenOps.GetBracePairsOnSameLine())
                {
                    tokensToIgnore.Add(pair.Item2);
                }
            }

            for (int k = 0; k < tokens.Length; k++)
            {
                var token = tokens[k];
                if (token.Kind == TokenKind.LCurly || token.Kind == TokenKind.AtCurly)
                {
                    curlyStack.Push(new Tuple<Token, int>(token, k));
                    continue;
                }

                if (token.Kind == TokenKind.RCurly)
                {
                    if (curlyStack.Count > 0)
                    {
                        var openBraceToken = curlyStack.Peek().Item1;
                        var openBracePos = curlyStack.Pop().Item2;

                        // Ignore if a one line hashtable
                        if (openBraceToken.Kind == TokenKind.AtCurly
                            && openBraceToken.Extent.StartLineNumber == token.Extent.StartLineNumber)
                        {
                            continue;
                        }

                        foreach (var violationFinder in violationFinders)
                        {
                            AddToDiagnosticRecords(
                                violationFinder(tokens, k, openBracePos, fileName),
                                ref diagnosticRecords);
                        }
                    }
                    else
                    {
                        break;
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
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceCloseBraceCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceCloseBraceDescription);
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
                Strings.PlaceCloseBraceName);
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

        private DiagnosticRecord GetViolationForBraceShouldNotFollowEmptyLine(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            if (tokens.Length > 2 && tokens.Length > closeBracePos)
            {
                var closeBraceToken = tokens[closeBracePos];
                var newLineToken = tokens[closeBracePos - 1];
                var extraNewLineToken = tokens[closeBracePos - 2];
                if (newLineToken.Kind == TokenKind.NewLine
                    && extraNewLineToken.Kind == TokenKind.NewLine)
                {
                    return new DiagnosticRecord(
                        GetError(Strings.PlaceCloseBraceErrorShouldNotFollowEmptyLine),
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForBraceShouldNotFollowEmptyLine(
                            tokens,
                            closeBracePos,
                            openBracePos,
                            fileName));
                }
            }

            return null;
        }

        private List<CorrectionExtent> GetCorrectionsForBraceShouldNotFollowEmptyLine(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var newLineToken = tokens[closeBracePos - 2];
            var closeBraceToken = tokens[closeBracePos];
            corrections.Add(new CorrectionExtent(
                newLineToken.Extent.StartLineNumber,
                closeBraceToken.Extent.EndLineNumber,
                newLineToken.Extent.StartColumnNumber,
                closeBraceToken.Extent.EndColumnNumber,
                newLineToken.Text + GetIndentation(tokens, closeBracePos, openBracePos) + closeBraceToken.Text,
                fileName));
            return corrections;
        }

        private List<CorrectionExtent> GetCorrectionsForBraceShouldHaveNewLineAfter(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var nextToken = tokens[closeBracePos + 1];
            var closeBraceToken = tokens[closeBracePos];
            corrections.Add(new CorrectionExtent(
                closeBraceToken.Extent,
                closeBraceToken.Text + Environment.NewLine,
                fileName));
            return corrections;
        }

        private string GetIndentation(Token[] tokens, int closeBracePos, int openBracePos)
        {
            // if open brace on a new line by itself, use its indentation
            var openBraceToken = tokens[openBracePos];
            if (openBracePos > 0 && tokens[openBracePos - 1].Kind == TokenKind.NewLine)
            {
                return new String(' ', openBraceToken.Extent.StartColumnNumber - 1);
            }

            // if open brace follows any keywords use the identation of the first keyword
            // on the line containing the open brace
            Token firstTokenOnOpenBraceLine = openBraceToken;
            for (int k = openBracePos; k > 0; --k)
            {
                if (tokens[k].Extent.StartLineNumber == firstTokenOnOpenBraceLine.Extent.StartLineNumber)
                {
                    firstTokenOnOpenBraceLine = tokens[k];
                }
                else
                {
                    break;
                }
            }

            return new String(' ', firstTokenOnOpenBraceLine.Extent.StartColumnNumber - 1);
        }

        private DiagnosticRecord GetViolationForBraceShouldHaveNewLineAfter(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            var expectedNewLinePos = closeBracePos + 1;
            if (tokens.Length > 1 && tokens.Length > expectedNewLinePos)
            {
                var closeBraceToken = tokens[closeBracePos];
                if (!tokensToIgnore.Contains(closeBraceToken) && IsBranchingStatementToken(tokens[expectedNewLinePos]))
                {
                    return new DiagnosticRecord(
                        GetError(Strings.PlaceCloseBraceErrorShouldFollowNewLine),
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForBraceShouldHaveNewLineAfter(tokens, closeBracePos, openBracePos, fileName));
                }
            }

            return null;
        }

        private DiagnosticRecord GetViolationsForUncuddledBranches(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            // this will not work if there is a comment in between any tokens.
            var closeBraceToken = tokens[closeBracePos];
            if (tokens.Length <= closeBracePos + 2 ||
                tokensToIgnore.Contains(closeBraceToken))
            {
                return null;
            }

            var token1 = tokens[closeBracePos + 1];
            var token2 = tokens[closeBracePos + 2];
            var branchTokenPos = IsBranchingStatementToken(token1) && !ApartByWhitespace(closeBraceToken, token1) ?
                             closeBracePos + 1 :
                             token1.Kind == TokenKind.NewLine && IsBranchingStatementToken(token2) ?
                                closeBracePos + 2 :
                                -1;

            return branchTokenPos == -1 ?
                null :
                new DiagnosticRecord(
                 GetError(Strings.PlaceCloseBraceErrorShouldCuddleBranchStatement),
                 closeBraceToken.Extent,
                 GetName(),
                 GetDiagnosticSeverity(),
                 fileName,
                 null,
                 GetCorrectionsForUncuddledBranches(tokens, closeBracePos, branchTokenPos, fileName));
        }

        private static bool ApartByWhitespace(Token token1, Token token2)
        {
            var e1 = token1.Extent;
            var e2 = token2.Extent;
            return e1.StartLineNumber == e2.StartLineNumber &&
                e2.StartColumnNumber - e1.EndColumnNumber == 1;
        }

        private List<CorrectionExtent> GetCorrectionsForUncuddledBranches(
            Token[] tokens,
            int closeBracePos,
            int branchStatementPos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var closeBraceToken = tokens[closeBracePos];
            var branchStatementToken = tokens[branchStatementPos];
            corrections.Add(new CorrectionExtent(
                closeBraceToken.Extent.StartLineNumber,
                branchStatementToken.Extent.EndLineNumber,
                closeBraceToken.Extent.StartColumnNumber,
                branchStatementToken.Extent.EndColumnNumber,
                closeBraceToken.Extent.Text + ' ' + branchStatementToken.Extent.Text,
                fileName));
            return corrections;

        }

        private DiagnosticRecord GetViolationForBraceShouldBeOnNewLine(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            if (tokens.Length > 1 && tokens.Length > closeBracePos)
            {
                var closeBraceToken = tokens[closeBracePos];
                if (tokens[closeBracePos - 1].Kind != TokenKind.NewLine
                    && !tokensToIgnore.Contains(closeBraceToken))
                {
                    return new DiagnosticRecord(
                        GetError(Strings.PlaceCloseBraceErrorShouldBeOnNewLine),
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForBraceShouldBeOnNewLine(tokens, closeBracePos, openBracePos, fileName));
                }
            }

            return null;
        }

        private List<CorrectionExtent> GetCorrectionsForBraceShouldBeOnNewLine(
            Token[] tokens,
            int closeBracePos,
            int openBracePos,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            var closeBraceToken = tokens[closeBracePos];
            corrections.Add(new CorrectionExtent(
                closeBraceToken.Extent.StartLineNumber,
                closeBraceToken.Extent.EndLineNumber,
                closeBraceToken.Extent.StartColumnNumber,
                closeBraceToken.Extent.EndColumnNumber,
                Environment.NewLine + GetIndentation(tokens, closeBracePos, openBracePos) + closeBraceToken.Extent.Text,
                fileName));
            return corrections;
        }

        private bool IsBranchingStatementToken(Token token)
        {
            switch (token.Kind)
            {
                case TokenKind.Else:
                case TokenKind.ElseIf:
                case TokenKind.Catch:
                case TokenKind.Finally:
                    return true;

                default:
                    return false;
            }
        }
        private void AddToDiagnosticRecords(
            DiagnosticRecord diagnosticRecord,
            ref List<DiagnosticRecord> diagnosticRecords)
        {
            if (diagnosticRecord != null)
            {
                diagnosticRecords.Add(diagnosticRecord);
            }
        }

        private List<Token> GetTokens(Ast ast, List<Token> tokens, ref Dictionary<Ast, List<Token>> astTokenMap)
        {
            if (astTokenMap.Keys.Contains(ast))
            {
                return astTokenMap[ast];
            }

            // check if any parent upstream is in the cache
            var parentAst = ast.Parent;
            while (parentAst != null
                    && !astTokenMap.Keys.Contains(parentAst))
            {
                parentAst = parentAst.Parent;
            }

            List<Token> tokenSuperSet = parentAst == null ? tokens : astTokenMap[parentAst];
            var tokenSet = new List<Token>();
            foreach (var token in tokenSuperSet)
            {
                if (Helper.ContainsExtent(ast.Extent, token.Extent))
                {
                    tokenSet.Add(token);
                }
            }

            astTokenMap[ast] = tokenSet;
            return tokenSet;
        }

        private static string GetError(string errorString)
        {
            return string.Format(CultureInfo.CurrentCulture, errorString);
        }
    }
}
