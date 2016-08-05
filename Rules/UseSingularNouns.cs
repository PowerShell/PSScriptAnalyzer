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
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// CmdletSingularNoun: Analyzes scripts to check that all defined cmdlets use singular nouns.
    /// 
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class CmdletSingularNoun : IScriptRule {

        private readonly string[] nounWhiteList =
        {
            "Data"
        };

        /// <summary>
        /// Checks that all defined cmdlet use singular noun
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullCommandInfoError);

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            char[] funcSeperator = { '-' };
            string[] funcNamePieces = new string[2];

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                if (funcAst.Name != null && funcAst.Name.Contains('-'))
                {
                    funcNamePieces = funcAst.Name.Split(funcSeperator);
                    String nounPart = funcNamePieces[1];

                    // Convert the noun part of the function into a series of space delimited words
                    // This helps the PluralizationService to provide an accurate determination about the plurality of the string
                    nounPart = SplitCamelCaseString(nounPart);
                    var words = nounPart.Split(new char [] { ' ' });
                    var noun = words.LastOrDefault();
                    if (noun == null)
                    {
                        continue;
                    }
                    var ps = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

                    if (!ps.IsSingular(noun) && ps.IsPlural(noun))
                    {
                        IScriptExtent extent = Helper.Instance.GetScriptExtentForFunctionName(funcAst);
                        if (nounWhiteList.Contains(noun, StringComparer.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (null == extent)
                        {
                            extent = funcAst.Extent;
                        }
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsError, funcAst.Name),
                            extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseSingularNounsName);
        }

        /// <summary>
        /// GetName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsDescription);
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

        /// <summary>
        /// SplitCamelCaseString: Splits a Camel Case'd string into individual words with space delimited
        /// </summary>
        private string SplitCamelCaseString(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return String.Empty;
            }

            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }

}
