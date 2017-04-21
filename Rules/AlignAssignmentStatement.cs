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
    /// A class to walk an AST to check if consecutive assignment statements are aligned.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class AlignAssignmentStatement : ConfigurableRule
    {
        private readonly char whitespaceChar = ' ';

        private List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>> violationFinders
            = new List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>>();

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckHashtable { get; set; }

        [ConfigurableRuleProperty(defaultValue: true)]
        public bool CheckDSCConfiguration { get; set; }

        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);
            if (CheckHashtable)
            {
                violationFinders.Add(FindHashtableViolations);
            }

            if (CheckDSCConfiguration)
            {
                violationFinders.Add(FindDSCConfigurationViolations);
            }
        }

        private IEnumerable<DiagnosticRecord> FindDSCConfigurationViolations(TokenOperations arg)
        {
            yield break;
        }

        private IEnumerable<DiagnosticRecord> FindHashtableViolations(TokenOperations tokenOps)
        {
            var hashtableAsts = tokenOps.Ast.FindAll(ast => ast is HashtableAst, true);
            if (hashtableAsts == null)
            {
                yield break;
            }

            // it is probably much easier have a hashtable writer that formats the hashtable and writes it
            // but it my make handling comments hard. So we need to use this approach.

            // check if each key is on a separate line
            // align only if each key=val pair is on a separate line
            // if each pair on a separate line
            //   find all the assignment operators
            //   if all the assignment operators are aligned (check the column number of each alignment operator)
            //      skip
            //   else
            //      find the distance between the assignment operaters its left expression
            //      find the longest left expression
            //      make sure all the assignment operators are in the same column as that of the longest left hand.
            //   else

            var alignments = new List<int>();
            foreach (var astItem in hashtableAsts)
            {
                var hashtableAst = (HashtableAst)astItem;
                if (!HasKeysOnSeparateLines(hashtableAst))
                {
                    continue;
                }

                var nodeTuples = GetExtents(tokenOps, hashtableAst);
                if (nodeTuples == null
                    || nodeTuples.Count == 0
                    || !nodeTuples.All(t => t.Item1.StartLineNumber == t.Item2.EndLineNumber))
                {
                    continue;
                }

                var widestKeyExtent = nodeTuples
                    .Select(t => t.Item1)
                    .Aggregate((t1, tAggregate) => {
                    return TokenOperations.GetExtentWidth(tAggregate) > TokenOperations.GetExtentWidth(t1)
                        ? tAggregate
                        : t1;
                });
                var expectedStartColumnNumber = widestKeyExtent.EndColumnNumber + 1;
                foreach (var extentTuple in nodeTuples)
                {
                    if (extentTuple.Item2.StartColumnNumber != expectedStartColumnNumber)
                    {
                        yield return new DiagnosticRecord(
                            GetError(),
                            extentTuple.Item2,
                            GetName(),
                            GetDiagnosticSeverity(),
                            extentTuple.Item1.File,
                            null,
                            GetHashtableCorrections(extentTuple, expectedStartColumnNumber).ToList());
                    }
                }
            }
        }

        private IEnumerable<CorrectionExtent> GetHashtableCorrections(
            Tuple<IScriptExtent, IScriptExtent> extentTuple,
            int expectedStartColumnNumber)
        {
            var equalExtent = extentTuple.Item2;
            var lhsExtent = extentTuple.Item1;
            var columnDiff = expectedStartColumnNumber - equalExtent.StartColumnNumber;
            yield return new CorrectionExtent(
                lhsExtent.EndLineNumber,
                equalExtent.StartLineNumber,
                lhsExtent.EndColumnNumber,
                equalExtent.StartColumnNumber,
                new String(whitespaceChar, expectedStartColumnNumber - lhsExtent.EndColumnNumber),
                GetError());
        }

        private string GetError()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementError);
        }

        private static IList<Tuple<IScriptExtent, IScriptExtent>> GetExtents(
            TokenOperations tokenOps,
            HashtableAst hashtableAst)
        {
            var nodeTuples = new List<Tuple<IScriptExtent, IScriptExtent>>();
            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                var keyStartOffset = kvp.Item1.Extent.StartOffset;
                var keyTokenNode = tokenOps.GetTokenNodes(
                    token => token.Extent.StartOffset == keyStartOffset).FirstOrDefault();
                if (keyTokenNode == null
                    || keyTokenNode.Next == null
                    || keyTokenNode.Next.Value.Kind != TokenKind.Equals)
                {
                    return null;
                }

                nodeTuples.Add(new Tuple<IScriptExtent, IScriptExtent>(
                    kvp.Item1.Extent,
                    keyTokenNode.Next.Value.Extent));
            }

            return nodeTuples;
        }

        private bool HasKeysOnSeparateLines(HashtableAst hashtableAst)
        {
            var lines = new HashSet<int>();
            foreach (var kvp in hashtableAst.KeyValuePairs)
            {
                if (lines.Contains(kvp.Item1.Extent.StartLineNumber))
                {
                    return false;
                }
                else
                {
                    lines.Add(kvp.Item1.Extent.StartLineNumber);
                }
            }

            return true;
        }

        /// <summary>
        /// Analyzes the given ast to find if consecutive assignment statements are aligned.
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
            // only handles one line assignments
            // if the rule encounters assignment statements that are multi-line, the rule will ignore that block

            var tokenOps = new TokenOperations(Helper.Instance.Tokens, ast);
            foreach (var violationFinder in violationFinders)
            {
                foreach (var diagnosticRecord in violationFinder(tokenOps))
                {
                    yield return diagnosticRecord;
                }
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AlignAssignmentStatementDescription);
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
                Strings.AlignAssignmentStatementName);
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
    }
}
