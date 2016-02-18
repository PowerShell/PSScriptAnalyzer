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
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseToExportFieldsInManifest: Checks if AliasToExport, CmdletsToExport, FunctionsToExport and VariablesToExport 
    /// fields do not use wildcards and $null in their entries. 
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseToExportFieldsInManifest : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the AST to check if AliasToExport, CmdletsToExport, FunctionsToExport 
        /// and VariablesToExport fields do not use wildcards and $null in their entries. 
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }
            
            if (fileName == null || !fileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            if (!IsValidManifest(ast, fileName))
            {
                yield break;
            }

            String[] manifestFields = {"FunctionsToExport", "CmdletsToExport", "VariablesToExport", "AliasesToExport"};
            var hashtableAst = ast.Find(x => x is HashtableAst, false) as HashtableAst;
            
            if (hashtableAst == null)
            {                                
                yield break;
            }

            foreach(String field in manifestFields)
            {
                IScriptExtent extent;
                if (!HasAcceptableExportField(field, hashtableAst, ast.Extent.Text, out extent) && extent != null)
                {
                    yield return new DiagnosticRecord(GetError(field), extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }                               
                        
        }
        
        /// <summary>
        /// Checks if the manifest file is valid. 
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns>A boolean value indicating the validity of the manifest file.</returns>
        private bool IsValidManifest(Ast ast, string fileName)
        {
            var missingManifestRule = new MissingModuleManifestField();
            return !missingManifestRule.AnalyzeScript(ast, fileName).GetEnumerator().MoveNext();
                    
        }
                
        /// <summary>
        /// Checks if the *ToExport fields are explicitly set to arrays, eg. @(...), and the array entries do not contain any wildcard.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hast"></param>
        /// <param name="scriptText"></param>
        /// <param name="extent"></param>
        /// <returns>A boolean value indicating if the the ToExport fields are explicitly set to arrays or not.</returns>
        private bool HasAcceptableExportField(string key, HashtableAst hast, string scriptText, out IScriptExtent extent)
        {
            extent = null;
            foreach (var pair in hast.KeyValuePairs)
            {
                if (pair.Item1 is StringConstantExpressionAst
                            && key.Equals((pair.Item1 as StringConstantExpressionAst).Value,
                                            StringComparison.OrdinalIgnoreCase))
                {
                    //checks for wildcard in the entry.
                    var elementWithWildcard = pair.Item2.Find(x => x is StringConstantExpressionAst
                            && WildcardPattern.ContainsWildcardCharacters((x as StringConstantExpressionAst).Value), false);

                    if (elementWithWildcard == null)
                    {
                        //checks for $null in the entry.
                        var nullAst = pair.Item2.Find(x => x is VariableExpressionAst
                            && (x as VariableExpressionAst).ToString().Equals("$null", StringComparison.OrdinalIgnoreCase), false);
                        
                        if (nullAst == null)
                        {
                            return true;
                        }
                        else
                        {
                            extent = nullAst.Extent;
                            return false;
                        }
                    }
                    else
                    {
                        extent = elementWithWildcard.Extent;
                        return false;
                    }                                            
                }

            }
            return true;
        }       

        public string GetError(string field)
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestError, field);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseToExportFieldsInManifestName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestDescription);
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
