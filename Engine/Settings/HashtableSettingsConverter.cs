// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Converts an inline PowerShell hashtable into a strongly typed <see cref="SettingsData"/>.
    /// Overview of parsing logic:
    /// 1. Recursively flattens nested Hashtables into case-insensitive Dictionary<string, object>
    ///    while preserving inner dictionaries for rule arguments.
    /// 2. Iterates top-level keys, normalising to lowercase to match known setting names.
    /// 3. For list-valued keys (Severity, IncludeRules, etc.) coerces single string or enumerable
    ///    of strings into a List<string>.
    /// 4. For boolean flags (IncludeDefaultRules, RecurseCustomRulePath) enforces strict bool
    ///    types.
    /// 5. For Rules, validates a two-level case-insensitive dictionary-of-dictionaries (rule -> 
    ///    argument name/value).
    /// 6. Throws <see cref="InvalidDataException"/> on unknown keys or invalid value shapes to fail
    ///    fast and surface user errors clearly.
    /// </summary>
    internal static class HashtableSettingsConverter
    {

        /// <summary>
        /// Entry point: converts a user-supplied settings hashtable into a
        /// <see cref="SettingsData"/> instance.
        /// </summary>
        /// <param name="table">Inline settings hashtable.</param>
        /// <returns>Populated <see cref="SettingsData"/>.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown when a key is unknown or a value does not meet required type/shape constraints.
        /// </exception>
        public static SettingsData Convert(Hashtable table)
        {
            var includeRules = new List<string>();
            var excludeRules = new List<string>();
            var severities = new List<string>();
            var customRulePath = new List<string>();
            bool includeDefaultRules = false;
            bool recurseCustomRulePath = false;
            var ruleArgsOuter = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            var dict = ToDictionary(table);

            foreach (var kvp in dict)
            {
                var keyLower = kvp.Key.ToLowerInvariant();
                var val = kvp.Value;
                switch (keyLower)
                {
                    case "severity":
                        severities = CoerceStringList(val, kvp.Key);
                        break;
                    case "includerules":
                        includeRules = CoerceStringList(val, kvp.Key);
                        break;
                    case "excluderules":
                        excludeRules = CoerceStringList(val, kvp.Key);
                        break;
                    case "customrulepath":
                        customRulePath = CoerceStringList(val, kvp.Key);
                        break;
                    case "includedefaultrules":
                        includeDefaultRules = CoerceBool(val, kvp.Key);
                        break;
                    case "recursecustomrulepath":
                        recurseCustomRulePath = CoerceBool(val, kvp.Key);
                        break;
                    case "rules":
                        ruleArgsOuter = ConvertRuleArguments(val, kvp.Key);
                        break;
                    default:
                        throw new InvalidDataException($"Unknown settings key '{kvp.Key}'.");
                }
            }

            return new SettingsData
            {
                IncludeRules = includeRules,
                ExcludeRules = excludeRules,
                Severities = severities,
                CustomRulePath = customRulePath,
                IncludeDefaultRules = includeDefaultRules,
                RecurseCustomRulePath = recurseCustomRulePath,
                RuleArguments = ruleArgsOuter
            };
        }

        /// <summary>
        /// Recursively converts a Hashtable (and any nested Hashtables) to a case-insensitive
        /// Dictionary.
        /// Nested Hashtables become nested Dictionary<string, object> instances.
        /// </summary>
        /// <param name="table">Source hashtable.</param>
        /// <returns>Case-insensitive dictionary representation.</returns>
        /// <exception cref="InvalidDataException">If any key is not a string.</exception>
        private static Dictionary<string, object> ToDictionary(Hashtable table)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var keyObj in table.Keys)
            {
                if (keyObj is not string key)
                    throw new InvalidDataException("Settings keys must be strings.");
                var value = table[keyObj];
                if (value is Hashtable ht)
                {
                    dict[key] = ToDictionary(ht);
                }
                else
                {
                    dict[key] = value;
                }
            }
            return dict;
        }

        /// <summary>
        /// Coerces a value into a list of strings. Accepts a single string or an enumerable of
        /// strings.
        /// </summary>
        /// <param name="val">Value to coerce.</param>
        /// <param name="key">Original key name for error context.</param>
        /// <returns>List of strings.</returns>
        /// <exception cref="InvalidDataException">
        /// If value is neither string nor enumerable of strings.
        /// </exception>
        private static List<string> CoerceStringList(object val, string key)
        {
            if (val is string s) return new List<string> { s };
            if (val is IEnumerable enumerable)
            {
                var list = new List<string>();
                foreach (var item in enumerable)
                {
                    if (item is string si) list.Add(si);
                    else throw new InvalidDataException($"Non-string element in array for key '{key}'.");
                }
                return list;
            }
            throw new InvalidDataException($"Value for key '{key}' must be string or string array.");
        }

        /// <summary>
        /// Validates and returns a boolean settings value.
        /// </summary>
        /// <param name="val">Value to validate.</param>
        /// <param name="key">Key name for error messages.</param>
        /// <returns>Boolean value.</returns>
        /// <exception cref="InvalidDataException">If value is not a boolean.</exception>
        private static bool CoerceBool(object val, string key)
        {
            if (val is bool b) return b;
            throw new InvalidDataException($"Value for key '{key}' must be boolean.");
        }

        /// <summary>
        /// Converts the value of the Rules key into a two-level case-insensitive dictionary
        /// structure.
        /// Expects outer and each inner dictionary to be case-insensitive
        /// Dictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="val">Rules value object.</param>
        /// <param name="key">Original key name ("Rules") for error context.</param>
        /// <returns>Dictionary of rule name to its argument dictionary.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown if the outer/inner dictionaries are missing, not case-insensitive, or wrongly
        /// typed.
        /// </exception>
        private static Dictionary<string, Dictionary<string, object>> ConvertRuleArguments(object val, string key)
        {
            if (val is not Dictionary<string, object> outer || outer.Comparer != StringComparer.OrdinalIgnoreCase)
                throw new InvalidDataException($"Rules value must be a case-insensitive dictionary for key '{key}'.");

            var result = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            foreach (var ruleName in outer.Keys)
            {
                if (outer[ruleName] is not Dictionary<string, object> inner || inner.Comparer != StringComparer.OrdinalIgnoreCase)
                    throw new InvalidDataException($"Rule arguments for '{ruleName}' must be a case-insensitive dictionary.");
                result[ruleName] = new Dictionary<string, object>(inner, StringComparer.OrdinalIgnoreCase);
            }
            return result;
        }
    }
}