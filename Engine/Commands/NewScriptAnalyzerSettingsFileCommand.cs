// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// Creates a new PSScriptAnalyzer settings file.
    /// The emitted file is always named PSScriptAnalyzerSettings.psd1 so that automatic
    /// settings discovery works when the file is placed in a project directory.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "ScriptAnalyzerSettingsFile", SupportsShouldProcess = true,
        HelpUri = "https://github.com/PowerShell/PSScriptAnalyzer")]
    public class NewScriptAnalyzerSettingsFileCommand : PSCmdlet, IOutputWriter
    {
        private const string SettingsFileName = "PSScriptAnalyzerSettings.psd1";

        #region Parameters

        /// <summary>
        /// The directory where the settings file will be created.
        /// Defaults to the current working directory.
        /// </summary>
        [Parameter(Mandatory = false, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// The name of a built-in preset to use as the basis for the
        /// generated settings file. When omitted, all rules and their default
        /// configurable options are included. Valid values are resolved dynamically
        /// from the shipped preset files and tab-completed via an argument completer
        /// registered in PSScriptAnalyzer.psm1.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string BaseOnPreset { get; set; }

        /// <summary>
        /// Overwrite an existing settings file at the target path.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        #endregion Parameters

        #region Overrides

        /// <summary>
        /// Initialise the analyser engine so that rule metadata is available.
        /// </summary>
        protected override void BeginProcessing()
        {
            Helper.Instance = new Helper(SessionState.InvokeCommand);
            Helper.Instance.Initialize();

            ScriptAnalyzer.Instance.Initialize(this, null, null, null, null, true);
        }

        /// <summary>
        /// Generate and write the settings file.
        /// </summary>
        protected override void ProcessRecord()
        {
            // Validate -BaseOnPreset against the dynamically discovered presets.
            if (!string.IsNullOrEmpty(BaseOnPreset))
            {
                var validPresets = Settings.GetSettingPresets().ToList();
                if (!validPresets.Contains(BaseOnPreset, StringComparer.OrdinalIgnoreCase))
                {
                    ThrowTerminatingError(
                        new ErrorRecord(
                            new ArgumentException(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.InvalidPresetName,
                                    BaseOnPreset,
                                    string.Join(", ", validPresets)
                                )
                            ),
                            "InvalidPresetName",
                            ErrorCategory.InvalidArgument,
                            BaseOnPreset
                        )
                    );
                }
            }

            string directory = string.IsNullOrEmpty(Path)
                ? SessionState.Path.CurrentFileSystemLocation.Path
                : GetUnresolvedProviderPathFromPSPath(Path);

            string targetPath = System.IO.Path.Combine(directory, SettingsFileName);

            // Guard against overwriting an existing settings file unless -Force is specified.
            if (File.Exists(targetPath) && !Force)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new IOException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.SettingsFileAlreadyExists,
                                targetPath
                            )
                        ),
                        "SettingsFileAlreadyExists",
                        ErrorCategory.ResourceExists,
                        targetPath
                    )
                );
            }

            string content;
            if (!string.IsNullOrEmpty(BaseOnPreset))
            {
                content = GenerateFromPreset(BaseOnPreset);
            }
            else
            {
                content = GenerateFromAllRules();
            }

            if (ShouldProcess(targetPath, "Create settings file"))
            {
                // Ensure the target directory exists.
                Directory.CreateDirectory(directory);
                File.WriteAllText(targetPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                WriteObject(new FileInfo(targetPath));
            }
        }

        #endregion Overrides

        #region Settings generation

        /// <summary>
        /// Generates settings content from a built-in preset. The preset is parsed and
        /// the output is normalised to include all top-level fields.
        /// </summary>
        private string GenerateFromPreset(string presetName)
        {
            string presetPath = Settings.GetSettingPresetFilePath(presetName);
            if (presetPath == null || !File.Exists(presetPath))
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        new ArgumentException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.PresetNotFound,
                                presetName
                            )
                        ),
                        "PresetNotFound",
                        ErrorCategory.ObjectNotFound,
                        presetName
                    )
                );
            }

            var parsed = new Settings(presetPath);
            var ruleOptionMap = BuildRuleOptionMap();

            var sb = new StringBuilder();
            WriteHeader(sb, presetName);
            sb.AppendLine("@{");

            sb.AppendLine("    # Rules to run. When populated, only these rules are used.");
            sb.AppendLine("    # Leave empty to run all rules.");
            WriteStringArray(sb, "IncludeRules", parsed.IncludeRules);
            sb.AppendLine();

            sb.AppendLine("    # Rules to skip. Takes precedence over IncludeRules.");
            WriteStringArray(sb, "ExcludeRules", parsed.ExcludeRules);
            sb.AppendLine();

            sb.AppendLine("    # Only report diagnostics at these severity levels.");
            sb.AppendLine("    # Leave empty to report all severities.");
            WriteSeverityArray(sb, parsed.Severities);
            sb.AppendLine();

            sb.AppendLine("    # Paths to modules or directories containing custom rules.");
            sb.AppendLine("    # When specified, these rules are loaded in addition to (or instead");
            sb.AppendLine("    # of) the built-in rules, depending on IncludeDefaultRules.");
            sb.AppendLine("    # Note: Relative paths are resolved from the caller's working directory,");
            sb.AppendLine("    # not the location of this settings file.");
            WriteStringArray(sb, "CustomRulePath", parsed.CustomRulePath);
            sb.AppendLine();

            sb.AppendLine("    # When set to $true and CustomRulePath is specified, built-in rules");
            sb.AppendLine("    # are loaded alongside custom rules. Has no effect without CustomRulePath.");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "    IncludeDefaultRules = {0}", parsed.IncludeDefaultRules ? "$true" : "$false"));
            sb.AppendLine();

            sb.AppendLine("    # When set to $true, searches sub-folders under CustomRulePath for");
            sb.AppendLine("    # additional rule modules. Has no effect without CustomRulePath.");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "    RecurseCustomRulePath = {0}", parsed.RecurseCustomRulePath ? "$true" : "$false"));
            sb.AppendLine();

            sb.AppendLine("    # Per-rule configuration. Only configurable rules appear here.");
            sb.AppendLine("    # Values from the preset are shown; other properties use defaults.");

            if (parsed.RuleArguments != null && parsed.RuleArguments.Count > 0)
            {
                sb.AppendLine("    Rules = @{");

                bool firstRule = true;
                foreach (var ruleEntry in parsed.RuleArguments.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
                {
                    if (!firstRule)
                    {
                        sb.AppendLine();
                    }
                    firstRule = false;

                    string ruleName = ruleEntry.Key;
                    var presetArgs = ruleEntry.Value;

                    if (ruleOptionMap.TryGetValue(ruleName, out var optionInfos))
                    {
                        WriteRuleSettings(sb, ruleName, optionInfos, presetArgs);
                    }
                    else
                    {
                        WriteRuleSettingsRaw(sb, ruleName, presetArgs);
                    }
                }

                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine("    Rules = @{}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Generates settings content that includes every available rule with all
        /// configurable properties set to their defaults.
        /// </summary>
        private string GenerateFromAllRules()
        {
            var ruleNames = new List<string>();
            var ruleOptionMap = BuildRuleOptionMap(ruleNames);

            var sb = new StringBuilder();
            WriteHeader(sb, presetName: null);
            sb.AppendLine("@{");

            sb.AppendLine("    # Rules to run. When populated, only these rules are used.");
            sb.AppendLine("    # Leave empty to run all rules.");
            WriteStringArray(sb, "IncludeRules", ruleNames);
            sb.AppendLine();

            sb.AppendLine("    # Rules to skip. Takes precedence over IncludeRules.");
            WriteStringArray(sb, "ExcludeRules", Enumerable.Empty<string>());
            sb.AppendLine();

            sb.AppendLine("    # Only report diagnostics at these severity levels.");
            sb.AppendLine("    # Leave empty to report all severities.");
            WriteSeverityArray(sb, Enumerable.Empty<string>());
            sb.AppendLine();

            sb.AppendLine("    # Paths to modules or directories containing custom rules.");
            sb.AppendLine("    # When specified, these rules are loaded in addition to (or instead");
            sb.AppendLine("    # of) the built-in rules, depending on IncludeDefaultRules.");
            sb.AppendLine("    # Note: Relative paths are resolved from the caller's working directory,");
            sb.AppendLine("    # not the location of this settings file.");
            WriteStringArray(sb, "CustomRulePath", Enumerable.Empty<string>());
            sb.AppendLine();

            sb.AppendLine("    # When set to $true and CustomRulePath is specified, built-in rules");
            sb.AppendLine("    # are loaded alongside custom rules. Has no effect without CustomRulePath.");
            sb.AppendLine("    IncludeDefaultRules = $false");
            sb.AppendLine();

            sb.AppendLine("    # When set to $true, searches sub-folders under CustomRulePath for");
            sb.AppendLine("    # additional rule modules. Has no effect without CustomRulePath.");
            sb.AppendLine("    RecurseCustomRulePath = $false");
            sb.AppendLine();

            sb.AppendLine("    # Per-rule configuration. Only configurable rules appear here.");
            sb.AppendLine("    Rules = @{");

            bool firstRule = true;
            foreach (var kvp in ruleOptionMap.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!firstRule)
                {
                    sb.AppendLine();
                }
                firstRule = false;

                WriteRuleSettings(sb, kvp.Key, kvp.Value, presetArgs: null);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Builds a map of rule name to its configurable property metadata.
        /// Optionally populates a list of all rule names encountered.
        /// </summary>
        private Dictionary<string, List<RuleOptionInfo>> BuildRuleOptionMap(List<string> allRuleNames = null)
        {
            var map = new Dictionary<string, List<RuleOptionInfo>>(StringComparer.OrdinalIgnoreCase);

            string[] modNames = ScriptAnalyzer.Instance.GetValidModulePaths();
            IEnumerable<IRule> rules = ScriptAnalyzer.Instance.GetRule(modNames, null)
                                      ?? Enumerable.Empty<IRule>();

            foreach (IRule rule in rules)
            {
                string name = rule.GetName();
                allRuleNames?.Add(name);

                if (rule is ConfigurableRule)
                {
                    var options = RuleOptionInfo.GetRuleOptions(rule);
                    if (options.Count > 0)
                    {
                        map[name] = options;
                    }
                }
            }

            return map;
        }

        #endregion Settings generation

        #region Formatting helpers

        /// <summary>
        /// Writes a comment header identifying the tool and version that generated
        /// the file, along with the preset if one was specified.
        /// </summary>
        private static void WriteHeader(StringBuilder sb, string presetName)
        {
            Version version = typeof(ScriptAnalyzer).Assembly.GetName().Version;
            string versionStr = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Build);

            sb.AppendLine("#");
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "# PSScriptAnalyzer settings file ({0})",
                versionStr));

            if (!string.IsNullOrEmpty(presetName))
            {
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "# Based on the '{0}' preset.",
                    presetName));
            }

            sb.AppendLine("#");
            sb.AppendLine("# Generated by New-ScriptAnalyzerSettingsFile.");
            sb.AppendLine("#");
            sb.AppendLine();
        }

        /// <summary>
        /// Writes a PowerShell string-array assignment such as IncludeRules = @( ... ).
        /// </summary>
        private static void WriteStringArray(StringBuilder sb, string key, IEnumerable<string> values)
        {
            var items = values?.ToList() ?? new List<string>();

            if (items.Count == 0)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "    {0} = @()", key));
                return;
            }

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "    {0} = @(", key));
            foreach (string item in items)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "        '{0}'", item));
            }
            sb.AppendLine("    )");
        }

        /// <summary>
        /// Writes the Severity array with an inline comment listing valid values.
        /// </summary>
        private static void WriteSeverityArray(StringBuilder sb, IEnumerable<string> values)
        {
            string validValues = string.Join(", ", Enum.GetNames(typeof(RuleSeverity)));
            var items = values?.ToList() ?? new List<string>();

            if (items.Count == 0)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "    Severity = @() # {0}", validValues));
                return;
            }

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "    Severity = @( # {0}", validValues));
            foreach (string item in items)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "        '{0}'", item));
            }
            sb.AppendLine("    )");
        }

        /// <summary>
        /// Writes a rule settings block using option metadata, optionally merging
        /// with values from a preset. Enable always appears first, followed by
        /// the remaining properties sorted alphabetically.
        /// </summary>
        private static void WriteRuleSettings(
            StringBuilder sb,
            string ruleName,
            List<RuleOptionInfo> optionInfos,
            Dictionary<string, object> presetArgs)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "        {0} = @{{", ruleName));

            foreach (RuleOptionInfo option in optionInfos)
            {
                object value = option.DefaultValue;
                if (presetArgs != null
                    && presetArgs.TryGetValue(option.Name, out object presetVal))
                {
                    value = presetVal;
                }

                string formatted = FormatValue(value);
                string comment = FormatPossibleValuesComment(option);

                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "            {0} = {1}{2}",
                    option.Name,
                    formatted,
                    comment));
            }

            sb.AppendLine("        }");
        }

        /// <summary>
        /// Writes preset rule arguments verbatim when no option metadata is available.
        /// </summary>
        private static void WriteRuleSettingsRaw(
            StringBuilder sb,
            string ruleName,
            Dictionary<string, object> args)
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "        {0} = @{{", ruleName));

            foreach (var kvp in args.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "            {0} = {1}",
                    kvp.Key,
                    FormatValue(kvp.Value)));
            }

            sb.AppendLine("        }");
        }

        /// <summary>
        /// Formats a value as a PowerShell literal suitable for inclusion in a .psd1 file.
        /// </summary>
        private static string FormatValue(object value)
        {
            if (value is bool boolVal)
            {
                return boolVal ? "$true" : "$false";
            }

            if (value is int || value is long || value is double || value is float)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            if (value is string strVal)
            {
                return string.Format(CultureInfo.InvariantCulture, "'{0}'", strVal);
            }

            if (value is Array arr)
            {
                if (arr.Length == 0)
                {
                    return "@()";
                }

                var elements = new List<string>();
                foreach (object item in arr)
                {
                    elements.Add(FormatValue(item));
                }
                return string.Format(CultureInfo.InvariantCulture, "@({0})", string.Join(", ", elements));
            }

            // Fallback - treat as string.
            return string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
        }

        /// <summary>
        /// Returns an inline comment listing the valid values, or an empty string
        /// when the option is unconstrained.
        /// </summary>
        private static string FormatPossibleValuesComment(RuleOptionInfo option)
        {
            if (option.PossibleValues == null || option.PossibleValues.Length == 0)
            {
                return string.Empty;
            }

            return " # " + string.Join(", ", option.PossibleValues.Select(v => v.ToString()));
        }

        #endregion Formatting helpers
    }
}
