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
    /// AvoidLongLines: Checks if a script overwrites a cmdlet that comes with PowerShell
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    /// <summary>
    /// A class to check if a script overwrites a cmdlet that comes with PowerShell
    /// </summary>
    public class AvoidOverwritingBuiltInCmdlets : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of AvoidOverwritingBuiltInCmdlets type.
        /// </summary>
        public AvoidOverwritingBuiltInCmdlets() : base()
        {
            initialized = false;
            cmdletMap = new Dictionary<string, HashSet<string>>();
            Enable = true;  // Enable rule by default
        }


        [ConfigurableRuleProperty(defaultValue: "core-6.1.0-windows")]
        public string PowerShellVersion { get; set; }

        private Dictionary<string, HashSet<string>> cmdletMap;
        private bool initialized;


        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var functionDefinitions = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            if (functionDefinitions.Count() < 1)
            {
                // There are no function definitions in this AST and so it's not worth checking the rest of this rule
                return null;
            }

            else
            {
                var diagnosticRecords = new List<DiagnosticRecord>();
                if (!initialized)
                {
                    Initialize();
                    if (!initialized)
                    {
                        throw new Exception("Failed to initialize rule " + GetName());
                    }
                }

                foreach (var functionDef in functionDefinitions)
                {
                    FunctionDefinitionAst funcDef = functionDef as FunctionDefinitionAst;
                    if (funcDef == null)
                    {
                        continue;
                    }

                    string functionName = funcDef.Name;
                    foreach (var map in cmdletMap)
                    {
                        if (map.Value.Contains(functionName))
                        {
                            diagnosticRecords.Add(CreateDiagnosticRecord(functionName, map.Key, functionDef.Extent));
                        }
                    }
                }

                return diagnosticRecords;
            }
        }


        private DiagnosticRecord CreateDiagnosticRecord(string FunctionName, string PSVer, IScriptExtent ViolationExtent)
        {
            var record = new DiagnosticRecord(
                string.Format(CultureInfo.CurrentCulture,
                    string.Format(Strings.AvoidOverwritingBuiltInCmdletsError, FunctionName, PSVer)),
                ViolationExtent,
                GetName(),
                GetDiagnosticSeverity(),
                ViolationExtent.File,
                null
            );
            return record;
        }


        private void Initialize()
        {
            var psVerList = PowerShellVersion.Split(',').ToList();

            string settingsPath = Settings.GetShippedSettingsDirectory();

            if (settingsPath == null || !ContainsReferenceFile(settingsPath))
            {
                return;
            }
            
            ProcessDirectory(settingsPath, psVerList);

            if (cmdletMap.Keys.Count != psVerList.Count())
            {
                return;
            }
            
            initialized = true;
        }


        private bool ContainsReferenceFile(string directory)
        {
            return File.Exists(Path.Combine(directory, PowerShellVersion + ".json"));
        }


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

                cmdletMap.Add(fileNameWithoutExt, GetCmdletsFromData(JObject.Parse(File.ReadAllText(filePath))));
            }
        }


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


        private bool IsValidPlatformString(string fileNameWithoutExt)
        {
            string psedition, psversion, os;
            return GetVersionInfoFromPlatformString(
                fileNameWithoutExt,
                out psedition,
                out psversion,
                out os);
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
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidOverwritingBuiltInCmdletsCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidOverwritingBuiltInCmdletsDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidOverwritingBuiltInCmdletsName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
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
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
