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

        private List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>> violationFinders
            = new List<Func<TokenOperations, IEnumerable<DiagnosticRecord>>>();

        [ConfigurableRuleProperty(defaultValue:true)]
        public bool CheckHashtable { get; set; }

        [ConfigurableRuleProperty(defaultValue:true)]
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
            throw new NotImplementedException();
        }

        private IEnumerable<DiagnosticRecord> FindHashtableViolations(TokenOperations tokenOps)
        {
            var hashtableAsts = tokenOps.Ast.FindAll(ast => ast is HashtableAst, true);
            if (hashtableAsts == null)
            {
                yield break;
            }
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



            // your code goes here
            yield break;
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
