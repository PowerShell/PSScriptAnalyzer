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
        private readonly Ast ast;

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
    }
}
