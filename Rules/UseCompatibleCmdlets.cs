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
    /// A class to check if a script uses cmdlets compatible with a given OS, PowerShell edition, and PowerShell version.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    public class UseCompatibleCmdlets : IScriptRule
    {
        // Valid parameters for this rule.
        private readonly List<string> validParameters;

        // Path of script being analyzed by ScriptAnalyzer.
        private string scriptPath;

        // Lists each target platform (broken down into PowerShell edition, version, os).
        private Dictionary<string, dynamic> platformSpecMap;

        // List of cmdlet names for each target platform.
        private Dictionary<string, HashSet<string>> psCmdletMap;

        // List of cmdlets from desktop PowerShell.
        private HashSet<string> referenceCmdletMap;

        // Name of PowerShell desktop version reference file.
        private readonly string defaultReferenceFileName = "desktop-5.1*";

        // List of user created cmdlets found in ast (functionDefinitionAsts).
        private List<string> customCommands;

        // List of cmdlets that exist on Linux and OSX but do not function correctly.
        private List<string> knownIssuesList;

        // Name of known issues list file.
        private readonly string knownIssuesFileName = "knownCmdletIssues.json";

        // List of all CommandAsts found in Ast.
        private IEnumerable<Ast> commandAsts;

        // List of all diagnostic records for incompatible cmdlets.
        private List<DiagnosticRecord> diagnosticRecords;

        private bool IsInitialized;
        private bool hasInitializationError;
        private string reference;

        private RuleParameters ruleParameters;

        private class RuleParameters
        {
            public string mode;
            public string[] compatibility;
            public string reference;
        }

        public UseCompatibleCmdlets()
        {
            validParameters = new List<string> { "mode", "uri", "compatibility", "reference" };
            IsInitialized = false;
        }

        /// <summary>
        /// Analyzes the given ast to find the violation(s).
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null.</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>An enumerable type containing the violations</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }
            // We do not want to initialize the data structures if the rule is not being used for analysis
            // hence we initialize when this method is called for the first time.
            if (!IsInitialized)
            {
                Initialize();
            }

            if (hasInitializationError)
            {
                Console.WriteLine("There was an error running the UseCompatibleCmdlets Rule. Please check the error log in the Settings file for more info.");
                return new DiagnosticRecord[0];
            }

            diagnosticRecords.Clear();

            scriptPath = fileName;
            customCommands = new List<string>();

            // List of all commands in the script.
            commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // List of user created commands (from a function definition).
            IEnumerable<Ast> functionDefinitionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            foreach (Ast functionDefinition in functionDefinitionAsts)
            {
                string function = functionDefinition.GetType().GetProperty("Name").GetValue(functionDefinition).ToString();
                customCommands.Add(function);
            }

            // If we have no cmdlets to check, we can exit from this rule.
            if (commandAsts.Count() == 0)
            {
                return new DiagnosticRecord[0];
            }

            CheckCompatibility();

            return diagnosticRecords;
        }

        /// <summary>
        /// Check if current command is present in the target platform library.
        /// If not, create a Diagnostic Record for that type.
        /// </summary>
        private void CheckCompatibility()
        {
            foreach (dynamic command in commandAsts)
            {
                bool existsInDesktopPSModule = false;
                bool knownIssueCmdlet = false;
                string commandName = command.GetCommandName();

                foreach (dynamic platform in psCmdletMap)
                {
                    // Check if cmdlet is a known issue for Linux or OSX.
                    if (platform.Key.Contains("linux") || platform.Key.Contains("osx"))
                    {
                        knownIssueCmdlet = checkKnownIssuesList(commandName);
                    }

                    // If the cmdlet exists on the target platform AND it is not a known issue, continue.
                    if (platform.Value.Contains(commandName) && !knownIssueCmdlet)
                    {
                        continue;
                    }
                    // If the cmdlet is user defined in the script, continue.
                    else if (customCommands.Contains(commandName))
                    {
                        continue;
                    }
                    // If the cmdlet does NOT exist on target platform NOR on reference platform, then it is probably a non-builtin
                    // command OR an alias (which we do not check for), so continue.
                    else if (!platform.Value.Contains(commandName) && !referenceCmdletMap.Contains(commandName))
                    {
                        continue;
                    }
                    // If the cmdlet does NOT exist on target platform, but DOES exist on Full PowerShell, incompatible.
                    else if (!(platform.Value.Contains(commandName)) && (referenceCmdletMap.Contains(commandName)))
                    {
                        // Uncomment below for ARM64 warnings instead of errors.
                        // existsInDesktopPSModule = true;
                        GenerateDiagnosticRecord(command, platform.Key, existsInDesktopPSModule, knownIssueCmdlet);
                    }
                    // If cmdlet DOES exist on target platform BUT is a known issue, incompatible.
                    else
                    {
                        GenerateDiagnosticRecord(command, platform.Key, existsInDesktopPSModule, knownIssueCmdlet);
                    }
                }
            }
        }

        /// <summary>
        /// Create an instance of DiagnosticRecord and add it to diagnosticRecords list.
        /// </summary>
        private void GenerateDiagnosticRecord(dynamic commandAst,
                                                string platformName,
                                                bool existsInDesktopPSModule,
                                                bool knownIssueCmdlet)
        {
            var errorMessage = Strings.UseCompatibleCmdletsError;
            bool warning = false;

            if (existsInDesktopPSModule)
            {
                errorMessage = Strings.UseCompatibleCmdletsWindowsPowerShellError;
                warning = true;
            }

            if (knownIssueCmdlet)
            {
                errorMessage = Strings.UseCompatibleCmdletsKnownIssueError;
            }

            var cmdletName = commandAst.GetCommandName();
            var platformInfo = platformSpecMap[platformName];
            var extent = commandAst.Extent;

            diagnosticRecords.Add(new DiagnosticRecord(
                String.Format(
                    errorMessage,
                    cmdletName,
                    platformInfo.PSEdition,
                    platformInfo.PSVersion,
                    platformInfo.OS),
                extent,
                GetName(),
                GetDiagnosticSeverity(warning),
                scriptPath,
                null,
                null));
        }

        /// <summary>
        /// Initialize data structures need to check cmdlet compatibility
        /// </summary>
        private void Initialize()
        {
            diagnosticRecords = new List<DiagnosticRecord>();
            psCmdletMap = new Dictionary<string, HashSet<string>>();
            referenceCmdletMap = new HashSet<string>();
            platformSpecMap = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            SetupCmdletsDictionary();
            IsInitialized = true;
        }

        /// <summary>
        /// Sets up a dictionaries indexed by PowerShell version/edition and OS.
        /// </summary>
        private void SetupCmdletsDictionary()
        {
            // If the method encounters any error it returns early, which implies there is an initialization error.
            // The error will be written to the log file in the Settings directory.
            hasInitializationError = true;

            // Get path to Settings Directory (where the json dictionaries are located).
            string settingsPath = Settings.GetShippedSettingsDirectory();

            if (String.IsNullOrEmpty(settingsPath))
            {
                return;
            }

            string logFile = CreateLogFileName(settingsPath);

            // Retrieve rule parameters provided by user.
            Dictionary<string, object> ruleArgs = Helper.Instance.GetRuleArguments(GetName());

            // If there are no params or if none are valid, return.
            if (ruleArgs == null || !RuleParamsValid(ruleArgs))
            {
                WriteToLogFile(logFile, "Parameters for UseCompatibleCmdlets are invalid.  Make sure to include a 'compatibility' param in your Settings file.");
                return;
            }

            // For each target platform listed in the 'compatibility' param, add it to compatibilityList.
            string[] compatibilityArray = ruleArgs["compatibility"] as string[];

            if (compatibilityArray == null || compatibilityArray.Length.Equals(0))
            {
                WriteToLogFile(logFile, "Compatibility parameter is null.");
                return;
            }

            List<string> compatibilityList = new List<string>();

            foreach (string target in compatibilityArray)
            {
                if (String.IsNullOrEmpty(target))
                {
                    // ignore invalid entries
                    continue;
                }
                compatibilityList.Add(target);
            }

            if (compatibilityList.Count.Equals(0))
            {
                WriteToLogFile(logFile, "There are no target platforms listed in the compatibility parameter.");
            }

            // Create our platformSpecMap from the target platforms in the compatibilityList.
            foreach (string target in compatibilityList)
            {
                string psedition, psversion, os;

                // ignore invalid entries
                if (GetVersionInfoFromPlatformString(target, out psedition, out psversion, out os))
                {
                    platformSpecMap.Add(target, new { PSEdition = psedition, PSVersion = psversion, OS = os });
                }
            }

            // Find corresponding dictionaries for target platforms and create cmdlet maps.
            ProcessDirectory(settingsPath, compatibilityList);

            if (psCmdletMap.Keys.Count != compatibilityList.Count())
            {
                WriteToLogFile(logFile, "One or more of the target platforms listed in the compatibility parameter is not valid.");
                return;
            }

            // Set up our reference cmdlet map.
            referenceCmdletMap = SetUpReferenceCmdletMap(settingsPath, defaultReferenceFileName);

            // Set up known issues list if target is linux or osx.
            string linux = compatibilityList.FirstOrDefault(s => s.Contains("linux"));
            string osx = compatibilityList.FirstOrDefault(s => s.Contains("osx"));

            if (!String.IsNullOrEmpty(linux) || !String.IsNullOrEmpty(osx))
            {
                knownIssuesList = setUpKnownIssuesList(settingsPath);
            }

            // Reached this point, so no initialization error.
            hasInitializationError = false;
        }

        /// <summary>
        /// Search the Settings directory for files in the form [PSEdition]-[PSVersion]-[OS].json.
        /// For each json file found that matches our target platforms, parse file to create cmdlet map.
        /// </summary>
        private void ProcessDirectory(string path, List<string> compatibilityList)
        {
            var jsonFiles = Directory.EnumerateFiles(path, "*.json");
            if (jsonFiles == null)
            {
                return;
            }

            foreach (string file in jsonFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (!compatibilityList.Contains(fileNameWithoutExtension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                dynamic deserialized = JObject.Parse(File.ReadAllText(file));
                psCmdletMap[fileNameWithoutExtension] = GetCmdletsFromData(deserialized);
            }
        }

        /// <summary>
        /// Get a hashset of cmdlet names from a deserialized json file
        /// </summary>
        private HashSet<string> GetCmdletsFromData(dynamic deserializedObject)
        {
            HashSet<string> cmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            dynamic modules = deserializedObject.Modules;
            foreach (dynamic module in modules)
            {
                if (module.ExportedCommands == null)
                {
                    continue;
                }

                foreach (dynamic cmdlet in module.ExportedCommands)
                {
                    string name = cmdlet.Name.ToString();
                    cmdlets.Add(name);
                }
            }
            return cmdlets;
        }

        /// <summary>
        /// Set up cmdlet list for Linux/OSX known issues.
        /// </summary>
        private List<string> setUpKnownIssuesList(string path)
        {
            var knownIssuesFile = Directory.GetFiles(path, knownIssuesFileName);
            dynamic deserialized = JArray.Parse(File.ReadAllText(knownIssuesFile[0]));
            List<string> issues = new List<string>();

            foreach (dynamic cmdlet in deserialized)
            {
                issues.Add(cmdlet.ToString());
            }

            return issues;
        }

        /// <summary>
        /// Set up cmdlet map from the latest desktop version of PowerShell.
        /// </summary>
        private HashSet<string> SetUpReferenceCmdletMap(string path, string fileName)
        {
            string[] cmdletFile = Directory.GetFiles(path, fileName);
            dynamic deserialized = JObject.Parse(File.ReadAllText(cmdletFile[0]));
            return GetCmdletsFromData(deserialized);
        }

        /// <summary>
        /// Check if cmdlet is on the known Issues List for Linux/OSX.
        /// </summary>
        private bool checkKnownIssuesList(string commandName)
        {
            if (knownIssuesList.Contains(commandName, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
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
        /// Check if rule arguments are valid
        /// </summary>
        private bool RuleParamsValid(Dictionary<string, object> ruleArgs)
        {
            return ruleArgs.Keys.All(
                key => validParameters.Any(x => x.Equals(key, StringComparison.OrdinalIgnoreCase)));
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
        /// Create a string with current date/time for log file name.
        /// </summary>
        private string CreateLogFileName(string settingsPath)
        {
            string dateString = String.Format("{0:g}", DateTime.Now);
            string editedDate = (new Regex("\\W")).Replace(dateString, "_");
            string logFile = settingsPath + "\\UseCompatibleCmdletsErrorLog" + editedDate + ".txt";
            return logFile;
        }

        /// <summary>
        /// Writes an error message to the error log file.
        /// </summary>
        private void WriteToLogFile(string logFile, string message)
        {
            using (StreamWriter writer = File.AppendText(logFile))
            {
                writer.WriteLine(message);
            }
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
            const string pattern = @"^(?<psedition>core.*|desktop)-(?<psversion>[\S]+)-(?<os>windows|linux|macos|nano|iot)$";
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
        public DiagnosticSeverity GetDiagnosticSeverity(bool warning)
        {
            if (warning)
            {
                return DiagnosticSeverity.Warning;
            }
            else
            {
                return DiagnosticSeverity.Error;
            }
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
