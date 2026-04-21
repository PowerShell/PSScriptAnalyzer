// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// Validates a PSScriptAnalyzer settings file as a self-contained unit.
    /// Checks that the file is parseable, that referenced rules exist, and that all
    /// rule options and their values are valid.
    ///
    /// Custom rule paths, RecurseCustomRulePath and IncludeDefaultRules are read
    /// from the settings file itself so that validation reflects what
    /// Invoke-ScriptAnalyzer would see when given the same file.
    ///
    /// In the default mode each problem is emitted as a DiagnosticRecord with the
    /// source extent of the offending text. When -Quiet is specified, returns only
    /// $true or $false - indicating whether the settings file is valid.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "ScriptAnalyzerSettingsFile",
        HelpUri = "https://github.com/PowerShell/PSScriptAnalyzer")]
    [OutputType(typeof(DiagnosticRecord))]
    [OutputType(typeof(bool))]
    public class TestScriptAnalyzerSettingsFileCommand : PSCmdlet, IOutputWriter
    {
        private const string RuleName = "Test-ScriptAnalyzerSettingsFile";

        #region Parameters

        /// <summary>
        /// The path to the settings file to validate.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// When specified, returns only $true or $false without emitting
        /// diagnostic records. Without this switch the cmdlet writes a
        /// DiagnosticRecord for every problem found and produces no output
        /// when the file is valid.
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Quiet { get; set; }

        #endregion Parameters

        #region Private state

        private string _resolvedPath;
        private List<DiagnosticRecord> _diagnostics;

        #endregion Private state

        #region Overrides

        /// <summary>
        /// Initialise the helper. Full engine initialisation is
        /// deferred to ProcessRecord because we need to read CustomRulePath and
        /// IncludeDefaultRules from the settings file first.
        /// </summary>
        protected override void BeginProcessing()
        {
            Helper.Instance = new Helper(SessionState.InvokeCommand);
            Helper.Instance.Initialize();
        }

        /// <summary>
        /// ProcessRecord: Parse and validate the settings file.
        /// </summary>
        protected override void ProcessRecord()
        {
            _resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);
            _diagnostics = new List<DiagnosticRecord>();

            if (!File.Exists(_resolvedPath))
            {
                if (Quiet)
                {
                    WriteObject(false);
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException(string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsFileNotFound,
                            _resolvedPath)),
                        "SettingsFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        _resolvedPath));
                }

                return;
            }

            // Parse with the PowerShell AST to get source extents.
            ScriptBlockAst scriptAst = Parser.ParseFile(
                _resolvedPath,
                out Token[] tokens,
                out ParseError[] parseErrors
                );

            if (parseErrors != null && parseErrors.Length > 0)
            {
                if (Quiet)
                {
                    WriteObject(false);
                }
                else
                {
                    foreach (ParseError pe in parseErrors)
                    {
                        AddDiagnostic(
                            string.Format(CultureInfo.CurrentCulture,
                                Strings.SettingsFileParseError, pe.Message),
                            pe.Extent,
                            DiagnosticSeverity.ParseError);
                    }

                    EmitDiagnostics();
                }

                return;
            }

            // Locate the root hashtable.
            HashtableAst rootHashtable = scriptAst.Find(ast => ast is HashtableAst, searchNestedScriptBlocks: false) as HashtableAst;
            if (rootHashtable == null)
            {
                if (Quiet)
                {
                    WriteObject(false);
                }
                else
                {
                    AddDiagnostic(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileParseError, "File does not contain a hashtable."),
                        scriptAst.Extent,
                        DiagnosticSeverity.Error);
                    EmitDiagnostics();
                }

                return;
            }

            // Also parse via Settings to get the evaluated data.
            Settings parsed;
            try
            {
                parsed = new Settings(_resolvedPath);
            }
            catch (Exception ex)
            {
                if (Quiet)
                {
                    WriteObject(false);
                }
                else
                {
                    AddDiagnostic(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileParseError, ex.Message),
                        rootHashtable.Extent,
                        DiagnosticSeverity.Error);
                    EmitDiagnostics();
                }

                return;
            }

            // Initialise the analyser engine using custom rule paths and
            // IncludeDefaultRules from the settings file so that validation
            // reflects the same rule set Invoke-ScriptAnalyzer would use (given
            // this settings file).
            string[] rulePaths = Helper.ProcessCustomRulePaths(
                parsed.CustomRulePath?.ToArray(),
                SessionState,
                parsed.RecurseCustomRulePath);

            // Treat an empty array the same as null — no custom paths were specified.
            if (rulePaths != null && rulePaths.Length == 0)
            {
                rulePaths = null;
            }

            bool includeDefaultRules = rulePaths == null || parsed.IncludeDefaultRules;
            ScriptAnalyzer.Instance.Initialize(this, rulePaths, null, null, null, includeDefaultRules);

            // Build lookup structures.
            var topLevelMap = BuildAstKeyMap(rootHashtable);

            string[] modNames = ScriptAnalyzer.Instance.GetValidModulePaths();
            IEnumerable<IRule> knownRules = ScriptAnalyzer.Instance.GetRule(modNames, null)
                                            ?? Enumerable.Empty<IRule>();

            var ruleMap = new Dictionary<string, IRule>(StringComparer.OrdinalIgnoreCase);
            foreach (IRule rule in knownRules)
            {
                ruleMap[rule.GetName()] = rule;
            }

            // Validate IncludeRules.
            ValidateRuleNameArray(parsed.IncludeRules, ruleMap, "IncludeRules", topLevelMap);

            // Validate ExcludeRules.
            ValidateRuleNameArray(parsed.ExcludeRules, ruleMap, "ExcludeRules", topLevelMap);

            // Validate Severity values.
            ValidateSeverityArray(parsed.Severities, topLevelMap);

            // Validate rule arguments.
            if (parsed.RuleArguments != null)
            {
                HashtableAst rulesHashtable = GetNestedHashtable(topLevelMap, "Rules");

                var rulesAstMap = rulesHashtable != null
                    ? BuildAstKeyMap(rulesHashtable)
                    : new Dictionary<string, Tuple<ExpressionAst, StatementAst>>(StringComparer.OrdinalIgnoreCase);

                foreach (var ruleEntry in parsed.RuleArguments)
                {
                    string ruleName = ruleEntry.Key;
                    IScriptExtent ruleKeyExtent = GetKeyExtent(rulesAstMap, ruleName)
                                                  ?? rulesHashtable?.Extent
                                                  ?? rootHashtable.Extent;

                    if (!ruleMap.TryGetValue(ruleName, out IRule rule))
                    {
                        AddDiagnostic(
                            string.Format(CultureInfo.CurrentCulture,
                                Strings.SettingsFileRuleArgRuleNotFound, ruleName),
                            ruleKeyExtent,
                            DiagnosticSeverity.Error);
                        continue;
                    }

                    if (!(rule is ConfigurableRule))
                    {
                        AddDiagnostic(
                            string.Format(CultureInfo.CurrentCulture,
                                Strings.SettingsFileRuleNotConfigurable, ruleName),
                            ruleKeyExtent,
                            DiagnosticSeverity.Error);
                        continue;
                    }

                    var optionInfos = RuleOptionInfo.GetRuleOptions(rule);
                    var optionMap = new Dictionary<string, RuleOptionInfo>(StringComparer.OrdinalIgnoreCase);
                    foreach (var opt in optionInfos)
                    {
                        optionMap[opt.Name] = opt;
                    }

                    // Get the AST for this rule's nested hashtable.
                    HashtableAst ruleHashtable = GetNestedHashtable(rulesAstMap, ruleName);
                    var ruleArgAstMap = ruleHashtable != null
                        ? BuildAstKeyMap(ruleHashtable)
                        : new Dictionary<string, Tuple<ExpressionAst, StatementAst>>(StringComparer.OrdinalIgnoreCase);

                    foreach (var arg in ruleEntry.Value)
                    {
                        string argName = arg.Key;
                        IScriptExtent argKeyExtent = GetKeyExtent(ruleArgAstMap, argName)
                                                     ?? ruleKeyExtent;

                        if (!optionMap.TryGetValue(argName, out RuleOptionInfo optionInfo))
                        {
                            AddDiagnostic(
                                string.Format(CultureInfo.CurrentCulture,
                                    Strings.SettingsFileUnrecognisedOption, ruleName, argName),
                                argKeyExtent,
                                DiagnosticSeverity.Error);
                            continue;
                        }

                        // Validate that the value is compatible with the expected type.
                        if (arg.Value != null && !IsValueCompatible(arg.Value, optionInfo.OptionType))
                        {
                            IScriptExtent valueExtent = GetValueExtent(ruleArgAstMap, argName)
                                                        ?? argKeyExtent;

                            AddDiagnostic(
                                string.Format(CultureInfo.CurrentCulture,
                                    Strings.SettingsFileInvalidOptionType,
                                    ruleName, argName, GetFriendlyTypeName(optionInfo.OptionType)),
                                valueExtent,
                                DiagnosticSeverity.Error);
                        }
                        // Validate constrained string values against the set of possible values.
                        else if (optionInfo.PossibleValues != null
                            && optionInfo.PossibleValues.Length > 0
                            && arg.Value is string strValue)
                        {
                            bool valueValid = optionInfo.PossibleValues.Any(pv =>
                                string.Equals(pv.ToString(), strValue, StringComparison.OrdinalIgnoreCase));

                            if (!valueValid)
                            {
                                IScriptExtent valueExtent = GetValueExtent(ruleArgAstMap, argName)
                                                            ?? argKeyExtent;

                                AddDiagnostic(
                                    string.Format(CultureInfo.CurrentCulture,
                                        Strings.SettingsFileInvalidOptionValue,
                                        ruleName, argName, strValue,
                                        string.Join(", ", optionInfo.PossibleValues.Select(v => v.ToString()))),
                                    valueExtent,
                                    DiagnosticSeverity.Error);
                            }
                        }
                    }
                }
            }

            if (Quiet)
            {
                WriteObject(_diagnostics.Count == 0);
            }
            else
            {
                EmitDiagnostics();
            }
        }

        #endregion Overrides

        #region Diagnostics

        /// <summary>
        /// Records a DiagnosticRecord for later emission.
        /// </summary>
        private void AddDiagnostic(string message, IScriptExtent extent, DiagnosticSeverity severity)
        {
            _diagnostics.Add(new DiagnosticRecord(
                message,
                extent,
                RuleName,
                severity,
                _resolvedPath));
        }

        /// <summary>
        /// Writes all collected DiagnosticRecord objects to the output pipeline.
        /// </summary>
        private void EmitDiagnostics()
        {
            foreach (var diag in _diagnostics)
            {
                WriteObject(diag);
            }
        }

        #endregion Diagnostics

        #region AST helpers

        /// <summary>
        /// Builds a case-insensitive dictionary mapping key names to their
        /// (key-expression, value-statement) tuples in a HashtableAst.
        /// </summary>
        private static Dictionary<string, Tuple<ExpressionAst, StatementAst>> BuildAstKeyMap(HashtableAst hashtableAst)
        {
            var map = new Dictionary<string, Tuple<ExpressionAst, StatementAst>>(StringComparer.OrdinalIgnoreCase);
            if (hashtableAst?.KeyValuePairs == null)
            {
                return map;
            }

            foreach (var pair in hashtableAst.KeyValuePairs)
            {
                if (pair.Item1 is StringConstantExpressionAst keyAst)
                {
                    map[keyAst.Value] = pair;
                }
            }

            return map;
        }

        /// <summary>
        /// Returns the IScriptExtent of a key expression in an AST key map,
        /// or null if the key is not found.
        /// </summary>
        private static IScriptExtent GetKeyExtent(
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> astMap,
            string keyName)
        {
            if (astMap.TryGetValue(keyName, out var pair))
            {
                return pair.Item1.Extent;
            }

            return null;
        }

        /// <summary>
        /// Returns the IScriptExtent of a value expression in an AST key map,
        /// or null if the key is not found.
        /// </summary>
        private static IScriptExtent GetValueExtent(
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> astMap,
            string keyName)
        {
            if (astMap.TryGetValue(keyName, out var pair))
            {
                ExpressionAst valueExpr = (pair.Item2 as PipelineAst)?.GetPureExpression();
                if (valueExpr != null)
                {
                    return valueExpr.Extent;
                }

                return pair.Item2.Extent;
            }

            return null;
        }

        /// <summary>
        /// Returns the HashtableAst for a nested hashtable value, or null.
        /// </summary>
        private static HashtableAst GetNestedHashtable(
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> astMap,
            string keyName)
        {
            if (astMap.TryGetValue(keyName, out var pair))
            {
                ExpressionAst valueExpr = (pair.Item2 as PipelineAst)?.GetPureExpression();
                return valueExpr as HashtableAst;
            }

            return null;
        }

        /// <summary>
        /// Returns the IScriptExtent of a specific string element within an
        /// array value in the AST, matching by string value. Falls back to
        /// the array extent or key extent if not found.
        /// </summary>
        private static IScriptExtent FindArrayElementExtent(
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> astMap,
            string keyName,
            string elementValue)
        {
            if (!astMap.TryGetValue(keyName, out var pair))
            {
                return null;
            }

            ExpressionAst valueExpr = (pair.Item2 as PipelineAst)?.GetPureExpression();
            if (valueExpr == null)
            {
                return pair.Item2.Extent;
            }

            // Look for the string element in array expressions.
            IEnumerable<Ast> stringNodes = valueExpr.FindAll(
                ast => ast is StringConstantExpressionAst strAst
                    && string.Equals(strAst.Value, elementValue, StringComparison.OrdinalIgnoreCase),
                searchNestedScriptBlocks: false);

            Ast match = stringNodes.FirstOrDefault();
            return match?.Extent ?? valueExpr.Extent;
        }

        #endregion AST helpers

        #region Validation helpers

        /// <summary>
        /// Validates that rule names in an array field exist in the known rule set.
        /// Wildcard entries are skipped.
        /// </summary>
        private void ValidateRuleNameArray(
            IEnumerable<string> ruleNames,
            Dictionary<string, IRule> ruleMap,
            string fieldName,
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> topLevelMap)
        {
            if (ruleNames == null)
            {
                return;
            }

            foreach (string name in ruleNames)
            {
                if (WildcardPattern.ContainsWildcardCharacters(name))
                {
                    continue;
                }

                if (!ruleMap.ContainsKey(name))
                {
                    IScriptExtent extent = FindArrayElementExtent(topLevelMap, fieldName, name)
                                           ?? GetKeyExtent(topLevelMap, fieldName);

                    AddDiagnostic(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileRuleNotFound, fieldName, name),
                        extent,
                        DiagnosticSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Validates severity values against the RuleSeverity enum.
        /// </summary>
        private void ValidateSeverityArray(
            IEnumerable<string> severities,
            Dictionary<string, Tuple<ExpressionAst, StatementAst>> topLevelMap)
        {
            if (severities == null)
            {
                return;
            }

            foreach (string sev in severities)
            {
                if (!Enum.TryParse<RuleSeverity>(sev, ignoreCase: true, out _))
                {
                    IScriptExtent extent = FindArrayElementExtent(topLevelMap, "Severity", sev)
                                           ?? GetKeyExtent(topLevelMap, "Severity");

                    AddDiagnostic(
                        string.Format(CultureInfo.CurrentCulture,
                            Strings.SettingsFileInvalidSeverity,
                            sev,
                            string.Join(", ", Enum.GetNames(typeof(RuleSeverity)))),
                        extent,
                        DiagnosticSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Checks whether a value from the settings file is compatible with the
        /// target CLR property type.
        /// </summary>
        private static bool IsValueCompatible(object value, Type targetType)
        {
            if (value == null)
            {
                return !targetType.IsValueType;
            }

            Type valueType = value.GetType();

            // Direct assignment.
            if (targetType.IsAssignableFrom(valueType))
            {
                return true;
            }

            // Bool property — only accept bool.
            if (targetType == typeof(bool))
            {
                return value is bool;
            }

            // Int property — accept int, long within range, or a string that parses as int.
            if (targetType == typeof(int))
            {
                if (value is int)
                {
                    return true;
                }

                if (value is long l)
                {
                    return l >= int.MinValue && l <= int.MaxValue;
                }

                return value is string s && int.TryParse(s, out _);
            }

            // String property — almost anything is acceptable since ToString works.
            if (targetType == typeof(string))
            {
                return true;
            }

            // Array property — accept arrays or a single element of the right kind.
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();

                if (valueType.IsArray)
                {
                    // Check that each element is compatible.
                    foreach (object item in (Array)value)
                    {
                        if (!IsValueCompatible(item, elementType))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                // A single value can be wrapped into a one-element array.
                return IsValueCompatible(value, elementType);
            }

            return false;
        }

        /// <summary>
        /// Returns a user-friendly name for a CLR type for use in error messages.
        /// </summary>
        private static string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(string[])) return "string[]";
            if (type == typeof(int[])) return "int[]";
            return type.Name;
        }

        #endregion Validation helpers
    }
}
