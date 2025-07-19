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
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseConsistentWhitespace: Checks if whitespace usage is consistent throughout the source file.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConsistentWhitespace : ConfigurableRule
    {
        private enum ErrorKind
        {
            BeforeOpeningBrace, Paren, Operator, SeparatorComma, SeparatorSemi,
            AfterOpeningBrace, BeforeClosingBrace, BeforePipe, AfterPipe, BetweenParameter
        };

        private const int whiteSpaceSize = 1;
        private const string whiteSpace = " ";
        private readonly SortedSet<TokenKind> openParenKeywordAllowList = new SortedSet<TokenKind>()
        {
            TokenKind.If,
            TokenKind.ElseIf,
            TokenKind.Switch,
            TokenKind.For,
            TokenKind.Foreach,
            TokenKind.While,
            TokenKind.Until,
            TokenKind.Do,
            TokenKind.Else,
            TokenKind.Catch,
            TokenKind.Finally
        };

        private List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>> violationFinders
                = new List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>>();

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckOpenBrace { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckInnerBrace { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckPipe { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: false)]
        public bool CheckPipeForRedundantWhitespace { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckOpenParen { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckOperator { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckSeparator { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: false)]
        public bool CheckParameter { get; protected set; }

        [ConfigurableRuleProperty(defaultValue: false)]
        public bool IgnoreAssignmentOperatorInsideHashTable { get; protected set; }

        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);
            if (CheckOpenBrace)
            {
                violationFinders.Add(FindOpenBraceViolations);
                violationFinders.Add(FindSpaceAfterClosingBraceViolations);
                violationFinders.Add(FindKeywordAfterBraceViolations);
            }

            if (CheckInnerBrace)
            {
                violationFinders.Add(FindInnerBraceViolations);
            }

            if (CheckPipe || CheckPipeForRedundantWhitespace)
            {
                violationFinders.Add(FindPipeViolations);
            }

            if (CheckOpenParen)
            {
                violationFinders.Add(FindOpenParenViolations);
            }

            if (CheckOperator)
            {
                violationFinders.Add(FindOperatorViolations);
            }

            if (CheckSeparator)
            {
                violationFinders.Add(FindSeparatorViolations);
            }
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
                throw new ArgumentNullException("ast");
            }

            var tokenOperations = new TokenOperations(Helper.Instance.Tokens, ast);
            var diagnosticRecords = Enumerable.Empty<DiagnosticRecord>();
            foreach (var violationFinder in violationFinders)
            {
                diagnosticRecords = diagnosticRecords.Concat(violationFinder(tokenOperations));
            }

            if (CheckParameter)
            {
                diagnosticRecords = diagnosticRecords.Concat(FindParameterViolations(ast));
            }

            return diagnosticRecords.ToArray(); // force evaluation here
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceDescription);
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
                Strings.UseConsistentWhitespaceName);
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

        private bool IsOperator(Token token)
        {
            return TokenTraits.HasTrait(token.Kind, TokenFlags.AssignmentOperator)
                    || TokenTraits.HasTrait(token.Kind, TokenFlags.BinaryPrecedenceAdd)
                    || TokenTraits.HasTrait(token.Kind, TokenFlags.BinaryPrecedenceMultiply)
                    || TokenTraits.HasTrait(token.Kind, TokenFlags.UnaryOperator)
                    || token.Kind == TokenKind.AndAnd
                    || token.Kind == TokenKind.OrOr;
        }

        private string GetError(ErrorKind kind)
        {
            switch (kind)
            {
                case ErrorKind.BeforeOpeningBrace:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorBeforeOpeningBrace);
                case ErrorKind.AfterOpeningBrace:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorAfterOpeningBrace);
                case ErrorKind.BeforeClosingBrace:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorBeforeClosingInnerBrace);
                case ErrorKind.Operator:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorOperator);
                case ErrorKind.BeforePipe:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorSpaceBeforePipe);
                case ErrorKind.AfterPipe:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorSpaceAfterPipe);
                case ErrorKind.SeparatorComma:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorSeparatorComma);
                case ErrorKind.SeparatorSemi:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorSeparatorSemi);
                case ErrorKind.BetweenParameter:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorSpaceBetweenParameter);
                default:
                    return string.Format(CultureInfo.CurrentCulture, Strings.UseConsistentWhitespaceErrorBeforeParen);
            }
        }

        private IEnumerable<DiagnosticRecord> FindOpenBraceViolations(TokenOperations tokenOperations)
        {
            foreach (var lcurly in tokenOperations.GetTokenNodes(TokenKind.LCurly))
            {
                if (lcurly.Previous == null
                    || !IsPreviousTokenOnSameLine(lcurly)
                    || lcurly.Previous.Value.Kind == TokenKind.LCurly
                    || lcurly.Previous.Value.Kind == TokenKind.Dot
                    || ((lcurly.Previous.Value.TokenFlags & TokenFlags.MemberName) == TokenFlags.MemberName))
                {
                    continue;
                }

                if (lcurly.Previous.Value.Kind == TokenKind.RCurly && lcurly.Previous.Previous != null)
                {
                    var keywordBeforeBrace = lcurly.Previous.Previous.Value;
                    if (IsKeyword(keywordBeforeBrace) && !IsPreviousTokenApartByWhitespace(lcurly.Previous))
                    {
                        yield return new DiagnosticRecord(
                            GetError(ErrorKind.BeforeOpeningBrace),
                            lcurly.Previous.Value.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            tokenOperations.Ast.Extent.File,
                            null,
                            GetCorrections(keywordBeforeBrace, lcurly.Previous.Value, lcurly.Value, false, true).ToList());
                    }
                    continue;
                }

                if (IsPreviousTokenApartByWhitespace(lcurly) || IsPreviousTokenLParen(lcurly))
                {
                    continue;
                }

                yield return new DiagnosticRecord(
                    GetError(ErrorKind.BeforeOpeningBrace),
                    lcurly.Value.Extent,
                    GetName(),
                    GetDiagnosticSeverity(),
                    tokenOperations.Ast.Extent.File,
                    null,
                    GetCorrections(lcurly.Previous.Value, lcurly.Value, lcurly.Next.Value, false, true).ToList());
            }
        }

        private IEnumerable<DiagnosticRecord> FindKeywordAfterBraceViolations(TokenOperations tokenOperations)
        {
            foreach (var keywordNode in tokenOperations.GetTokenNodes(IsKeyword))
            {
                var keyword = keywordNode.Value;

                if (keywordNode.Previous != null)
                {
                    if (keywordNode.Previous.Value.Kind == TokenKind.RCurly &&
                        IsPreviousTokenOnSameLine(keywordNode))
                    {
                        var hasWhitespace = IsPreviousTokenApartByWhitespace(keywordNode);

                        if (!hasWhitespace)
                        {
                            var corrections = new List<CorrectionExtent>
                            {
                                new CorrectionExtent(
                                    keywordNode.Previous.Value.Extent.EndLineNumber,
                                    keyword.Extent.StartLineNumber,
                                    keywordNode.Previous.Value.Extent.EndColumnNumber,
                                    keyword.Extent.StartColumnNumber,
                                    whiteSpace,
                                    keyword.Extent.File)
                            };

                            yield return new DiagnosticRecord(
                                GetError(ErrorKind.BeforeOpeningBrace),
                                keyword.Extent,
                                GetName(),
                                GetDiagnosticSeverity(),
                                tokenOperations.Ast.Extent.File,
                                null,
                                corrections);
                        }
                    }
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindInnerBraceViolations(TokenOperations tokenOperations)
        {
            // Handle opening braces
            foreach (var lCurly in tokenOperations.GetTokenNodes(TokenKind.LCurly))
            {
                if (lCurly.Next == null
                    || (lCurly.Previous != null && !IsPreviousTokenOnSameLine(lCurly))
                    || lCurly.Next.Value.Kind == TokenKind.NewLine
                    || lCurly.Next.Value.Kind == TokenKind.LineContinuation)
                {
                    continue;
                }

                // Special handling for empty braces - they should have a space
                if (lCurly.Next.Value.Kind == TokenKind.RCurly)
                {
                    if (!IsNextTokenApartByWhitespace(lCurly))
                    {
                        var prevToken = lCurly.Previous?.Value ?? lCurly.Value;
                        var nextToken = lCurly.Next?.Value ?? lCurly.Value;

                        yield return new DiagnosticRecord(
                            GetError(ErrorKind.AfterOpeningBrace),
                            lCurly.Value.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            tokenOperations.Ast.Extent.File,
                            null,
                            GetCorrections(prevToken, lCurly.Value, nextToken, true, false).ToList());
                    }
                    continue;
                }

                if (!IsNextTokenApartByWhitespace(lCurly))
                {
                    var prevToken = lCurly.Previous?.Value ?? lCurly.Value;
                    yield return new DiagnosticRecord(
                        GetError(ErrorKind.AfterOpeningBrace),
                        lCurly.Value.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(prevToken, lCurly.Value, lCurly.Next.Value, true, false).ToList());
                }
            }

            // Handle closing braces
            foreach (var rCurly in tokenOperations.GetTokenNodes(TokenKind.RCurly))
            {
                if (rCurly.Previous == null)
                {
                    continue;
                }

                if (!IsPreviousTokenOnSameLine(rCurly)
                    || rCurly.Previous.Value.Kind == TokenKind.NewLine
                    || rCurly.Previous.Value.Kind == TokenKind.LineContinuation
                    || rCurly.Previous.Value.Kind == TokenKind.AtCurly)
                {
                    continue;
                }

                // Skip empty braces that already have space
                if (rCurly.Previous.Value.Kind == TokenKind.LCurly && IsPreviousTokenApartByWhitespace(rCurly))
                {
                    continue;
                }

                // Use AST to check if this is a hashtable
                var ast = tokenOperations.GetAstPosition(rCurly.Value);

                if (ast is HashtableAst hashtableAst)
                {
                    if (rCurly.Value.Extent.EndOffset == hashtableAst.Extent.EndOffset)
                    {
                        continue;
                    }
                }

                bool hasSpace = IsPreviousTokenApartByWhitespace(rCurly);

                if (!hasSpace)
                {
                    var nextToken = rCurly.Next?.Value ?? rCurly.Value;
                    yield return new DiagnosticRecord(
                        GetError(ErrorKind.BeforeClosingBrace),
                        rCurly.Value.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(rCurly.Previous.Value, rCurly.Value, nextToken, false, true).ToList());
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindSpaceAfterClosingBraceViolations(TokenOperations tokenOperations)
        {
            foreach (var rCurly in tokenOperations.GetTokenNodes(TokenKind.RCurly))
            {
                if (rCurly.Next == null
                    || !IsPreviousTokenOnSameLine(rCurly.Next)
                    || rCurly.Next.Value.Kind == TokenKind.NewLine
                    || rCurly.Next.Value.Kind == TokenKind.EndOfInput
                    || rCurly.Next.Value.Kind == TokenKind.Semi
                    || rCurly.Next.Value.Kind == TokenKind.Comma
                    || rCurly.Next.Value.Kind == TokenKind.RParen)
                {
                    continue;
                }

                // Need space after } before keywords, numbers, or another }
                if ((IsKeyword(rCurly.Next.Value)
                    || rCurly.Next.Value.Kind == TokenKind.Number
                    || rCurly.Next.Value.Kind == TokenKind.RCurly)
                    && !IsNextTokenApartByWhitespace(rCurly))
                {
                    var prevToken = rCurly.Previous?.Value ?? rCurly.Value;
                    yield return new DiagnosticRecord(
                        GetError(ErrorKind.BeforeOpeningBrace),
                        rCurly.Value.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(prevToken, rCurly.Value, rCurly.Next.Value, true, false).ToList());
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindPipeViolations(TokenOperations tokenOperations)
        {
            foreach (var pipe in tokenOperations.GetTokenNodes(TokenKind.Pipe))
            {
                if (pipe.Next == null
                    || !IsPreviousTokenOnSameLine(pipe)
                    || pipe.Next.Value.Kind == TokenKind.Pipe
                    || pipe.Next.Value.Kind == TokenKind.NewLine
                    || pipe.Next.Value.Kind == TokenKind.LineContinuation
                    )
                {
                    continue;
                }

                if (!IsNextTokenApartByWhitespace(pipe, out bool hasRedundantWhitespace))
                {
                    if (CheckPipeForRedundantWhitespace && hasRedundantWhitespace || CheckPipe && !hasRedundantWhitespace)
                    {
                        yield return new DiagnosticRecord(
                            GetError(ErrorKind.AfterPipe),
                            pipe.Value.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            tokenOperations.Ast.Extent.File,
                            null,
                            GetCorrections(pipe.Previous.Value, pipe.Value, pipe.Next.Value, true, false).ToList());
                    }
                }
            }

            foreach (var pipe in tokenOperations.GetTokenNodes(TokenKind.Pipe))
            {
                if (pipe.Previous == null
                    || !IsPreviousTokenOnSameLine(pipe)
                    || pipe.Previous.Value.Kind == TokenKind.Pipe
                    || pipe.Previous.Value.Kind == TokenKind.NewLine
                    || pipe.Previous.Value.Kind == TokenKind.LineContinuation
                    )
                {
                    continue;
                }

                if (!IsPreviousTokenApartByWhitespace(pipe, out bool hasRedundantWhitespace))
                {
                    if (CheckPipeForRedundantWhitespace && hasRedundantWhitespace || CheckPipe && !hasRedundantWhitespace)
                    {
                        yield return new DiagnosticRecord(
                        GetError(ErrorKind.BeforePipe),
                        pipe.Value.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(pipe.Previous.Value, pipe.Value, pipe.Next.Value, false, true).ToList());
                    }
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindOpenParenViolations(TokenOperations tokenOperations)
        {
            foreach (var lparen in tokenOperations.GetTokenNodes(TokenKind.LParen))
            {
                if (lparen.Previous != null
                    && IsPreviousTokenOnSameLine(lparen)
                    && TokenTraits.HasTrait(lparen.Previous.Value.Kind, TokenFlags.Keyword)
                    && IsKeyword(lparen.Previous.Value)
                    && !IsPreviousTokenApartByWhitespace(lparen))
                {
                    yield return new DiagnosticRecord(
                        GetError(ErrorKind.Paren),
                        lparen.Value.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(lparen.Previous.Value, lparen.Value, lparen.Next.Value, false, true).ToList());
                }
            }
        }

        private IEnumerable<DiagnosticRecord> FindParameterViolations(Ast ast)
        {
            IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);
            foreach (CommandAst commandAst in commandAsts)
            {
                /// When finding all the command parameter elements, there is no guarantee that
                /// we will read them from the AST in the order they appear in the script (in token
                /// order). So we first sort the tokens by their starting line number, followed by
                /// their starting column number.
                List<Ast> commandParameterAstElements = commandAst.FindAll(
                        testAst => testAst.Parent == commandAst, searchNestedScriptBlocks: false
                    ).OrderBy(
                        e => e.Extent.StartLineNumber
                    ).ThenBy(
                        e => e.Extent.StartColumnNumber
                    ).ToList();

                for (int i = 0; i < commandParameterAstElements.Count - 1; i++)
                {
                    IScriptExtent leftExtent = commandParameterAstElements[i].Extent;
                    IScriptExtent rightExtent = commandParameterAstElements[i + 1].Extent;

                    // Skip if elements are on different lines
                    if (leftExtent.EndLineNumber != rightExtent.StartLineNumber)
                    {
                        continue;
                    }

                    // # 1561 - Skip if the whitespace is inside a string literal
                    // Check if any string in the command contains this whitespace region
                    var stringAsts = commandAst.FindAll(a => a is StringConstantExpressionAst || a is ExpandableStringExpressionAst, true);
                    bool isInsideString = false;
                    foreach (var stringAst in stringAsts)
                    {
                        if (stringAst.Extent.StartOffset < leftExtent.EndOffset &&
                            stringAst.Extent.EndOffset > rightExtent.StartOffset)
                        {
                            isInsideString = true;
                            break;
                        }
                    }

                    if (isInsideString)
                    {
                        continue;
                    }

                    var expectedStartColumnNumberOfRightExtent = leftExtent.EndColumnNumber + 1;
                    if (rightExtent.StartColumnNumber > expectedStartColumnNumberOfRightExtent)
                    {
                        int numberOfRedundantWhiteSpaces = rightExtent.StartColumnNumber - expectedStartColumnNumberOfRightExtent;
                        var correction = new CorrectionExtent(
                            startLineNumber: leftExtent.EndLineNumber,
                            endLineNumber: rightExtent.StartLineNumber,
                            startColumnNumber: leftExtent.EndColumnNumber + 1,
                            endColumnNumber: leftExtent.EndColumnNumber + 1 + numberOfRedundantWhiteSpaces,
                            text: string.Empty,
                            file: leftExtent.File);

                        yield return new DiagnosticRecord(
                            GetError(ErrorKind.BetweenParameter),
                            leftExtent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            leftExtent.File,
                            suggestedCorrections: new CorrectionExtent[] { correction });
                    }
                }
            }
        }

        private bool IsSeparator(Token token)
        {
            return token.Kind == TokenKind.Comma || token.Kind == TokenKind.Semi;
        }

        private IEnumerable<DiagnosticRecord> FindSeparatorViolations(TokenOperations tokenOperations)
        {
            foreach (var tokenNode in tokenOperations.GetTokenNodes(IsSeparator))
            {
                if (tokenNode.Next == null
                    || tokenNode.Next.Value.Kind == TokenKind.NewLine
                    || tokenNode.Next.Value.Kind == TokenKind.Comment
                    || tokenNode.Next.Value.Kind == TokenKind.EndOfInput)
                {
                    continue;
                }

                var separator = tokenNode.Value;

                // Check if comma is part of a parameter value by looking at surrounding tokens
                if (separator.Kind == TokenKind.Comma)
                {
                    // Look for pattern: word,word (no spaces) which indicates parameter value
                    if (tokenNode.Previous != null && tokenNode.Next != null)
                    {
                        var prevTok = tokenNode.Previous.Value;
                        var nextTok = tokenNode.Next.Value;

                        // Skip if comma appears to be within a parameter value (no spaces around it)
                        if ((prevTok.Kind == TokenKind.Identifier || prevTok.Kind == TokenKind.Generic) &&
                            (nextTok.Kind == TokenKind.Identifier || nextTok.Kind == TokenKind.Generic) &&
                            prevTok.Extent.EndColumnNumber == separator.Extent.StartColumnNumber &&
                            separator.Extent.EndColumnNumber == nextTok.Extent.StartColumnNumber)
                        {
                            // This looks like key=value,key=value pattern
                            continue;
                        }
                    }
                }

                var prevToken = tokenNode.Previous.Value;
                var nextToken = tokenNode.Next.Value;

                // Check for space before separator (should not exist)
                if (tokenNode.Previous != null && IsPreviousTokenOnSameLine(tokenNode))
                {
                    var spaceBefore = separator.Extent.StartColumnNumber - prevToken.Extent.EndColumnNumber;
                    if (spaceBefore > 0)
                    {
                        // Remove space before separator
                        yield return new DiagnosticRecord(
                            GetError(separator.Kind == TokenKind.Comma ? ErrorKind.SeparatorComma : ErrorKind.SeparatorSemi),
                            separator.Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            separator.Extent.File,
                            null,
                            new List<CorrectionExtent> {
                                new CorrectionExtent(
                                    prevToken.Extent.EndLineNumber,
                                    separator.Extent.StartLineNumber,
                                    prevToken.Extent.EndColumnNumber,
                                    separator.Extent.StartColumnNumber,
                                    string.Empty,
                                    separator.Extent.File)
                            });
                    }
                }

                // Check for space after separator (should exist)
                if (!IsPreviousTokenApartByWhitespace(tokenNode.Next))
                {
                    var errorKind = separator.Kind == TokenKind.Comma ? ErrorKind.SeparatorComma : ErrorKind.SeparatorSemi;

                    yield return GetDiagnosticRecord(
                        separator,
                        errorKind,
                        new List<CorrectionExtent> {
                            new CorrectionExtent(
                                separator.Extent.EndLineNumber,
                                nextToken.Extent.StartLineNumber,
                                separator.Extent.EndColumnNumber,
                                nextToken.Extent.StartColumnNumber,
                                whiteSpace,
                                separator.Extent.File)
                        });
                }
            }
        }

        private DiagnosticRecord GetDiagnosticRecord(
            Token token,
            ErrorKind errKind,
            List<CorrectionExtent> corrections)
        {
            return new DiagnosticRecord(
                GetError(errKind),
                token.Extent,
                GetName(),
                GetDiagnosticSeverity(),
                token.Extent.File,
                null,
                corrections);
        }

        private bool IsKeyword(Token token)
        {
            return openParenKeywordAllowList.Contains(token.Kind);
        }

        private static bool IsPreviousTokenApartByWhitespace(LinkedListNode<Token> tokenNode)
        {
            return IsPreviousTokenApartByWhitespace(tokenNode, out _);
        }

        private static bool IsPreviousTokenApartByWhitespace(LinkedListNode<Token> tokenNode, out bool hasRedundantWhitespace)
        {
            if (tokenNode.Value.Extent.StartLineNumber != tokenNode.Previous.Value.Extent.StartLineNumber)
            {
                hasRedundantWhitespace = false;
                return true;
            }
            var actualWhitespaceSize = tokenNode.Value.Extent.StartColumnNumber - tokenNode.Previous.Value.Extent.EndColumnNumber;
            hasRedundantWhitespace = actualWhitespaceSize - whiteSpaceSize > 0;
            return whiteSpaceSize == actualWhitespaceSize;
        }

        private static bool IsPreviousTokenLParen(LinkedListNode<Token> tokenNode)
        {
            return tokenNode.Previous.Value.Kind == TokenKind.LParen;
        }

        private static bool IsNextTokenApartByWhitespace(LinkedListNode<Token> tokenNode)
        {
            return IsNextTokenApartByWhitespace(tokenNode, out _);
        }

        private static bool IsNextTokenApartByWhitespace(LinkedListNode<Token> tokenNode, out bool hasRedundantWhitespace)
        {
            var actualWhitespaceSize = tokenNode.Next.Value.Extent.StartColumnNumber - tokenNode.Value.Extent.EndColumnNumber;
            hasRedundantWhitespace = actualWhitespaceSize - whiteSpaceSize > 0;
            return whiteSpaceSize == actualWhitespaceSize;
        }

        private bool IsPreviousTokenOnSameLineAndApartByWhitespace(LinkedListNode<Token> tokenNode)
        {
            return IsPreviousTokenOnSameLine(tokenNode) && IsPreviousTokenApartByWhitespace(tokenNode);
        }

        private IEnumerable<DiagnosticRecord> FindOperatorViolations(TokenOperations tokenOperations)
        {
            foreach (var tokenNode in tokenOperations.GetTokenNodes(IsOperator))
            {
                var token = tokenNode.Value;

                if (IsSeparator(token))
                {
                    continue;
                }

                var skipNullOrDotDot = tokenNode.Previous == null ||
                    tokenNode.Next == null ||
                    token.Kind == TokenKind.DotDot;

                if (skipNullOrDotDot)
                {
                    continue;
                }

                // Exclude assignment operator inside of multi-line hash tables if requested
                if (IgnoreAssignmentOperatorInsideHashTable && tokenNode.Value.Kind == TokenKind.Equals)
                {
                    Ast containingAst = tokenOperations.GetAstPosition(tokenNode.Value);
                    if (containingAst is HashtableAst && containingAst.Extent.EndLineNumber != containingAst.Extent.StartLineNumber)
                    {
                        continue;
                    }
                }

                var isUnaryOperator = TokenTraits.HasTrait(token.Kind, TokenFlags.UnaryOperator);

                // Check if we can skip Unary Method invocations or Unary Postfix invocations
                // E.g., someObject.method(-$variable) or $A++, $B++
                if (isUnaryOperator)
                {
                    if (IsUnaryOperatorInMethodCall(tokenNode) || IsUnaryPostfixOperator(tokenNode))
                    {
                        continue;
                    }
                }

                // Check for 'before' and 'after' whitespaces
                var hasWhitespaceBefore = IsPreviousTokenOnSameLineAndApartByWhitespace(tokenNode);

                if (!hasWhitespaceBefore && isUnaryOperator)
                {
                    // Special case: Don't require space before unary operator if preceded by LParen
                    hasWhitespaceBefore = IsPreviousTokenLParen(tokenNode);
                }

                var hasWhitespaceAfter = tokenNode.Next.Value.Kind == TokenKind.NewLine ||
                    IsPreviousTokenOnSameLineAndApartByWhitespace(tokenNode.Next);

                if (!hasWhitespaceAfter || !hasWhitespaceBefore)
                {
                    yield return new DiagnosticRecord(
                        GetError(ErrorKind.Operator),
                        token.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        tokenOperations.Ast.Extent.File,
                        null,
                        GetCorrections(
                            tokenNode.Previous.Value,
                            token,
                            tokenNode.Next.Value,
                            hasWhitespaceBefore,
                            hasWhitespaceAfter));
                }
            }
        }

        private bool IsUnaryOperatorInMethodCall(LinkedListNode<Token> tokenNode)
        {
            var prevToken = tokenNode.Previous;

            if (prevToken == null || prevToken.Previous == null)
            {
                return false;
            }

            if (!IsPreviousTokenLParen(tokenNode) || tokenNode.Next?.Value.Kind != TokenKind.Variable)
            {
                return false;
            }

            var tokenBeforeLParam = prevToken.Previous.Value;

            // Pattern: someObject.method(-$variable)
            return tokenBeforeLParam.Kind == TokenKind.Dot ||
                (tokenBeforeLParam.TokenFlags & TokenFlags.MemberName) == TokenFlags.MemberName;
        }

        private bool IsUnaryPostfixOperator(LinkedListNode<Token> tokenNode)
        {
            var token = tokenNode.Value;

            if (token.Kind != TokenKind.PlusPlus && token.Kind != TokenKind.MinusMinus)
            {
                return false;
            }

            // Postfix operators come after variables, identifiers, or closing brackets/parentheses
            var prevToken = tokenNode.Previous.Value;

            return prevToken.Kind == TokenKind.Variable ||
                   prevToken.Kind == TokenKind.Identifier ||
                   prevToken.Kind == TokenKind.RBracket ||  // for array access like $arr[0]++
                   prevToken.Kind == TokenKind.RParen ||    // for expressions like ($x)++
                   (prevToken.TokenFlags & TokenFlags.MemberName) == TokenFlags.MemberName;
        }

        private List<CorrectionExtent> GetCorrections(
            Token prevToken,
            Token token,
            Token nextToken,
            bool hasWhitespaceBefore, // if this is false, then the returned correction extent will add a whitespace before the token
            bool hasWhitespaceAfter   // if this is false, then the returned correction extent will add a whitespace after the token
            )
        {
            var sb = new StringBuilder();
            IScriptExtent e1 = token.Extent;
            if (!hasWhitespaceBefore)
            {
                sb.Append(whiteSpace);
                e1 = prevToken.Extent;
            }

            var e2 = token.Extent;
            if (!hasWhitespaceAfter)
            {
                if (!hasWhitespaceBefore)
                {
                    sb.Append(token.Text);
                }

                e2 = nextToken.Extent;
                sb.Append(whiteSpace);
            }

            var extent = new ScriptExtent(
                new ScriptPosition(e1.File, e1.EndLineNumber, e1.EndColumnNumber, null),
                new ScriptPosition(e2.File, e2.StartLineNumber, e2.StartColumnNumber, null)
            );

            return new List<CorrectionExtent>()
            {
                new CorrectionExtent(
                extent,
                sb.ToString(),
                token.Extent.File,
                GetError(ErrorKind.Operator))
            };
        }


        private static bool IsPreviousTokenOnSameLine(LinkedListNode<Token> lparen)
        {
            return lparen.Previous.Value.Extent.EndLineNumber == lparen.Value.Extent.StartLineNumber;
        }
    }
}