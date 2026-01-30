// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    // TODO Move all token query related methods here
    /// <summary>
    /// A class to encapsulate all the token querying operations.
    /// </summary>
    public class TokenOperations
    {
        private readonly Token[] tokens;
        private readonly Lazy<LinkedList<Token>> tokensLL;
        private readonly Ast ast;

        public Ast Ast { get { return ast; } }

        /// <summary>
        /// Initializes the fields of the TokenOperations class.
        /// </summary>
        /// <param name="tokens">Tokens referring to the input AST.</param>
        /// <param name="ast">AST that needs to be analyzed.</param>
        public TokenOperations(Token[] tokens, Ast ast)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            this.tokens = tokens;
            this.ast = ast;
            this.tokensLL = new Lazy<LinkedList<Token>>(() => new LinkedList<Token>(this.tokens));
        }

        /// <summary>
        /// Return tokens of kind LCurly that begin a scriptblock expression in an command element.
        ///
        /// E.g. Get-Process * | where { $_.Name -like "powershell" }
        /// In the above example it will return the open brace following the where command.
        /// </summary>
        /// <returns>An enumerable of type Token</returns>
        public IEnumerable<Token> GetOpenBracesInCommandElements()
        {
            return GetBraceInCommandElement(TokenKind.LCurly);
        }

        /// <summary>
        /// Return tokens of kind RCurly that end a scriptblock expression in an command element.
        ///
        /// E.g. Get-Process * | where { $_.Name -like "powershell" }
        /// In the above example it will return the close brace following "powershell".
        /// </summary>
        /// <returns>An enumerable of type Token</returns>
        public IEnumerable<Token> GetCloseBracesInCommandElements()
        {
            return GetBraceInCommandElement(TokenKind.RCurly);
        }

        /// <summary>
        /// Returns pairs of associatd braces.
        /// </summary>
        /// <returns>Tuples of tokens such that item1 is LCurly token and item2 is RCurly token.</returns>
        public IEnumerable<Tuple<Token, Token>> GetBracePairs()
        {
            var openBraceStack = new Stack<Token>();
            IEnumerable<Ast> hashtableAsts = ast.FindAll(oneAst => oneAst is HashtableAst, searchNestedScriptBlocks: true);
            foreach (var token in tokens)
            {
                if (token.Kind == TokenKind.LCurly)
                {
                    openBraceStack.Push(token);
                    continue;
                }

                if (token.Kind == TokenKind.RCurly
                    && openBraceStack.Count > 0)
                {
                    bool closeBraceBelongsToHashTable = hashtableAsts.Any(hashtableAst =>
                    {
                        return hashtableAst.Extent.EndLineNumber == token.Extent.EndLineNumber
                            && hashtableAst.Extent.EndColumnNumber == token.Extent.EndColumnNumber;
                    });

                    if (!closeBraceBelongsToHashTable)
                    {
                        yield return new Tuple<Token, Token>(openBraceStack.Pop(), token);
                    }
                }
            }
        }

        /// <summary>
        /// Returns brace pairs that are on the same line.
        /// </summary>
        /// <returns>Tuples of tokens such that item1 is LCurly token and item2 is RCurly token.</returns>
        public IEnumerable<Tuple<Token, Token>> GetBracePairsOnSameLine()
        {
            foreach (var bracePair in GetBracePairs())
            {
                if (bracePair.Item1.Extent.StartLineNumber == bracePair.Item2.Extent.StartLineNumber)
                {
                    yield return bracePair;
                }
            }
        }

        private IEnumerable<Token> GetBraceInCommandElement(TokenKind tokenKind)
        {
            var cmdElemAsts = ast.FindAll(x => x is CommandElementAst && x is ScriptBlockExpressionAst, true);
            if (cmdElemAsts == null)
            {
                yield break;
            }

            Func<Token, Ast, bool> predicate;

            switch (tokenKind)
            {
                case TokenKind.LCurly:
                    predicate = (x, cmdElemAst) =>
                        x.Kind == TokenKind.LCurly && x.Extent.StartOffset == cmdElemAst.Extent.StartOffset;
                    break;

                case TokenKind.RCurly:
                    predicate = (x, cmdElemAst) =>
                        x.Kind == TokenKind.RCurly && x.Extent.EndOffset == cmdElemAst.Extent.EndOffset;
                    break;

                default:
                    throw new ArgumentException("", nameof(tokenKind));
            }

            foreach (var cmdElemAst in cmdElemAsts)
            {
                var tokenFound = tokens.FirstOrDefault(token => predicate(token, cmdElemAst));
                if (tokenFound != null)
                {
                    yield return tokenFound;
                }
            }
        }

        public static IEnumerable<Token> GetTokens(Ast outerAst, Ast innerAst, Token[] outerTokens)
        {
            ThrowIfNull(outerAst, nameof(outerAst));
            ThrowIfNull(innerAst, nameof(innerAst));
            ThrowIfNull(outerTokens, nameof(outerTokens));

            // check if inner ast belongs in outerAst
            var foundAst = outerAst.Find(x => x.Equals(innerAst), true);
            if (foundAst == null)
            {
                // todo localize
                throw new ArgumentException(String.Format("innerAst cannot be found within outerAst"));
            }

            var tokenOps = new TokenOperations(outerTokens, outerAst);
            return tokenOps.GetTokens(innerAst);
        }

        private static void ThrowIfNull<T>(T param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
        private IEnumerable<Token> GetTokens(Ast ast)
        {
            int k = 0;
            while (k < tokens.Length && tokens[k].Extent.EndOffset <= ast.Extent.StartOffset)
            {
                k++;
            }

            while (k < tokens.Length && tokens[k].Extent.EndOffset <= ast.Extent.EndOffset)
            {
                var token = tokens[k++];
                if (token.Extent.StartOffset >= ast.Extent.StartOffset)
                {
                    yield return token;
                }
            }
        }

        public IEnumerable<LinkedListNode<Token>> GetTokenNodes(TokenKind kind)
        {
            return GetTokenNodes((token) => token.Kind == kind);
        }

        public IEnumerable<LinkedListNode<Token>> GetTokenNodes(Func<Token, bool> predicate)
        {
            var token = tokensLL.Value.First;
            while (token != null)
            {
                if (predicate(token.Value))
                {
                    yield return token;
                }
                token = token.Next;
            }
        }

        private IEnumerable<Tuple<Token, int>> GetTokenAndPrecedingWhitespace(TokenKind kind)
        {
            var lCurlyTokens = GetTokenNodes(TokenKind.LCurly);
            foreach (var item in lCurlyTokens)
            {
                if (item.Previous == null
                    || !OnSameLine(item.Previous.Value, item.Value))
                {
                    continue;
                }

                yield return new Tuple<Token, int>(
                    item.Value,
                    item.Value.Extent.StartColumnNumber - item.Previous.Value.Extent.EndColumnNumber);
            }
        }

        private bool OnSameLine(Token token1, Token token2)
        {
            return token1.Extent.StartLineNumber == token2.Extent.EndLineNumber;
        }

        /// <summary>
        /// Finds the position of a given token in the AST.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> to search for.</param>
        /// <returns>The Ast node directly containing the provided <see cref="Token"/>.</returns>
        public Ast GetAstPosition(Token token)
        {
            FindAstPositionVisitor findAstVisitor = new FindAstPositionVisitor(token.Extent.StartScriptPosition);
            ast.Visit(findAstVisitor);
            return findAstVisitor.AstPosition;
        }

        /// <summary>
        /// Returns a list of non-overlapping ranges (startOffset,endOffset) representing the start
        /// and end of braced member access expressions. These are member accesses where the name is
        /// enclosed in braces. The contents of such braces are treated literally as a member name.
        /// Altering the contents of these braces by formatting is likely to break code.
        /// </summary>
        public List<Tuple<int, int>> GetBracedMemberAccessRanges()
        {
            // A list of (startOffset, endOffset) pairs representing the start
            // and end braces of braced member access expressions.
            var ranges = new List<Tuple<int, int>>();

            var node = tokensLL.Value.First;
            while (node != null)
            {
                switch (node.Value.Kind)
                {
#if CORECLR
                    // TokenKind added in PS7
                    case TokenKind.QuestionDot:
#endif
                    case TokenKind.Dot:
                        break;
                    default:
                        node = node.Next;
                        continue;
                }

                // Note: We don't check if the dot is part of an existing range. When we find
                // a valid range, we skip all tokens inside it - so we won't ever evaluate a token
                // which already part of a previously found range.

                // Backward scan:
                // Determine if this 'dot' is part of a member access.
                // Walk left over contiguous comment tokens that are 'touching'.
                // After skipping comments, the preceding non-comment token must also be 'touching'
                // and one of the expected TokenKinds.
                var leftToken = node.Previous;
                var rightToken = node;
                while (leftToken != null && leftToken.Value.Kind == TokenKind.Comment)
                {
                    if (leftToken.Value.Extent.EndOffset != rightToken.Value.Extent.StartOffset)
                    {
                        leftToken = null;
                        break;
                    }
                    rightToken = leftToken;
                    leftToken = leftToken.Previous;
                }
                if (leftToken == null)
                {
                    // We ran out of tokens before finding a non-comment token to the left or there
                    // was intervening whitespace.
                    node = node.Next;
                    continue;
                }

                if (leftToken.Value.Extent.EndOffset != rightToken.Value.Extent.StartOffset)
                {
                    // There's whitespace between the two tokens
                    node = node.Next;
                    continue;
                }

                // Limit to valid token kinds that can precede a 'dot' in a member access.
                switch (leftToken.Value.Kind)
                {
                    // Note: TokenKind.Number isn't in the list as 5.{Prop} is a syntax error
                    // (Unexpected token). Numbers also have no properties - only methods.
                    case TokenKind.Variable:
                    case TokenKind.Identifier:
                    case TokenKind.StringLiteral:
                    case TokenKind.StringExpandable:
                    case TokenKind.HereStringLiteral:
                    case TokenKind.HereStringExpandable:
                    case TokenKind.RParen:
                    case TokenKind.RCurly:
                    case TokenKind.RBracket:
                        // allowed
                        break;
                    default:
                        // not allowed
                        node = node.Next;
                        continue;
                }

                // Forward Scan:
                // Check that the next significant token is an LCurly
                // Starting from the token after the 'dot', walk right skipping trivia tokens:
                //   - Comment
                //   - NewLine
                //   - LineContinuation (`)
                // These may be multi-line and need not be 'touching' the dot.
                // The first non-trivia token encountered must be an opening curly brace (LCurly) for
                // this dot to begin a braced member access. If it is not LCurly or we run out
                // of tokens, this dot is ignored.
                var scan = node.Next;
                while (scan != null)
                {
                    if (
                        scan.Value.Kind == TokenKind.Comment ||
                        scan.Value.Kind == TokenKind.NewLine ||
                        scan.Value.Kind == TokenKind.LineContinuation
                    )
                    {
                        scan = scan.Next;
                        continue;
                    }
                    break;
                }

                // If we reached the end without finding a significant token, or if the found token
                // is not LCurly, continue.
                if (scan == null || scan.Value.Kind != TokenKind.LCurly)
                {
                    node = node.Next;
                    continue;
                }

                // We have a valid token, followed by a dot, followed by an LCurly.
                // Find the matching RCurly and create the range.
                var lCurlyNode = scan;

                // Depth count braces to find the RCurly which closes the LCurly.
                int depth = 0;
                LinkedListNode<Token> rcurlyNode = null;
                while (scan != null)
                {
                    if (scan.Value.Kind == TokenKind.LCurly) depth++;
                    else if (scan.Value.Kind == TokenKind.RCurly)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            rcurlyNode = scan;
                            break;
                        }
                    }
                    scan = scan.Next;
                }

                // If we didn't find a matching RCurly, something has gone wrong.
                // Should an unmatched pair be caught by the parser as a parse error?
                if (rcurlyNode == null)
                {
                    node = node.Next;
                    continue;
                }

                ranges.Add(new Tuple<int, int>(
                    lCurlyNode.Value.Extent.StartOffset,
                    rcurlyNode.Value.Extent.EndOffset
                ));

                // Skip all tokens inside the excluded range.
                node = rcurlyNode.Next;
            }

            return ranges;
        }
    }
}
