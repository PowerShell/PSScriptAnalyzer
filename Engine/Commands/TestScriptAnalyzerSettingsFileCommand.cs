// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// TestScriptAnalyzerSettingsFileCommand: Validates a PSScriptAnalyzer settings file.
    /// Checks that the file is parseable, that referenced rules exist, and that all
    /// rule options and their values are valid.
    ///
    /// By default, returns $true when a file is valid, and writes non-terminating
    /// errors describing each problem found (no output on failure beyond the errors).
    /// When -Quiet is specified, returns $true or $false silently.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "ScriptAnalyzerSettingsFile",
        HelpUri = "https://github.com/PowerShell/PSScriptAnalyzer")]
    [OutputType(typeof(bool))]
    public class TestScriptAnalyzerSettingsFileCommand : PSCmdlet, IOutputWriter
    {
        #region Parameters

        /// <summary>
        /// The path to the settings file to validate.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// When specified, returns only $true or $false without writing
        /// errors or warnings. Without this switch the cmdlet writes
        /// non-terminating errors for every problem found.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Quiet { get; set; }

        /// <summary>
        /// Paths to custom rule modules.
        /// When specified, custom rule names are also treated as valid.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string[] CustomRulePath { get; set; }

        /// <summary>
        /// Search sub-folders under the custom rule path.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter RecurseCustomRulePath { get; set; }

        #endregion Parameters

        #region Overrides

        /// <summary>
        /// BeginProcessing: Initialise the analyser engine.
        /// </summary>
        protected override void BeginProcessing()
        {
            Helper.Instance = new Helper(SessionState.InvokeCommand);
            Helper.Instance.Initialize();

            string[] rulePaths = Helper.ProcessCustomRulePaths(
                CustomRulePath, SessionState, RecurseCustomRulePath);
            ScriptAnalyzer.Instance.Initialize(this, rulePaths, null, null, null, rulePaths == null);
        }

        /// <summary>
        /// ProcessRecord: Parse and validate the settings file.
        /// </summary>
        protected override void ProcessRecord()
        {
            string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);

            if (!File.Exists(resolvedPath))
            {
                var error = new ErrorRecord(
                    new FileNotFoundException(string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.SettingsFileNotFound,
                        resolvedPath)),
                    "SettingsFileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath);

                if (Quiet)
                {
                    WriteObject(false);
                }
                else
                {
                    WriteError(error);
                }

                return;
            }

            // Attempt to parse the settings file.
            Settings parsed;
            try
            {
                parsed = new Settings(resolvedPath);
            }
            catch (Exception ex)
            {
                ReportProblem(string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.SettingsFileParseError,
                    ex.Message),
                    "SettingsFileParseError",
                    ErrorCategory.ParserError,
                    resolvedPath);
                return;
            }

            bool isValid = true;

            // Build a set of known rule names from the engine.
            string[] modNames = ScriptAnalyzer.Instance.GetValidModulePaths();
            IEnumerable<IRule> knownRules = ScriptAnalyzer.Instance.GetRule(modNames, null)
                                            ?? Enumerable.Empty<IRule>();

            var ruleMap = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
            foreach (IRule rule in knownRules)
            {
                ruleMap[rule.GetName()] = rule;
            }

            // Validate IncludeRules.
            isValid &= ValidateRuleNames(parsed.IncludeRules, ruleMap, "IncludeRules");

            // Validate ExcludeRules.
            isValid &= ValidateRuleNames(parsed.ExcludeRules, ruleMap, "ExcludeRules");

            // Validate Severity values.
            isValid &= ValidateSeverities(parsed.Severities);

            // Validate rule arguments.
            if (parsed.RuleArguments != null)
            {
                foreach (var ruleEntry in parsed.RuleArguments)
                {
                    string ruleName = ruleEntry.Key;

                    if (!ruleMap.TryGetValue(ruleName, out IRule rule))
                    {
                        ReportProblem(
                            string.Format(CultureInfo.CurrentCulture,
                                Strings.SettingsFileRuleArgRuleNotFound, ruleName),
                            "RuleNotFound",
                            ErrorCategory.ObjectNotFound,
                            ruleName);
                        isValid = false;
                        continue;
                    }

                    if (!(rule is ConfigurableRule))
                    {
                        ReportProblem(
                            string.Format(CultureInfo.CurrentCulture,
                                Strings.SettingsFileRuleNotConfigurable, ruleName),
                            "RuleNotConfigurable",
                            ErrorCategory.InvalidArgument,
                            ruleName);
                        isValid = false;
                        continue;
                    }

                    var optionInfos = RuleOptionInfo.GetRuleOptions(rule);
                    var optionMap = new Dictionary<string, RuleOptionInfo>(StringComparer.OrdinalIgnoreCase);
                    foreach (var opt in optionInfos)
                    {
                        optionMap[opt.Name] = opt;
                    }

                    foreach (var arg in ruleEntry.Value)
                    {
                        string argName = arg.Key;

                        if (!optionMap.TryGetValue(argName, out RuleOptionInfo optionInfo))
                        {
                            ReportProblem(
                                string.Format(CultureInfo.CurrentCulture,
                                    Strings.SettingsFileUnrecognisedOption, ruleName, argName),
                                "UnrecognisedRuleOption",
                                ErrorCategory.InvalidArgument,
                                argName);
                            isValid = false;
                            continue;
                        }

                        // Validate possible values for constrained options.
                        if (optionInfo.PossibleValues != null
                            && optionInfo.PossibleValues.Length > 0
                            && arg.Value is string strValue)
                        {
                            bool valueValid = optionInfo.PossibleValues.Any(pv =>
                                string.Equals(pv.ToString(), strValue, StringComparison.OrdinalIgnoreCase));

                            if (!valueValid)
                            {
                                ReportProblem(
                                    string.Format(CultureInfo.CurrentCulture,
                                        Strings.SettingsFileInvalidOptionValue,
                                        ruleName, argName, strValue,
                                        string.Join(", ", optionInfo.PossibleValues.Select(v => v.ToString()))),
                                    "InvalidRuleOptionValue",
                                    ErrorCategory.InvalidArgument,
                                    strValue);
                                isValid = false;
                            }
                        }
                    }
                }
            }

            if (Quiet)
            {
                WriteObject(isValid);
            }
            else if (isValid)
            {
                WriteObject(true);
            }
        }

        #endregion Overrides

        #region Helpers

        /// <summary>
        /// Reports a validation problem. In quiet mode the problem is silently
        /// recorded; otherwise a non-terminating error is written.
        /// </summary>
        private void ReportProblem(string message, string errorId, ErrorCategory category, object target)
        {
            if (!Quiet)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException(message),
                    errorId,
                    category,
                    target));
            }
        }

        /// <summary>
        /// Validates that rule names from a settings field exist in the known rule set.
        /// Entries containing wildcard characters are skipped as they are pattern-matched
        /// at runtime.
        /// </summary>
        private bool ValidateRuleNames(
            IEnumerable<string> ruleNames,
            Dictionary<string, IRule> ruleMap,
            string fieldName)
        {
            bool valid = true;
            if (ruleNames == null)
            {
                return valid;
            }

            foreach (string name in ruleNames)
            {
                // Skip wildcard patterns such as PSDSC* - these are resolved at runtime.
                if (WildcardPattern.ContainsWildcardCharacters(name))
                {
                    continue;
                }

                if (!ruleMap.ContainsKey(name))
                {
                    ReportProblem(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileRuleNotFound, fieldName, name),
                        "RuleNotFound",
                        ErrorCategory.ObjectNotFound,
                        name);
                    valid = false;
                }
            }

            return valid;
        }

        /// <summary>
        /// Validates severity values against the RuleSeverity enum.
        /// </summary>
        private bool ValidateSeverities(IEnumerable<string> severities)
        {
            bool valid = true;
            if (severities == null)
            {
                return valid;
            }

            foreach (string sev in severities)
            {
                if (!Enum.TryParse<RuleSeverity>(sev, ignoreCase: true, out _))
                {
                    ReportProblem(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileInvalidSeverity,
                            sev,
                            string.Join(", ", Enum.GetNames(typeof(RuleSeverity)))),
                        "InvalidSeverity",
                        ErrorCategory.InvalidArgument,
                        sev);
                    valid = false;
                }
            }

            return valid;
        }

        #endregion Helpers
    }
}
