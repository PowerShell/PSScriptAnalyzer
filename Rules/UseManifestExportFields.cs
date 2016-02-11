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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseManifestExportFields: Run Test Module Manifest to check that no deprecated fields are being used.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseManifestExportFields : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Run Test Module Manifest to check that no deprecated fields are being used.
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
            
            if (!fileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            String[] manifestFields = {"FunctionsToExport", "CmdletsToExport", "VariablesToExport", "AliasesToExport"};
            var hashtableAst = ast.Find(x => x is HashtableAst, false) as HashtableAst;

            if (hashtableAst == null)
            {
                //Should we emit a warning if the parser cannot find a hashtable?
                yield break;
            }

            foreach(String field in manifestFields)
            {
                IScriptExtent extent;
                if (!HasAcceptableExportField(field, hashtableAst, out extent) && extent != null)
                {
                    yield return new DiagnosticRecord(GetError(field), extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }                               
                        
        }
        
        private bool HasAcceptableExportField(string key, HashtableAst hast, out IScriptExtent extent)
        {
            extent = null;
            foreach (var pair in hast.KeyValuePairs)
            {
                if (key.Equals(pair.Item1.Extent.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    var arrayAst = pair.Item2.Find(x => x is ArrayLiteralAst, true) as ArrayLiteralAst;
                    if (arrayAst == null)
                    {
                        extent = GetScriptExtent(pair);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return true;
        }


        private ScriptExtent GetScriptExtent(Tuple<ExpressionAst, StatementAst> pair)
        {
            return new ScriptExtent(new ScriptPosition(pair.Item1.Extent.StartScriptPosition.File,
                                                                        pair.Item1.Extent.StartScriptPosition.LineNumber,
                                                                        pair.Item1.Extent.StartScriptPosition.Offset,
                                                                        pair.Item1.Extent.StartScriptPosition.Line),
                                                  new ScriptPosition(pair.Item2.Extent.EndScriptPosition.File,
                                                                        pair.Item2.Extent.EndScriptPosition.LineNumber,
                                                                        pair.Item2.Extent.EndScriptPosition.Offset,
                                                                        pair.Item2.Extent.EndScriptPosition.Line));
        }

        public string GetError(string field)
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseManifestExportFieldsError, field);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseManifestExportFieldsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseManifestExportFieldsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseManifestExportFieldsDescription);
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
