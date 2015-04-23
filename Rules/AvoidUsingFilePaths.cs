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
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.IO;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingFilePaths: Check that rooted file paths are not used.
    /// Examples of rooted file paths are C:\Users or \\windows\powershell
    /// </summary>
    [Export(typeof (IScriptRule))]
    public class AvoidUsingFilePaths : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that rooted file paths are not used. From the ILintScriptRule interface.
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
                    bool isPathValid = false;
                    bool isRootedPath = false;
                    //make sure there is no path 
                    char[] invalidPathChars = Path.GetInvalidPathChars();
                    if (expressionAst.Value.IndexOfAny(invalidPathChars) < 0)
                    {
                        isPathValid = true;
                    }

                    if (isPathValid)
                    {
                        if (Path.IsPathRooted(expressionAst.Value))
                        {
                            isRootedPath = true;
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(expressionAst.Value) && isRootedPath)
                    {
                        //Exclude the case where there are only slashes in the expressions
                        char[] varToTrim = { '/', '\\' };
                        if (!String.IsNullOrEmpty(expressionAst.Value.Trim(varToTrim)))
                        {
                            yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingFilePathError,
                                    expressionAst.Value, Path.GetFileName(fileName)), expressionAst.Extent,
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingFilePathName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingFilePathCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingFilePathDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
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
