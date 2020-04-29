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
    }
}
