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
    class PlaceCloseBrace : IScriptRule
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

            var tokens = Helper.Instance.Tokens;
            foreach (var dr in GetViolationsForBraceOnSameLine(tokens, fileName))
            {
                yield return dr;
            }

            foreach (var dr in GetViolationsForEmptyLineBeforeBrace(tokens, fileName))
            {
                yield return dr;
            }
        }

        private IEnumerable<DiagnosticRecord> GetViolationsForEmptyLineBeforeBrace(
            Token[] tokens,
            string fileName)
        {
            for (int k = 2; k < tokens.Length; k++)
            {
                if (tokens[k].Kind == TokenKind.RCurly
                    && tokens[k - 1].Kind == TokenKind.NewLine
                    && tokens[k - 2].Kind == TokenKind.NewLine)
                {
                    yield return new DiagnosticRecord(
                        "Extra new line before close brace",
                        tokens[k].Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        null);
                }
            }
        }

        private IEnumerable<DiagnosticRecord> GetViolationsForBraceOnSameLine(
            Token[] tokens,
            string fileName)
        {
            for (int k = 1; k < tokens.Length; k++)
            {
                if (tokens[k].Kind == TokenKind.RCurly
                    && tokens[k - 1].Kind != TokenKind.NewLine)
                {
                    yield return new DiagnosticRecord(
                        GetError(),
                        tokens[k].Extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        fileName,
                        null,
                        null);
                }
            }
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
