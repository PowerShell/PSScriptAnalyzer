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
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// A class to walk an AST to check for [violation]
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class PlaceOpenBrace : IScriptRule
    {
        private class RuleArguments
        {
            public bool OnSameLine { get; set; } = true;

            public static RuleArguments Create(Dictionary<string, object> arguments)
            {
                try
                {
                    var ruleArguments = new RuleArguments();
                    var properties = ruleArguments.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        if (arguments.ContainsKey(property.Name))
                        {
                            var type = property.PropertyType;
                            var obj = arguments[property.Name];
                            property.SetValue(ruleArguments, System.Convert.ChangeType(obj, Type.GetTypeCode(type)));
                        }
                    }

                    return ruleArguments;
                }
                catch
                {
                    return new RuleArguments(); // return arguments with defaults
                }
            }
        }

        private RuleArguments ruleArgs;

        public PlaceOpenBrace()
        {
            ruleArgs = RuleArguments.Create(Helper.Instance.GetRuleArguments(this.GetName()));
        }

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

            // TODO Should have the following options
            // * on-same-line
            // * on-new-line
            // * new-line-after
            // * no-empty-line-after

            var tokens = Helper.Instance.Tokens;
            if (ruleArgs.OnSameLine)
            {
                for (int k = 2; k < tokens.Length; k++)
                {
                    if (tokens[k].Kind == TokenKind.LCurly
                        && tokens[k - 1].Kind == TokenKind.NewLine)
                    {
                        yield return new DiagnosticRecord(
                            GetError(),
                            tokens[k].Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName,
                            null,
                            GetSuggestedCorrections(tokens[k - 2], tokens[k], fileName));
                    }
                }
            }
            else
            {
                for (int k = 1; k < tokens.Length; k++)
                {
                    if (tokens[k].Kind == TokenKind.LCurly
                        && tokens[k - 1].Kind != TokenKind.NewLine)
                    {
                        yield return new DiagnosticRecord(
                            GetError(),
                            tokens[k].Extent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            fileName,
                            null,
                            GetSuggestedCorrectionsForNotOneSameLine(tokens, k - 1, k, fileName));
                    }
                }
            }
        }

        private List<CorrectionExtent> GetSuggestedCorrectionsForNotOneSameLine(
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

        private List<CorrectionExtent> GetSuggestedCorrections(Token precedingExpression, Token lCurly, string fileName)
        {
            var corrections = new List<CorrectionExtent>();
            corrections.Add(
                new CorrectionExtent(
                    precedingExpression.Extent.StartLineNumber,
                    lCurly.Extent.EndLineNumber,
                    precedingExpression.Extent.StartColumnNumber,
                    lCurly.Extent.EndColumnNumber,
                    precedingExpression.Text + " " + lCurly.Text,
                    fileName));
            return corrections;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceOpenBraceCommonName);
        }

        public string GetError()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.PlaceOpenBraceError,
                "same");
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.PlaceOpenBraceDescription);
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
                Strings.PlaceOpenBraceName);
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
