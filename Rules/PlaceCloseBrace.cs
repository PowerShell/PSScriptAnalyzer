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
    /// A class to walk an AST to check for [violation]
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class PlaceCloseBrace : IScriptRule
    {
        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            // TODO Given that we need to make exceptions for
            // ScriptBlockExpressionAst and NamedBlockAst, a
            // simpler approach using only tokens seems more
            // robust - for every close brace, check the position
            // of its corresponding open brace. If the open brace
            // is on a line by itself, use its identation to decide
            // close brace's indentation. If the open brace is
            // preceded by any non new line token then find the
            // fist keyword on the line and use its indentation for
            // the close brace

            var tokens = Helper.Instance.Tokens.ToList();
            var astTokenMap = new Dictionary<Ast, List<Token>>();
            var violationTokens = new HashSet<Token>();
            var diagnosticRecords = new List<DiagnosticRecord>();
            var astItems = ast.FindAll(x => x is ScriptBlockAst
                                            || x is StatementBlockAst
                                            || x is NamedBlockAst,
                                        true);
            foreach (var astItem in astItems)
            {
                var astTokens = GetTokens(astItem, tokens, ref astTokenMap);
                AddToDiagnosticRecords(
                    GetViolationForBraceOnSameLine(astItem, astTokens, fileName, ref violationTokens),
                    ref diagnosticRecords);

                AddToDiagnosticRecords(
                    GetViolationForEmptyLineBeforeBrace(astItem, astTokens, fileName, ref violationTokens),
                    ref diagnosticRecords);
            }

            return diagnosticRecords;
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
        private DiagnosticRecord GetViolationForEmptyLineBeforeBrace(
            Ast ast,
            List<Token> tokens,
            string fileName,
            ref HashSet<Token> violationTokens)
        {
            if (tokens.Count >= 3)
            {
                var closeBraceToken = tokens.Last();
                var extraNewLineToken = tokens[tokens.Count - 2];
                var newLineToken = tokens[tokens.Count - 3];
                if (!violationTokens.Contains(closeBraceToken)
                    && closeBraceToken.Kind == TokenKind.RCurly
                    && extraNewLineToken.Kind == TokenKind.NewLine
                    && newLineToken.Kind == TokenKind.NewLine)
                {
                    violationTokens.Add(closeBraceToken);
                    return new DiagnosticRecord(
                        "Extra new line before close brace",
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetSuggestedCorrectionsForEmptyLineBeforeBrace(ast, closeBraceToken, newLineToken, fileName));
                }
            }

            return null;
        }

        private DiagnosticRecord GetViolationForBraceOnSameLine(
            Ast ast,
            List<Token> tokens,
            string fileName,
            ref HashSet<Token> violationTokens)
        {
            if (tokens.Count >= 2)
            {
                var closeBraceToken = tokens.Last();
                if (!violationTokens.Contains(closeBraceToken)
                    && closeBraceToken.Kind == TokenKind.RCurly
                    && tokens[tokens.Count - 2].Kind != TokenKind.NewLine)
                {
                    violationTokens.Add(closeBraceToken);
                    return new DiagnosticRecord(
                        GetError(),
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetSuggestedCorrectionsForBraceOnSameLine(ast, closeBraceToken, fileName));
                }
            }

            return null;
        }

        private List<CorrectionExtent> GetSuggestedCorrectionsForBraceOnSameLine(
            Ast ast,
            Token closeBraceToken,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            corrections.Add(
                new CorrectionExtent(
                    closeBraceToken.Extent.StartLineNumber,
                    closeBraceToken.Extent.EndLineNumber,
                    closeBraceToken.Extent.StartColumnNumber,
                    closeBraceToken.Extent.EndColumnNumber,
                    Environment.NewLine + GetIndentation(ast) + closeBraceToken.Text,
                    fileName));
            return corrections;
        }

        private string GetIndentation(Ast ast)
        {
            var targetAst = ast;
            if (!(targetAst is NamedBlockAst)
                && targetAst.Parent != null)
            {
                targetAst = targetAst.Parent;
                if (targetAst is ScriptBlockExpressionAst)
                {
                    targetAst = targetAst.Parent ?? targetAst;
                }
            }

            return new String(' ', targetAst.Extent.StartColumnNumber - 1);
        }

        private List<CorrectionExtent> GetSuggestedCorrectionsForEmptyLineBeforeBrace(
            Ast ast,
            Token closeBraceToken,
            Token newLineToken,
            string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            corrections.Add(
                new CorrectionExtent(
                    newLineToken.Extent.StartLineNumber,
                    closeBraceToken.Extent.EndLineNumber,
                    newLineToken.Extent.StartColumnNumber,
                    closeBraceToken.Extent.EndColumnNumber,
                    Environment.NewLine + GetIndentation(ast) + closeBraceToken.Text,
                    fileName));
            return corrections;
        }

        /// <summary>
        /// Retrieves the error message of this rule
        /// </summary>
        private string GetError()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceCloseBraceError);
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceCloseBraceCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceCloseBraceDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public string GetName()
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
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Information;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
