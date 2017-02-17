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
    /// A class to walk an AST to check for violation.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class PlaceCloseBrace : ConfigurableRule
    {
        /// <summary>
        /// Indicates if there should or should not be an empty line before a close brace.
        ///
        /// Default value is false.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue:false)]
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

        private HashSet<Token> tokensToIgnore;

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
            var curlyStack = new Stack<Tuple<Token, int>> ();

            // TODO move part common with PlaceOpenBrace to one place
            var tokenOps = new TokenOperations(tokens, ast);
            tokensToIgnore = new HashSet<Token> (tokenOps.GetCloseBracesInCommandElements());

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

                        AddToDiagnosticRecords(
                            GetViolationForBraceShouldBeOnNewLine(tokens, k, openBracePos, fileName),
                            ref diagnosticRecords);

                        if (NoEmptyLineBefore)
                        {
                            AddToDiagnosticRecords(
                                GetViolationForBraceShouldNotFollowEmptyLine(tokens, k, openBracePos, fileName),
                                ref diagnosticRecords);
                        }

                        if (NewLineAfter)
                        {
                            AddToDiagnosticRecords(
                                GetViolationForNewLineShouldFollowBrace(tokens, k, openBracePos, fileName),
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

        private List<CorrectionExtent> GetCorrectionsForNewLineShouldFollowBrace(Token[] tokens, int closeBracePos, int openBracePos, string fileName)
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
            if (tokens[openBracePos - 1].Kind == TokenKind.NewLine)
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

        private DiagnosticRecord GetViolationForNewLineShouldFollowBrace(Token[] tokens, int closeBracePos, int openBracePos, string fileName)
        {
            var expectedNewLinePos = closeBracePos + 1;
            if (tokens.Length > 1 && tokens.Length > expectedNewLinePos)
            {
                var closeBraceToken = tokens[closeBracePos];
                if (tokens[expectedNewLinePos].Kind != TokenKind.NewLine
                    && tokens[expectedNewLinePos].Kind != TokenKind.Comment
                    && !tokensToIgnore.Contains(tokens[closeBracePos]))
                    {

                    return new DiagnosticRecord(
                        GetError(Strings.PlaceCloseBraceErrorShouldFollowNewLine),
                        closeBraceToken.Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        GetCorrectionsForNewLineShouldFollowBrace(tokens, closeBracePos, openBracePos, fileName));
                    }
            }

            return null;
        }

        private DiagnosticRecord GetViolationForBraceShouldBeOnNewLine(Token[] tokens, int closeBracePos, int openBracePos, string fileName)
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
