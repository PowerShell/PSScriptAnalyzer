//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseApprovedVerbs: Analyzes scripts to check that all defined functions use approved verbs.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseApprovedVerbs : IScriptRule {
        /// <summary>
        /// Analyze script to check that all defined functions use approved verbs
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            List<string> approvedVerbs = typeof(VerbsCommon).GetFields().Concat<FieldInfo>(
                typeof(VerbsCommunications).GetFields()).Concat<FieldInfo>(
                typeof(VerbsData).GetFields()).Concat<FieldInfo>(
                typeof(VerbsDiagnostic).GetFields()).Concat<FieldInfo>(
                typeof(VerbsLifecycle).GetFields()).Concat<FieldInfo>(
                typeof(VerbsSecurity).GetFields()).Concat<FieldInfo>(
                typeof(VerbsOther).GetFields()).Select<FieldInfo, String>(
                item => item.Name).ToList();

            string funcName;
            char[] funcSeperator = { '-' };
            string[] funcNamePieces = new string[2];
            string verb;

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                funcName = Helper.Instance.FunctionNameWithoutScope(funcAst.Name);

                if (funcName != null && funcName.Contains('-'))
                {
                    funcNamePieces = funcName.Split(funcSeperator);
                    verb = funcNamePieces[0];

                    if (!approvedVerbs.Contains(verb, StringComparer.OrdinalIgnoreCase))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsError, funcName),
                            funcAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseApprovedVerbsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }

}
