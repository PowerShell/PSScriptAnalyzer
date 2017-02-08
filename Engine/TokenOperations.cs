// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
        private LinkedList<Token> tokensLL;
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
            this.tokensLL = new LinkedList<Token>(this.tokens);
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

        public IEnumerable<Token> GetCloseBraceInOneLineIfStatement()
        {
            return GetBraceInOneLineIfStatment(TokenKind.RCurly);
        }

        public IEnumerable<Token> GetOpenBraceInOneLineIfStatement()
        {
            return GetBraceInOneLineIfStatment(TokenKind.LCurly);
        }

        // TODO Fix code duplication in the following method and GetBraceInCommandElement
        private IEnumerable<Token> GetBraceInOneLineIfStatment(TokenKind tokenKind)
        {
            var ifStatementAsts = ast.FindAll(ast =>
            {
                var ifAst = ast as IfStatementAst;
                if (ifAst == null)
                {
                    return false;
                }

                return ifAst.Extent.StartLineNumber == ifAst.Extent.EndLineNumber;
            },
            true);

            if (ifStatementAsts == null)
            {
                yield break;
            }

            var braceTokens = new List<Token>();
            foreach (var ast in ifStatementAsts)
            {
                var ifStatementAst = ast as IfStatementAst;
                foreach (var clause in ifStatementAst.Clauses)
                {
                    var tokenIf
                        = tokenKind == TokenKind.LCurly
                            ? GetTokens(clause.Item2).FirstOrDefault()
                            : GetTokens(clause.Item2).LastOrDefault();
                    if (tokenIf != null)
                    {
                        yield return tokenIf;
                    }
                }

                if (ifStatementAst.ElseClause == null)
                {
                    continue;
                }

                var tokenElse
                    = tokenKind == TokenKind.LCurly
                            ? GetTokens(ifStatementAst.ElseClause).FirstOrDefault()
                            : GetTokens(ifStatementAst.ElseClause).LastOrDefault();
                if (tokenElse != null)
                {
                    yield return tokenElse;
                }
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
            var token = tokensLL.First;
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

        public IEnumerable<Tuple<Token, int>> GetOpenBracesWithWhiteSpacesBefore()
        {
            return GetTokenAndPrecedingWhitespace(TokenKind.LCurly);
        }

        public IEnumerable<Tuple<Token, int>> GetOpenParensWithWhiteSpacesBefore()
        {
            return GetTokenAndPrecedingWhitespace(TokenKind.LParen);
        }

        private bool OnSameLine(Token token1, Token token2)
        {
            return token1.Extent.StartLineNumber == token2.Extent.EndLineNumber;
        }
    }
}
