// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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

using Newtonsoft.Json.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseCompatibleCmdlets: Checks if a script uses Cmdlets compatible with a given version and edition of PowerShell.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    /// <summary>
    /// A class to check if a script uses Cmdlets compatible with a given version and edition of PowerShell.
    /// </summary>
    public class UseCompatibleCmdlets : AstVisitor, IScriptRule
    {
        private struct RuleParameters
        {
            public string mode;
            public string[] compatibility;
            public string reference;
        }

        private List<DiagnosticRecord> diagnosticRecords;
        private Dictionary<string, HashSet<string>> psCmdletMap;
        private readonly List<string> validParameters;
        private CommandAst curCmdletAst;
        private Dictionary<string, bool> curCmdletCompatibilityMap;
        private Dictionary<string, dynamic> platformSpecMap;
        private string scriptPath;
        private bool IsInitialized;
        private bool hasInitializationError;
        private string reference;
        private readonly string defaultReference = "desktop-5.1.14393.206-windows";
        private readonly string alternativeDefaultReference = "core-6.1.0-windows";
        private RuleParameters ruleParameters;

        public UseCompatibleCmdlets()
        {
            validParameters = new List<string> { "mode", "uri", "compatibility", "reference" };
            IsInitialized = false;
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

            if (hasInitializationError)
            {
                yield break;
            }

            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            scriptPath = fileName;
            diagnosticRecords.Clear();
            ast.Visit(this);
            foreach (var dr in diagnosticRecords)
            {
                yield return dr;
            }
        }


        /// <summary>
        /// Visits the CommandAst type node in an AST
        /// </summary>
        /// <param name="commandAst">CommandAst node</param>
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
            ResetCurCmdletCompatibilityMap();
            CheckCompatibility();
            GenerateDiagnosticRecords();
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Create an instance of DiagnosticRecord and add it to a list
        /// </summary>
        private void GenerateDiagnosticRecords()
        {
            bool referenceCompatibility = curCmdletCompatibilityMap[reference];

            // If the command is present in reference platform but not in any of the target platforms.
            // Or if the command is not present in reference platform but present in any of the target platforms
            // then declare it as an incompatible cmdlet.
            // If it is present neither in reference platform nor any target platforms, then it is probably a
            // non-builtin command and hence do not declare it as an incompatible cmdlet.
            // Since we do not check for aliases, the XOR-ing will also make sure that aliases are not flagged
            // as they will be found neither in reference platform nor in target platforms
            foreach (var platform in ruleParameters.compatibility)
            {
                var curCmdletCompat = curCmdletCompatibilityMap[platform];
                if (!curCmdletCompat && referenceCompatibility)
                {
                    var cmdletName = curCmdletAst.GetCommandName();
                    var platformInfo = platformSpecMap[platform];
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

        /// <summary>
        /// Initialize data structures need to check cmdlet compatibility
        /// </summary>
        private void Initialize()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            psCmdletMap = new Dictionary<string, HashSet<string>>();
            curCmdletCompatibilityMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            platformSpecMap = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            SetupCmdletsDictionary();
            IsInitialized = true;
        }

        /// <summary>
        /// Sets up a dictionaries indexed by PowerShell version/edition and OS
        /// </summary>
        private void SetupCmdletsDictionary()
        {
            // If the method encounters any error, it returns early
            // which implies there is an initialization error
            hasInitializationError = true;
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

            ruleParameters.compatibility = compatibilityList.ToArray();
            reference = defaultReference;
            if (compatibilityList.Count == 1 && compatibilityList[0] == defaultReference)
            {
                reference = alternativeDefaultReference;
            }
#if DEBUG
            // Setup reference file
            object referenceObject;
            if (ruleArgs.TryGetValue("reference", out referenceObject))
            {
                reference = referenceObject as string;
                if (reference == null)
                {
                    reference = GetStringArgFromListStringArg(referenceObject);
                    if (reference == null)
                    {
                        return;
                    }
                }
            }
#endif
            ruleParameters.reference = reference;

            // check if the reference file has valid platformSpec
            if (!IsValidPlatformString(reference))
            {
                return;
            }

            string settingsPath = Settings.GetShippedSettingsDirectory();
#if DEBUG
            object modeObject;
            if (ruleArgs.TryGetValue("mode", out modeObject))
            {
                // This is for testing only. User should not be specifying mode!
                var mode = GetStringArgFromListStringArg(modeObject);
                ruleParameters.mode = mode;
                switch (mode)
                {
                    case "offline":
                        settingsPath = GetStringArgFromListStringArg(ruleArgs["uri"]);
                        break;

                    case "online": // not implemented yet.
                    case null:
                    default:
                        return;
                }

            }
#endif
            if (settingsPath == null
                || !ContainsReferenceFile(settingsPath))
            {
                return;
            }

            var extentedCompatibilityList = compatibilityList.Union(Enumerable.Repeat(reference, 1));
            foreach (var compat in extentedCompatibilityList)
            {
                string psedition, psversion, os;

                // ignore (warn) invalid entries
                if (GetVersionInfoFromPlatformString(compat, out psedition, out psversion, out os))
                {
                    platformSpecMap.Add(compat, new { PSEdition = psedition, PSVersion = psversion, OS = os });
                    curCmdletCompatibilityMap.Add(compat, true);
                }
            }

            ProcessDirectory(
                settingsPath,
                extentedCompatibilityList);
            if (psCmdletMap.Keys.Count != extentedCompatibilityList.Count())
            {
                return;
            }

            // reached this point, so no error
            hasInitializationError = false;
        }

        /// <summary>
        /// Checks if the given directory has the reference file
        /// directory must be non-null
        /// </summary>
        private bool ContainsReferenceFile(string directory)
        {
            return File.Exists(Path.Combine(directory, reference + ".json"));
        }

        /// <summary>
        /// Resets the values in curCmdletCompatibilityMap to true
        /// </summary>
        private void ResetCurCmdletCompatibilityMap()
        {
            // cannot iterate over collection and change the values, hence the conversion to list
            foreach(var key in curCmdletCompatibilityMap.Keys.ToList())
            {
                curCmdletCompatibilityMap[key] = true;
            }
        }

        private bool IsValidPlatformString(string fileNameWithoutExt)
        {
            string psedition, psversion, os;
            return GetVersionInfoFromPlatformString(
                fileNameWithoutExt,
                out psedition,
                out psversion,
                out os);
        }

        /// <summary>
        /// Gets PowerShell Edition, Version and OS from input string
        /// </summary>
        /// <returns>True if it can retrieve information from string, otherwise, False</returns>
        private bool GetVersionInfoFromPlatformString(
            string fileName,
            out string psedition,
            out string psversion,
            out string os)
        {
            psedition = null;
            psversion = null;
            os = null;
            const string pattern = @"^(?<psedition>core|desktop)-(?<psversion>[\S]+)-(?<os>windows|linux|macos)$";
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

        /// <summary>
        /// Gets the string from a one element string array
        /// </summary>
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

        /// <summary>
        /// Search a directory for files of form [PSEdition]-[PSVersion]-[OS].json
        /// </summary>
        private void ProcessDirectory(string path, IEnumerable<string> acceptablePlatformSpecs)
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
                if (acceptablePlatformSpecs != null
                    && !acceptablePlatformSpecs.Contains(fileNameWithoutExt, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                psCmdletMap[fileNameWithoutExt] = GetCmdletsFromData(JObject.Parse(File.ReadAllText(filePath)));
            }
        }

        /// <summary>
        /// Get a hashset of cmdlet names from a deserialized json file
        /// </summary>
        /// <param name="deserializedObject"></param>
        /// <returns></returns>
        private HashSet<string> GetCmdletsFromData(dynamic deserializedObject)
        {
            var cmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            dynamic modules = deserializedObject.Modules;
            foreach (dynamic module in modules)
            {
                if (module.ExportedCommands == null)
                {
                    continue;
                }

                foreach (dynamic cmdlet in module.ExportedCommands)
                {
                    var name = cmdlet.Name as string;
                    if (name == null)
                    {
                        name = cmdlet.Name.ToObject<string>();
                    }
                    cmdlets.Add(name);
                }
            }

            return cmdlets;
        }

        /// <summary>
        /// Check if rule arguments are valid
        /// </summary>
        private bool RuleParamsValid(Dictionary<string, object> ruleArgs)
        {
            return ruleArgs.Keys.All(
                key => validParameters.Any(x => x.Equals(key, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Check if current command is present in the whitelists
        /// If not, flag the corresponding value in curCmdletCompatibilityMap
        /// </summary>
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
    }
}
