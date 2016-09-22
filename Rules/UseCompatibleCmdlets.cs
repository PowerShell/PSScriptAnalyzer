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
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

using Newtonsoft.Json.Linq;

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
        private bool IsInitialized;

        public UseCompatibleCmdlets()
        {
            validParameters = new List<string> { "mode", "uri", "compatibility" };
            IsInitialized = false;
        }

        private void Initialize()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            psCmdletMap = new Dictionary<string, HashSet<string>>();
            curCmdletCompatibilityMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            platformSpecMap = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            SetupCmdletsDictionary();
            IsInitialized = true;
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

            object modeObject;
            if (ruleArgs.TryGetValue("mode", out modeObject))
            {
                // This is for testing only. User should not be specifying mode!
                var mode = GetStringArgFromListStringArg(modeObject);
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
                        break;
                }

                return;
            }

            var settingsPath = GetSettingsDirectory();
            if (settingsPath == null)
            {
                return;
            }

            ProcessDirectory(settingsPath);
        }

        private void ResetCurCmdletCompatibilityMap()
        {
            // cannot iterate over collection and change the values, hence the conversion to list
            foreach(var key in curCmdletCompatibilityMap.Keys.ToList())
            {
                curCmdletCompatibilityMap[key] = true;
            }
        }

        private string GetSettingsDirectory()
        {
            // Find the compatibility files in Settings folder
            var path = this.GetType().GetTypeInfo().Assembly.Location;
            if (String.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var settingsPath = Path.Combine(Path.GetDirectoryName(path), "Settings");
            if (!Directory.Exists(settingsPath))
            {
                // try one level down as the PSScriptAnalyzer module structure is not consistent
                // CORECLR binaries are in PSScriptAnalyzer/coreclr/, PowerShell v3 binaries are in PSScriptAnalyzer/PSv3/
                // and PowerShell v5 binaries are in PSScriptAnalyzer/
                settingsPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(path)), "Settings");
                if (!Directory.Exists(settingsPath))
                {
                    return null;
                }
            }

            return settingsPath;
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

            ProcessDirectory(uri);
        }

        private void ProcessDirectory(string path)
        {
            foreach (var filePath in Directory.EnumerateFiles(path))
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

                psCmdletMap[fileNameWithoutExt] = GetCmdletsFromData(JObject.Parse(File.ReadAllText(filePath)));
            }

            RemoveUnavailableKeys();
        }

        private void RemoveUnavailableKeys()
        {
            var keysToRemove = new List<string>();
            foreach (var key in platformSpecMap.Keys)
            {
                if (!psCmdletMap.ContainsKey(key))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                platformSpecMap.Remove(key);
                curCmdletCompatibilityMap.Remove(key);
            }
        }

        private HashSet<string> GetCmdletsFromData(dynamic deserializedObject)
        {
            var cmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            dynamic modules = deserializedObject.Modules;
            foreach (var module in modules)
            {
                foreach (var cmdlet in module.ExportedCommands)
                {
                    var name = cmdlet.Name.Value as string;
                    cmdlets.Add(name);
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
            // we do not want to initialize the data structures if the rule is not being used for analysis
            // hence we initialize when this method is called for the first time
            if (!IsInitialized)
            {
                Initialize();
            }

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
