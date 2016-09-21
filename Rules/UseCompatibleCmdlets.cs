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
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

using Newtonsoft.Json;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// A class to walk an AST to check for [violation]
    /// </summary>
    #if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class UseCompatibleCmdlets : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> diagnosticRecords;
        private Dictionary<string, HashSet<string>> psCmdletMap;
        private readonly List<string> validParameters;
        private CommandAst curCmdletAst;
        private Dictionary<string, bool> curCmdletCompatibilityMap;
        private Dictionary<string, dynamic> platformSpecMap;
        private string scriptPath;

        public UseCompatibleCmdlets()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            psCmdletMap = new Dictionary<string, HashSet<string>>();
            validParameters = new List<string> { "mode", "uri", "compatibility" };
            curCmdletCompatibilityMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            platformSpecMap = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            SetupCmdletsDictionary();
        }

        private void SetupCmdletsDictionary()
        {
            Dictionary<string, object> ruleArgs = Helper.Instance.GetRuleArguments(GetName());
            if (ruleArgs == null)
            {
                return;
            }

            if (!RuleParamsValid(ruleArgs))
            {
                return;
            }

            var compatibilityObjectArr = ruleArgs["compatibility"] as object[];
            var compatibilityList = new List<string>();
            if (compatibilityObjectArr == null)
            {
                compatibilityList = ruleArgs["compatibility"] as List<string>;
                if (compatibilityList == null)
                {
                    return;
                }
            }
            else
            {
                foreach (var compatItem in compatibilityObjectArr)
                {
                    var compatString = compatItem as string;
                    if (compatString == null)
                    {
                        // ignore (warn) non-string invalid entries
                        continue;
                    }

                    compatibilityList.Add(compatString);
                }
            }

            foreach (var compat in compatibilityList)
            {
                string psedition, psversion, os;

                // ignore (warn) invalid entries
                if (GetVersionInfoFromPlatformString(compat, out psedition, out psversion, out os))
                {
                    platformSpecMap.Add(compat, new { PSEdition = psedition, PSVersion = psversion, OS = os });
                    curCmdletCompatibilityMap.Add(compat, false);
                }
            }

            var mode = GetStringArgFromListStringArg(ruleArgs["mode"]);
            switch (mode)
            {
                case "online":
                    ProcessOnlineModeArgs(ruleArgs);
                    break;

                case "offline":
                    ProcessOfflineModeArgs(ruleArgs);
                    break;

                case null:
                default:
                    return;
            }
        }

        private bool GetVersionInfoFromPlatformString(
            string fileName,
            out string psedition,
            out string psversion,
            out string os)
        {
            psedition = null;
            psversion = null;
            os = null;
            const string pattern = @"^(?<psedition>core|desktop)-(?<psversion>[\S]+)-(?<os>windows|linux|osx)$";
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
            if (match == Match.Empty)
            {
                return false;
            }
            psedition = match.Groups["psedition"].Value;
            psversion = match.Groups["psversion"].Value;
            os = match.Groups["os"].Value;
            return true;
        }

        private string GetStringArgFromListStringArg(object arg)
        {
            if (arg == null)
            {
                return null;
            }
            var strList = arg as List<string>;
            if (strList == null
                || strList.Count != 1)
            {
                return null;
            }
            return strList[0];
        }

        private void ProcessOfflineModeArgs(Dictionary<string, object> ruleArgs)
        {
            var uri = GetStringArgFromListStringArg(ruleArgs["uri"]);
            if (uri == null)
            {
                // TODO: log this
                return;
            }
            if (!Directory.Exists(uri))
            {
                // TODO: log this
                return;
            }
            foreach (var filePath in Directory.EnumerateFiles(uri))
            {
                var extension = Path.GetExtension(filePath);
                if (String.IsNullOrWhiteSpace(extension)
                    || !extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (!platformSpecMap.ContainsKey(fileNameWithoutExt))
                {
                    continue;
                }

                psCmdletMap[fileNameWithoutExt] = GetCmdletsFromData(JsonConvert.DeserializeObject(File.ReadAllText(filePath)));
            }
        }

        private HashSet<string> GetCmdletsFromData(dynamic deserializedObject)
        {
            var cmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var module in deserializedObject)
            {
                if (module.HasValues == false)
                {
                    continue;
                }

                foreach (var cmdlet in module.Value)
                {
                    if (cmdlet.Name != null)
                    {
                        cmdlets.Add(cmdlet.Name);
                    }
                }
            }
            return cmdlets;
        }

        private void ProcessOnlineModeArgs(Dictionary<string, object> ruleArgs)
        {
            throw new NotImplementedException();
        }

        private bool RuleParamsValid(Dictionary<string, object> ruleArgs)
        {
            return ruleArgs.Keys.All(
                key => validParameters.Any(x => x.Equals(key, StringComparison.OrdinalIgnoreCase)));
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

            scriptPath = fileName;
            diagnosticRecords.Clear();
            ast.Visit(this);
            foreach(var dr in diagnosticRecords)
            {
                yield return dr;
            }
        }


        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (commandAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            var commandName = commandAst.GetCommandName();
            if (commandName == null)
            {
                return AstVisitAction.SkipChildren;
            }

            curCmdletAst = commandAst;
            CheckCompatibility();
            GenerateDiagnosticRecords();
            return AstVisitAction.Continue;
        }

        private void GenerateDiagnosticRecords()
        {
            foreach (var curCmdletCompat in curCmdletCompatibilityMap)
            {
                if (!curCmdletCompat.Value)
                {
                    var cmdletName = curCmdletAst.GetCommandName();
                    var platformInfo = platformSpecMap[curCmdletCompat.Key];
                    var funcNameTokens = Helper.Instance.Tokens.Where(
                                                token =>
                                                Helper.ContainsExtent(curCmdletAst.Extent, token.Extent)
                                                && token.Text.Equals(cmdletName));
                    var funcNameToken = funcNameTokens.FirstOrDefault();
                    var extent = funcNameToken == null ? null : funcNameToken.Extent;
                    diagnosticRecords.Add(new DiagnosticRecord(
                        String.Format(
                            Strings.UseCompatibleCmdletsError,
                            cmdletName,
                            platformInfo.PSEdition,
                            platformInfo.PSVersion,
                            platformInfo.OS),
                        extent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        scriptPath,
                        null,
                        null));
                }
            }
        }

        private void CheckCompatibility()
        {
            string commandName = curCmdletAst.GetCommandName();
            foreach (var platformSpec in psCmdletMap)
            {
                if (platformSpec.Value.Contains(commandName))
                {
                    curCmdletCompatibilityMap[platformSpec.Key] = true;
                }
                else
                {
                    curCmdletCompatibilityMap[platformSpec.Key] = false;
                }
            }
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdletsCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdletsDescription);
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
                Strings.UseCompatibleCmdletsName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public RuleSeverity GetSeverity()
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
