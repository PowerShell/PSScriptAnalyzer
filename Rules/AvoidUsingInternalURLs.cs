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
using System.Data.Odbc;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingInternalURLs: Check if a URL is potentially an internal URL,
    /// eg://msw, //scratch2/scratch
    /// </summary>
    [Export(typeof (IScriptRule))]
    public class AvoidUsingInternalURLs : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check if any internal URL is used. 
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> expressionAsts = ast.FindAll(testAst => testAst is StringConstantExpressionAst, true);

            if (expressionAsts != null)
            {
                foreach (StringConstantExpressionAst expressionAst in expressionAsts)
                {
                    //Check if XPath is used. If XPath is used, then we don't throw warnings.
                    Ast parentAst = expressionAst.Parent;
                    if (parentAst is InvokeMemberExpressionAst)
                    {
                        InvokeMemberExpressionAst invocation = parentAst as InvokeMemberExpressionAst;
                        if (invocation != null)
                        {
                            if (String.Equals(invocation.Member.ToString(), "SelectSingleNode",StringComparison.OrdinalIgnoreCase) ||
                                String.Equals(invocation.Member.ToString(), "SelectNodes",StringComparison.OrdinalIgnoreCase) ||
                                String.Equals(invocation.Member.ToString(), "Select", StringComparison.OrdinalIgnoreCase) ||
                                String.Equals(invocation.Member.ToString(), "Evaluate",StringComparison.OrdinalIgnoreCase) ||
                                String.Equals(invocation.Member.ToString(), "Matches",StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                        }
                    }


                    bool isPathValid = false;
                    bool isInternalURL = false;
                    //make sure there is no path 
                    char[] invalidPathChars = Path.GetInvalidPathChars();
                    if (expressionAst.Value.IndexOfAny(invalidPathChars) < 0)
                    {
                        isPathValid = true;
                    }

                    //Check if path is UNC or begins with "http:" or "www"
                    if (isPathValid && ((!String.IsNullOrWhiteSpace(expressionAst.Value))) &&
                        (Path.IsPathRooted(expressionAst.Value) ||
                         expressionAst.Value.StartsWith("http:", StringComparison.CurrentCultureIgnoreCase)) ||
                        (expressionAst.Value.StartsWith("www", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        //Exclude the case where there are only slashes in the expressions
                        char[] varToTrim = {'/','\\'};
                        string noSlash = expressionAst.Value.Trim(varToTrim);
                        if (!String.IsNullOrEmpty(noSlash) && noSlash.Trim().Length > 1)
                        {
                            //Check if the string contains two back or forward slashes, such as: \\scratch2\scratch or http:\\www.google.com 
                            bool backSlash = expressionAst.Value.Contains(@"\\");
                            bool forwardSlash = expressionAst.Value.Contains(@"//");
                            string firstPartURL = "";
                            if (backSlash)
                            {
                                //Get the first part of the URL before the first back slash, eg: \\scratch2\scratch we check only scratch2 as the first part
                                string trimmedAddress =
                                    expressionAst.Value.Substring(expressionAst.Value.IndexOf(@"\\") + 2);
                                if (trimmedAddress.Contains(@"\"))
                                {
                                    firstPartURL = trimmedAddress.Substring(0, trimmedAddress.IndexOf(@"\"));
                                }
                                else
                                {
                                    firstPartURL = trimmedAddress;
                                }
                            }
                            else if (forwardSlash)
                            {
                                //Get the first part of the URL before the first forward slash
                                string trimmedAddress =
                                    expressionAst.Value.Substring(expressionAst.Value.IndexOf(@"//") + 2);
                                if (trimmedAddress.Contains(@"/"))
                                {
                                    firstPartURL = trimmedAddress.Substring(0, trimmedAddress.IndexOf(@"/"));
                                }
                                else
                                {
                                    firstPartURL = trimmedAddress;
                                }
                            }
                            else
                            {
                                if (expressionAst.Value.Contains(@"\"))
                                {
                                    firstPartURL = expressionAst.Value.Substring(0, expressionAst.Value.IndexOf(@"\"));
                                }
                                else if (expressionAst.Value.Contains(@"/"))
                                {
                                    firstPartURL = expressionAst.Value.Substring(0, expressionAst.Value.IndexOf(@"/"));
                                }
                                else
                                {
                                    firstPartURL = expressionAst.Value;
                                }
                            }
                            if (!firstPartURL.Contains("."))
                            {
                                isInternalURL = true;
                                //Add a check to exclude potential SDDL format. Check if a string have four components separated by ":"
                                var count = firstPartURL.Count(x => x == ':');
                                if (count == 3 || count == 4 )
                                {
                                    isInternalURL = false;
                                }
                            }
                        }
                        if (isInternalURL)
                        {
                            yield return
                                new DiagnosticRecord(
                                    String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInternalURLsError,
                                        expressionAst.Value), expressionAst.Extent,
                                    GetName(), DiagnosticSeverity.Warning, fileName);

                        }
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingInternalURLsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInternalURLsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInternalURLsDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
