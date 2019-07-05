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

            this.includeRules = new List<string>();
            this.excludeRules = new List<string>();
            this.severities = new List<string>();
            this.ruleArguments = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            // If `settings` is a string, then preprocess it by (1) resolving it to a file path, and (2) parsing the file to a Hashtable.
            var settingsFilePath = settings as string;
            if (settingsFilePath != null)
            {
                if (presetResolver != null)
                {
                    var resolvedFilePath = presetResolver(settingsFilePath);
                    if (resolvedFilePath != null)
                    {
                        settingsFilePath = resolvedFilePath;
                    }
                    // Do not throw an exception if `presetResolver` fails to resolve `settingsFilePath`. Rather, attempt to handle the issue
                    // ourselves by acting simply as if no `presetResolver` was passed in the first place.
                }
                // Do not throw an exception if the `presetResolver` argument is null. This is because it is permitted for a file path `settings` to
                // not have any associated `presetResolver`.

                if (File.Exists(settingsFilePath))
                {
                    this.filePath = settingsFilePath;

                    // TODO Refactor the `ParseSettingsFile(string) => Settings` method to `ParseSettingsFiles(string) => Hashtable`, and then remove
                    // the `return` statement in order to proceed to the call to `ParseSettingsHashtable(Hashtable) => Settings` on the result.
                    ParseSettingsFile(settingsFilePath);
                    return;
                }

                throw new ArgumentException(String.Format(
                    Strings.InvalidPath,
                    settingsFilePath));
            }

            // Do the real work of parsing the `settings` Hashtable (whether passed directly or first parsed from a resolved file path).
            var settingsHashtable = settings as Hashtable;
            if (settingsHashtable != null)
            {
                ParseSettingsHashtable(settingsHashtable);
                return;
            }

            // The `settings` argument must be either a string or a Hashtable.
            throw new ArgumentException(Strings.SettingsInvalidType);
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

        private List<string> ParseSettingValueStringOrStrings(object value, string settingName, IList<Exception> exceptions)
        {
            if (value == null)
            {
                exceptions.Add(new InvalidDataException(string.Format(
                    Strings.SettingValueIsNull,
                    settingName)));
                return null;
            }

            if (value is string)
            {
                value = new[] { value };
            }

            if (!(value is ICollection))
            {
                exceptions.Add(new InvalidDataException(string.Format(
                    Strings.SettingValueIsNotStringOrStringsType,
                    settingName)));
                return null;
            }
            var values = value as ICollection;

            var strings = new List<string>(values.Count);
            int elementIndex = 0;
            int currentElementIndex = elementIndex;
            foreach (var element in values)
            {
                currentElementIndex = elementIndex++;

                if (element is null)
                {
                    exceptions.Add(new InvalidDataException(string.Format(
                        Strings.SettingValueElementIsNull,
                        settingName,
                        currentElementIndex)));
                    continue;
                }

                if (!(element is string))
                {
                    exceptions.Add(new InvalidDataException(string.Format(
                        Strings.SettingValueElementIsNotStringType,
                        settingName,
                        currentElementIndex,
                        element)));
                    continue;
                }

                strings.Add(element as string);
            }

            return strings;
        }

        private bool? ParseSettingValueBoolean(object value, string settingName, IList<Exception> exceptions)
        {
            if (value == null)
            {
                exceptions.Add(new InvalidDataException(string.Format(
                    Strings.SettingValueIsNull,
                    settingName)));
                return null;
            }

            if (!(value is bool))
            {
                exceptions.Add(new InvalidDataException(string.Format(
                    Strings.SettingValueIsNotBooleanType,
                    settingName,
                    value)));
                return null;
            }

            return (bool) value;
        }

        private void ParseSettingsHashtable(Hashtable settings)
        {
            IList<Exception> exceptions = new List<Exception>();

            ISet<string> uniqueSettingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry setting in settings)
            {
                if (setting.Key is null)
                {
                    exceptions.Add(new InvalidDataException(
                        Strings.SettingKeyIsNull));
                    continue;
                }

                if (!(setting.Key is string))
                {
                    exceptions.Add(new InvalidDataException(string.Format(
                        Strings.SettingKeyIsNotStringType,
                        setting.Key)));
                    continue;
                }
                string settingName = setting.Key as string;

                if (!uniqueSettingKeys.Add(settingName))
                {
                    // setting.Key should be used instead of settingName because the former preserves information about the source casing.
                    exceptions.Add(new InvalidDataException(string.Format(
                        Strings.SettingKeyIsNotUniqueIgnoringCase,
                        setting.Key)));
                    continue;
                }

                if (setting.Value is null)
                {
                    exceptions.Add(new InvalidDataException(string.Format(
                        Strings.SettingValueIsNull,
                        settingName)));
                    continue;
                }

                // ToLowerInvariant is important to also work with turkish culture, see https://github.com/PowerShell/PSScriptAnalyzer/issues/1095
                switch (settingName.ToLowerInvariant())
                {
                    case "severity":
                        var maybeSeverity = ParseSettingValueStringOrStrings(setting.Value, settingName, exceptions);
                        if (maybeSeverity is null)
                        {
                            continue;
                        }

                        this.severities = maybeSeverity;
                        break;

                    case "includerules":
                        var maybeIncludeRules = ParseSettingValueStringOrStrings(setting.Value, settingName, exceptions);
                        if (maybeIncludeRules is null)
                        {
                            continue;
                        }

                        this.includeRules = maybeIncludeRules;
                        break;

                    case "excluderules":
                        var maybeExcludeRules = ParseSettingValueStringOrStrings(setting.Value, settingName, exceptions);
                        if (maybeExcludeRules is null)
                        {
                            continue;
                        }

                        this.excludeRules = maybeExcludeRules;
                        break;

                    case "customrulepath":
                        var maybeCustomRulePath = ParseSettingValueStringOrStrings(setting.Value, settingName, exceptions);
                        if (maybeCustomRulePath is null)
                        {
                            continue;
                        }

                        this.customRulePath = maybeCustomRulePath;
                        break;

                    case "includedefaultrules":
                        bool? maybeIncludeDefaultRules = ParseSettingValueBoolean(setting.Value, settingName, exceptions);
                        if (maybeIncludeDefaultRules is null)
                        {
                            continue;
                        }

                        this.includeDefaultRules = (bool) maybeIncludeDefaultRules;
                        break;

                    case "recursecustomrulepath":
                        bool? maybeRecurseCustomRulePath = ParseSettingValueBoolean(setting.Value, settingName, exceptions);
                        if (maybeRecurseCustomRulePath is null)
                        {
                            continue;
                        }

                        this.recurseCustomRulePath = (bool) maybeRecurseCustomRulePath;
                        break;

                    case "rules":
                        if (!(setting.Value is System.Collections.IDictionary))
                        {
                            exceptions.Add(new InvalidDataException(string.Format(
                                Strings.SettingRulesValueIsNotDictionaryType,
                                setting.Value)));
                            continue;
                        }
                        Hashtable rules = setting.Value as Hashtable;

                        var parsedRules = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
                        ISet<string> uniqueRuleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (DictionaryEntry rule in rules)
                        {
                            if (rule.Key is null)
                            {
                                exceptions.Add(new InvalidDataException(
                                    Strings.SettingRuleKeyIsNull));
                                continue;
                            }

                            if (!(rule.Key is string))
                            {
                                exceptions.Add(new InvalidDataException(string.Format(
                                    Strings.SettingRuleKeyIsNotStringType,
                                    rule.Key)));
                                continue;
                            }
                            string ruleName = rule.Key as string;

                            if (!uniqueRuleKeys.Add(ruleName))
                            {
                                // rule.Key should be used instead of ruleName because the former preserves information about the source casing.
                                exceptions.Add(new InvalidDataException(string.Format(
                                    Strings.SettingRuleKeyIsNotUniqueIgnoringCase,
                                    rule.Key)));
                                // Do not `continue` because even if an element's key is non-unique, that element's value may still be checked.
                            }

                            if (rule.Value is null)
                            {
                                exceptions.Add(new InvalidDataException(string.Format(
                                    Strings.SettingRuleValueIsNull,
                                    ruleName)));
                                continue;
                            }

                            if (!(rule.Value is System.Collections.IDictionary))
                            {
                                exceptions.Add(new InvalidDataException(string.Format(
                                    Strings.SettingRuleValueIsNotDictionaryType,
                                    ruleName,
                                    rule.Value)));
                                continue;
                            }
                            Hashtable arguments = rule.Value as Hashtable;

                            var parsedArguments = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            ISet<string> uniqueArgumentKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            foreach (DictionaryEntry argument in arguments)
                            {
                                if (argument.Key is null)
                                {
                                    exceptions.Add(new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNull,
                                        ruleName)));
                                    continue;
                                }

                                if (!(argument.Key is string))
                                {
                                    exceptions.Add(new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNotStringType,
                                        ruleName,
                                        argument.Key)));
                                    continue;
                                }
                                string argumentName = argument.Key as string;

                                if (!uniqueArgumentKeys.Add(argumentName))
                                {
                                    // argument.Key should be used instead of argumentName because the former preserves information about the source casing.
                                    exceptions.Add(new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentKeyIsNotUniqueIgnoringCase,
                                        ruleName,
                                        argument.Key)));
                                    continue;
                                }

                                if (argument.Value is null)
                                {
                                    exceptions.Add(new InvalidDataException(string.Format(
                                        Strings.SettingRuleArgumentValueIsNull,
                                        ruleName,
                                        argumentName)));
                                    continue;
                                }

                                parsedArguments[argumentName] = argument.Value;
                            }

                            parsedRules[ruleName] = parsedArguments;
                        }

                        this.ruleArguments = parsedRules;
                        break;

                    default:
                        exceptions.Add(new InvalidDataException(string.Format(
                            Strings.WrongKeyHashTable,
                            settingName)));
                        continue;
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        private void ParseSettingsFile(string settingsFilePath)
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

            ParseSettingsHashtable(hashtable);
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
