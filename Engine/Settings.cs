// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal enum SettingsMode { None = 0, Auto, File, Hashtable, Preset };

    /// <summary>
    /// A class to represent the settings provided to ScriptAnalyzer class.
    /// </summary>
    public class Settings
    {
        private bool recurseCustomRulePath = false;
        private bool includeDefaultRules = false;
        private string filePath;
        private List<string> includeRules;
        private List<string> excludeRules;
        private List<string> severities;
        private List<string> customRulePath;
        private Dictionary<string, Dictionary<string, object>> ruleArguments;

        public bool RecurseCustomRulePath => recurseCustomRulePath;
        public bool IncludeDefaultRules => includeDefaultRules;
        public string FilePath => filePath;
        public IEnumerable<string> IncludeRules => includeRules;
        public IEnumerable<string> ExcludeRules => excludeRules;
        public IEnumerable<string> Severities => severities;
        public IEnumerable<string> CustomRulePath => customRulePath;
        public Dictionary<string, Dictionary<string, object>> RuleArguments => ruleArguments;

        /// <summary>
        /// Create a settings object from the input object.
        /// </summary>
        /// <param name="settings">An input object of type Hashtable or string.</param>
        /// <param name="presetResolver">A function that takes in a preset and resolves it to a path.</param>
        public Settings(object settings, Func<string, string> presetResolver)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            includeRules = new List<string>();
            excludeRules = new List<string>();
            severities = new List<string>();
            ruleArguments = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            var settingsFilePath = settings as string;

            //it can either be a preset or path to a file or a hashtable
            if (settingsFilePath != null)
            {
                if (presetResolver != null)
                {
                    var resolvedFilePath = presetResolver(settingsFilePath);
                    if (resolvedFilePath != null)
                    {
                        settingsFilePath = resolvedFilePath;
                    }
                }

                if (File.Exists(settingsFilePath))
                {
                    filePath = settingsFilePath;
                    parseSettingsFile(settingsFilePath);
                }
                else
                {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.InvalidPath,
                            settingsFilePath));
                }
            }
            else
            {
                var settingsHashtable = settings as Hashtable;
                if (settingsHashtable != null)
                {
                    parseSettingsHashtable(settingsHashtable);
                }
                else
                {
                    throw new ArgumentException(Strings.SettingsInvalidType);
                }
            }
        }

        /// <summary>
        /// Create a Settings object from the input object.
        /// </summary>
        /// <param name="settings">An input object of type Hashtable or string.</param>
        public Settings(object settings) : this(settings, null)
        {
        }

        /// <summary>
        /// Retrieves the Settings directory from the Module directory structure
        /// </summary>
        public static string GetShippedSettingsDirectory()
        {
            // Find the compatibility files in Settings folder
            var path = typeof(Helper).GetTypeInfo().Assembly.Location;
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

        /// <summary>
        /// Returns the builtin setting presets
        ///
        /// Looks for powershell data files (*.psd1) in the PSScriptAnalyzer module settings directory
        /// and returns the names of the files without extension
        /// </summary>
        public static IEnumerable<string> GetSettingPresets()
        {
            var settingsPath = GetShippedSettingsDirectory();
            if (settingsPath != null)
            {
                foreach (var filepath in System.IO.Directory.EnumerateFiles(settingsPath, "*.psd1"))
                {
                    yield return System.IO.Path.GetFileNameWithoutExtension(filepath);
                }
            }
        }

        /// <summary>
        /// Gets the path to the settings file corresponding to the given preset.
        ///
        /// If the corresponding preset file is not found, the method returns null.
        /// </summary>
        public static string GetSettingPresetFilePath(string settingPreset)
        {
            var settingsPath = GetShippedSettingsDirectory();
            if (settingsPath != null)
            {
                if (GetSettingPresets().Contains(settingPreset, StringComparer.OrdinalIgnoreCase))
                {
                    return System.IO.Path.Combine(settingsPath, settingPreset + ".psd1");
                }
            }

            return null;
        }

        /// <summary>
        /// Create a settings object from an input object.
        /// </summary>
        /// <param name="settingsObj">An input object of type Hashtable or string.</param>
        /// <param name="cwd">The path in which to search for a settings file.</param>
        /// <param name="outputWriter">An output writer.</param>
        /// <param name="getResolvedProviderPathFromPSPathDelegate">The GetResolvedProviderPathFromPSPath method from PSCmdlet to resolve relative path including wildcard support.</param>
        /// <returns>An object of Settings type.</returns>
        internal static Settings Create(object settingsObj, string cwd, IOutputWriter outputWriter,
            PathResolver.GetResolvedProviderPathFromPSPath<string, ProviderInfo, Collection<string>> getResolvedProviderPathFromPSPathDelegate)
        {
            object settingsFound;
            var settingsMode = FindSettingsMode(settingsObj, cwd, out settingsFound);

            switch (settingsMode)
            {
                case SettingsMode.Auto:
                    outputWriter?.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsNotProvided,
                            ""));
                    outputWriter?.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsAutoDiscovered,
                            (string)settingsFound));
                    break;

                case SettingsMode.Preset:
                case SettingsMode.File:
                    var userProvidedSettingsString = settingsFound.ToString();
                    try
                    {
                        var resolvedPath = getResolvedProviderPathFromPSPathDelegate(userProvidedSettingsString, out ProviderInfo providerInfo).Single();
                        settingsFound = resolvedPath;
                        outputWriter?.WriteVerbose(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                Strings.SettingsUsingFile,
                                resolvedPath));
                    }
                    catch
                    {
                        outputWriter?.WriteVerbose(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                Strings.SettingsCannotFindFile,
                                userProvidedSettingsString));
                    }
                    break;

                case SettingsMode.Hashtable:
                    outputWriter?.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsUsingHashtable));
                    break;

                default:
                    outputWriter?.WriteVerbose(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Strings.SettingsObjectCouldNotBResolved));
                    return null;
            }

            return new Settings(settingsFound);
        }

        private bool IsStringOrStringArray(object val)
        {
            if (val is string)
            {
                return true;
            }

            var valArr = val as object[];
            return val == null ? false : valArr.All(x => x is string);
        }

        // TODO Clean up method GetData(object, string).
        private List<string> ParseSettingValueAsStrings(object value, string settingName)
        {
            // value must be either string or or an array of strings
            if (value == null)
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.WrongValueHashTable,
                        "",
                        settingName));
            }

            List<string> values = new List<string>();
            var valueStr = value as string;
            if (valueStr != null)
            {
                values.Add(valueStr);
            }
            else
            {
                var valueArr = value as object[];
                if (valueArr == null)
                {
                    // check if it is an array of strings
                    valueArr = value as string[];
                }

                if (valueArr != null)
                {
                    foreach (var item in valueArr)
                    {
                        var itemStr = item as string;
                        if (itemStr != null)
                        {
                            values.Add(itemStr);
                        }
                        else
                        {
                            throw new InvalidDataException(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.WrongValueHashTable,
                                    value,
                                    settingName));
                        }
                    }
                }
                else
                {
                    throw new InvalidDataException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.WrongValueHashTable,
                                value,
                                settingName));
                }
            }

            return values;
        }

        private void parseSettingsHashtable(Hashtable settings)
        {
            ISet<string> uniqueSettingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry setting in settings)
            {
                if (setting.Key is null)
                {
                    throw new InvalidDataException(Strings.SettingKeyIsNull);
                }
                
                if (!(setting.Key is string))
                {
                    throw new InvalidDataException(string.Format(
                        Strings.SettingKeyIsNotStringType,
                        setting.Key));
                }
                string settingName = (setting.Key as string).ToLowerInvariant();  // ToLowerInvariant is important to also work with turkish culture, see https://github.com/PowerShell/PSScriptAnalyzer/issues/1095

                if (!uniqueSettingKeys.Add(settingName))
                {
                    // setting.Key should be used instead of settingName because the former preserves information about the source casing.
                    throw new InvalidDataException(string.Format(
                        Strings.SettingKeyIsNotUniqueIgnoringCase,
                        setting.Key));
                }

                if (setting.Value is null)
                {
                    throw new InvalidDataException(string.Format(
                        Strings.SettingValueIsNull,
                        settingName));
                }

                switch (settingName)
                {
                    // TODO Clean up "Severity" setting validating parsing.
                    case "severity":
                        this.severities = ParseSettingValueAsStrings(setting.Value, settingName);
                        break;

                    // TODO Clean up "IncludeRules" setting validating parsing.
                    case "includerules":
                        this.includeRules = ParseSettingValueAsStrings(setting.Value, settingName);
                        break;

                    // TODO Clean up "ExcludeRules" setting validating parsing.
                    case "excluderules":
                        this.excludeRules = ParseSettingValueAsStrings(setting.Value, settingName);
                        break;

                    // TODO Clean up "CustomRulePath" setting validating parsing.
                    case "customrulepath":
                        this.customRulePath = ParseSettingValueAsStrings(setting.Value, settingName);
                        break;

                    // TODO Clean up "IncludeDefaultRules" setting validating parsing.
                    case "includedefaultrules":

                    // TODO Clean up "RecurseCustomRulePath" setting validating parsing.
                    case "recursecustomrulepath":
                        if (!(setting.Value is bool))
                        {
                            throw new InvalidDataException(string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.SettingsValueTypeMustBeBool,
                                setting.Key));
                        }

                        var booleanVal = (bool)setting.Value;
                        var field = this.GetType().GetField(
                            settingName,
                            BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic);
                        field.SetValue(this, booleanVal);
                        break;

                    case "rules":
                        if (!(setting.Value is System.Collections.IDictionary))
                        {
                            throw new InvalidDataException(string.Format(
                                Strings.SettingRulesValueIsNotDictionaryType,
                                setting.Value));
                        }
                        Hashtable rules = setting.Value as Hashtable;

                        var parsedRules = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                        ISet<string> uniqueRuleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (DictionaryEntry rule in rules)
                        {
                            if (rule.Key is null)
                            {
                                throw new InvalidDataException(Strings.SettingRuleKeyIsNull);
                            }

                            if (!(rule.Key is string))
                            {
                                throw new InvalidDataException(string.Format(
                                    Strings.SettingRuleKeyIsNotStringType,
                                    rule.Key));
                            }
                            string ruleName = (rule.Key as string).ToLowerInvariant();

                            if (!uniqueRuleKeys.Add(ruleName))
                            {
                                // rule.Key should be used instead of ruleName because the former preserves information about the source casing.
                                throw new InvalidDataException(string.Format(
                                    Strings.SettingRuleKeyIsNotUniqueIgnoringCase,
                                    rule.Key));
                            }

                            if (rule.Value is null)
                            {
                                throw new InvalidDataException(string.Format(
                                    Strings.SettingRuleValueIsNull,
                                    ruleName));
                            }

                            if (!(rule.Value is System.Collections.IDictionary))
                            {
                                throw new InvalidDataException(string.Format(
                                    Strings.SettingRuleValueIsNotDictionaryType,
                                    ruleName,
                                    rule.Value));
                            }
                            Hashtable arguments = rule.Value as Hashtable;

                            var parsedArguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            ISet<string> uniqueArgumentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            foreach (DictionaryEntry argument in arguments)
                            {
                                if (argument.Key is null)
                                {
                                    throw new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNull,
                                        ruleName));
                                }

                                if (!(argument.Key is string))
                                {
                                    throw new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNotStringType,
                                        ruleName,
                                        argument.Key));
                                }
                                string argumentName = (argument.Key as string).ToLowerInvariant();
                                
                                if (!uniqueArgumentKeys.Add(argumentName))
                                {
                                    // argument.Key should be used instead of argumentName because the former preserves information about the source casing.
                                    throw new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNotUniqueIgnoringCase,
                                        ruleName,
                                        argument.Key));
                                }
                                
                                if (argument.Value is null)
                                {
                                    throw new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentValueIsNull,
                                        ruleName,
                                        argumentName));
                                }

                                parsedArguments[argumentName] = argument.Value;
                            }

                            parsedRules[ruleName] = parsedArguments;
                        }

                        this.ruleArguments = parsedRules;
                        break;

                    // TODO Clean up default setting validating parsing.
                    default:
                        throw new InvalidDataException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.WrongKeyHashTable,
                                settingName));
                }
            }
        }

        private void parseSettingsFile(string settingsFilePath)
        {
            Token[] parserTokens = null;
            ParseError[] parserErrors = null;
            Ast profileAst = Parser.ParseFile(settingsFilePath, out parserTokens, out parserErrors);
            IEnumerable<Ast> hashTableAsts = profileAst.FindAll(item => item is HashtableAst, false);

            // no hashtable, raise warning
            if (hashTableAsts.Count() == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.InvalidProfile, settingsFilePath));
            }

            HashtableAst hashTableAst = hashTableAsts.First() as HashtableAst;
            Hashtable hashtable;
            try
            {
                // ideally we should use HashtableAst.SafeGetValue() but since
                // it is not available on PSv3, we resort to our own narrow implementation.
                hashtable = GetSafeValueFromHashtableAst(hashTableAst);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(Strings.InvalidProfile, e);
            }

            if (hashtable == null)
            {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Strings.InvalidProfile,
                        settingsFilePath));
            }

            parseSettingsHashtable(hashtable);
        }

        /// <summary>
        /// Evaluates all statically evaluable, side-effect-free expressions under an
        /// expression AST to return a value.
        /// Throws if an expression cannot be safely evaluated.
        /// Attempts to replicate the GetSafeValue() method on PowerShell AST methods from PSv5.
        /// </summary>
        /// <param name="exprAst">The expression AST to try to evaluate.</param>
        /// <returns>The .NET value represented by the PowerShell expression.</returns>
        private static object GetSafeValueFromExpressionAst(ExpressionAst exprAst)
        {
            switch (exprAst)
            {
                case ConstantExpressionAst constExprAst:
                    // Note, this parses top-level command invocations as bareword strings
                    // However, forbidding this causes hashtable parsing to fail
                    // It is probably not worth the complexity to isolate this case
                    return constExprAst.Value;

                case VariableExpressionAst varExprAst:
                    // $true and $false are VariableExpressionAsts, so look for them here
                    switch (varExprAst.VariablePath.UserPath.ToLowerInvariant())
                    {
                        case "true":
                            return true;

                        case "false":
                            return false;

                        case "null":
                            return null;

                        default:
                            throw CreateInvalidDataExceptionFromAst(varExprAst);
                    }

                case ArrayExpressionAst arrExprAst:

                    // Most cases are handled by the inner array handling,
                    // but we may have an empty array
                    if (arrExprAst.SubExpression?.Statements == null)
                    {
                        throw CreateInvalidDataExceptionFromAst(arrExprAst);
                    }

                    if (arrExprAst.SubExpression.Statements.Count == 0)
                    {
                        return new object[0];
                    }

                    var listComponents = new List<object>();
                    // Arrays can either be array expressions (1, 2, 3) or array literals with statements @(1 `n 2 `n 3)
                    // Or they can be a combination of these
                    // We go through each statement (line) in an array and read the whole subarray
                    // This will also mean that @(1; 2) is parsed as an array of two elements, but there's not much point defending against this
                    foreach (StatementAst statement in arrExprAst.SubExpression.Statements)
                    {
                        if (!(statement is PipelineAst pipelineAst))
                        {
                            throw CreateInvalidDataExceptionFromAst(arrExprAst);
                        }

                        ExpressionAst pipelineExpressionAst = pipelineAst.GetPureExpression();
                        if (pipelineExpressionAst == null)
                        {
                            throw CreateInvalidDataExceptionFromAst(arrExprAst);
                        }

                        object arrayValue = GetSafeValueFromExpressionAst(pipelineExpressionAst);
                        // We might hit arrays like @(\n1,2,3\n4,5,6), which the parser sees as two statements containing array expressions
                        if (arrayValue is object[] subArray)
                        {
                            listComponents.AddRange(subArray);
                            continue;
                        }

                        listComponents.Add(arrayValue);
                    }
                    return listComponents.ToArray();


                case ArrayLiteralAst arrLiteralAst:
                    return GetSafeValuesFromArrayAst(arrLiteralAst);

                case HashtableAst hashtableAst:
                    return GetSafeValueFromHashtableAst(hashtableAst);

                default:
                    // Other expression types are too complicated or fundamentally unsafe
                    throw CreateInvalidDataExceptionFromAst(exprAst);
            }
        }

        /// <summary>
        /// Process a PowerShell array literal with statically evaluable/safe contents
        /// into a .NET value.
        /// </summary>
        /// <param name="arrLiteralAst">The PowerShell array AST to turn into a value.</param>
        /// <returns>The .NET value represented by PowerShell syntax.</returns>
        private static object[] GetSafeValuesFromArrayAst(ArrayLiteralAst arrLiteralAst)
        {
            if (arrLiteralAst == null)
            {
                throw new ArgumentNullException(nameof(arrLiteralAst));
            }

            if (arrLiteralAst.Elements == null)
            {
                throw CreateInvalidDataExceptionFromAst(arrLiteralAst);
            }

            var elements = new List<object>();
            foreach (ExpressionAst exprAst in arrLiteralAst.Elements)
            {
                elements.Add(GetSafeValueFromExpressionAst(exprAst));
            }

            return elements.ToArray();
        }

        /// <summary>
        /// Create a hashtable value from a PowerShell AST representing one,
        /// provided that the PowerShell expression is statically evaluable and safe.
        /// </summary>
        /// <param name="hashtableAst">The PowerShell representation of the hashtable value.</param>
        /// <returns>The Hashtable as a hydrated .NET value.</returns>
        private static Hashtable GetSafeValueFromHashtableAst(HashtableAst hashtableAst)
        {
            if (hashtableAst == null)
            {
                throw new ArgumentNullException(nameof(hashtableAst));
            }

            if (hashtableAst.KeyValuePairs == null)
            {
                throw CreateInvalidDataExceptionFromAst(hashtableAst);
            }

            var hashtable = new Hashtable();
            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                // Get the key
                object key = GetSafeValueFromExpressionAst(entry.Item1);
                if (key == null)
                {
                    throw CreateInvalidDataExceptionFromAst(entry.Item1);
                }

                // Get the value
                ExpressionAst valueExprAst = (entry.Item2 as PipelineAst)?.GetPureExpression();
                if (valueExprAst == null)
                {
                    throw CreateInvalidDataExceptionFromAst(entry.Item2);
                }

                // Add the key/value entry into the hydrated hashtable
                hashtable[key] = GetSafeValueFromExpressionAst(valueExprAst);
            }

            return hashtable;
        }

        private static InvalidDataException CreateInvalidDataExceptionFromAst(Ast ast)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            return CreateInvalidDataException(ast.Extent);
        }

        private static InvalidDataException CreateInvalidDataException(IScriptExtent extent)
        {
            return new InvalidDataException(string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.WrongValueFormat,
                                    extent.StartLineNumber,
                                    extent.StartColumnNumber,
                                    extent.File ?? ""));
        }

        private static bool IsBuiltinSettingPreset(object settingPreset)
        {
            var preset = settingPreset as string;
            if (preset != null)
            {
                return GetSettingPresets().Contains(preset, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        internal static SettingsMode FindSettingsMode(object settings, string path, out object settingsFound)
        {
            var settingsMode = SettingsMode.None;
            settingsFound = settings;
            if (settingsFound == null)
            {
                if (path != null)
                {
                    // add a directory separator character because if there is no trailing separator character, it will return the parent
                    var directory = path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    if (File.Exists(directory))
                    {
                        // if given path is a file, get its directory
                        directory = Path.GetDirectoryName(directory);
                    }

                    if (Directory.Exists(directory))
                    {
                        // if settings are not provided explicitly, look for it in the given path
                        // check if pssasettings.psd1 exists
                        var settingsFilename = "PSScriptAnalyzerSettings.psd1";
                        var settingsFilePath = Path.Combine(directory, settingsFilename);
                        settingsFound = settingsFilePath;
                        if (File.Exists(settingsFilePath))
                        {
                            settingsMode = SettingsMode.Auto;
                        }
                    }
                }
            }
            else
            {
                if (!TryResolveSettingForStringType(settingsFound, ref settingsMode, ref settingsFound))
                {
                    if (settingsFound is Hashtable)
                    {
                        settingsMode = SettingsMode.Hashtable;
                    }
                    // if the provided argument is wrapped in an expressions then PowerShell resolves it but it will be of type PSObject and we have to operate then on the BaseObject
                    else if (settingsFound is PSObject settingsFoundPSObject)
                    {
                        TryResolveSettingForStringType(settingsFoundPSObject.BaseObject, ref settingsMode, ref settingsFound);
                    }
                }
            }

            return settingsMode;
        }

        // If the settings object is a string determine wheter it is one of the settings preset or a file path and resolve the setting in the former case.
        private static bool TryResolveSettingForStringType(object settingsObject, ref SettingsMode settingsMode, ref object resolvedSettingValue)
        {
            if (settingsObject is string settingsString)
            {
                if (IsBuiltinSettingPreset(settingsString))
                {
                    settingsMode = SettingsMode.Preset;
                    resolvedSettingValue = GetSettingPresetFilePath(settingsString);
                }
                else
                {
                    settingsMode = SettingsMode.File;
                    resolvedSettingValue = settingsString;
                }
                return true;
            }

            return false;
        }
    }
}
