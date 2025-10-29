// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Parses PowerShell data files (.psd1) containing a top-level hashtable into
    /// <see cref="SettingsData"/>.
    /// Parsing steps:
    /// 1. Verify the source file exists (the PowerShell parser requires a path).
    /// 2. Parse the file into an AST using <see cref="Parser.ParseFile"/>.
    /// 3. Locate the first <see cref="HashtableAst"/> (expected to represent the settings).
    /// 4. Safely convert the hashtable AST into a <see cref="Hashtable"/> via
    ///    <see cref="Helper.GetSafeValueFromHashtableAst"/>.
    /// 5. Delegate normalization and validation to
    ///    <see cref="HashtableSettingsConverter.Convert"/>.
    /// Throws <see cref="InvalidDataException"/> for structural issues (missing hashtable, invalid
    /// values).
    /// </summary>
    internal sealed class Psd1SettingsParser : ISettingsParser
    {
        public string FormatName => "psd1";

        /// <summary>
        /// Determines whether the supplied path (or extension) is a .psd1 settings file.
        /// </summary>
        /// <param name="pathOrExtension">Full path or just an extension string.</param>
        /// <returns>True if the extension is .psd1 (case-insensitive).</returns>
        public bool CanParse(string pathOrExtension) =>
            string.Equals(Path.GetExtension(pathOrExtension), ".psd1", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Parses a .psd1 settings file into <see cref="SettingsData"/>.
        /// </summary>
        /// <param name="content">
        /// Stream for API symmetry; not directly consumed (PowerShell parser reads from file path).
        /// </param>
        /// <param name="sourcePath">Absolute or relative path to the .psd1 file.</param>
        /// <returns>Normalized <see cref="SettingsData"/> instance.</returns>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="InvalidDataException">
        /// If no top-level hashtable is found or conversion yields invalid data.
        /// </exception>
        public SettingsData Parse(Stream content, string sourcePath)
        {
            // Need file path for PowerShell Parser.ParseFile
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Settings file not found.", sourcePath);
            }

            Ast ast = Parser.ParseFile(sourcePath, out Token[] tokens, out ParseError[] errors);

            if (ast.FindAll(a => a is HashtableAst, false).FirstOrDefault() is not HashtableAst hashTableAst)
            {
                throw new InvalidDataException($"Settings file '{sourcePath}' does not contain a hashtable.");
            }

            Hashtable raw;
            try
            {
                raw = Helper.GetSafeValueFromHashtableAst(hashTableAst);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidDataException($"Invalid settings file '{sourcePath}'.", e);
            }
            if (raw == null)
            {
                throw new InvalidDataException($"Invalid settings file '{sourcePath}'.");
            }

            return HashtableSettingsConverter.Convert(raw);
        }

        /// <summary>
        /// Serializes a <see cref="SettingsData"/> instance into a formatted .psd1 settings file
        /// (PowerShell hashtable) similar to shipped presets.
        /// Omits empty collections and flags (if false) to keep output concise.
        /// </summary>
        /// <param name="settingsData">Settings to serialize.</param>
        /// <returns>Formatted .psd1 content as a string.</returns>
        public string Serialise(SettingsData settingsData)
        {
            if (settingsData == null) throw new ArgumentNullException(nameof(settingsData));

            var sb = new System.Text.StringBuilder();
            var indent = "    ";

            string Quote(string s) => "'" + s.Replace("'", "''") + "'";

            void AppendStringList(string key, List<string> list)
            {
                if (list == null || list.Count == 0) return;
                sb.Append(indent).Append(key).Append(" = @(").AppendLine();
                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append(indent).Append(indent).Append(Quote(list[i]));
                    sb.AppendLine(i == list.Count - 1 ? string.Empty : ",");
                }
                sb.AppendLine(indent + ")").AppendLine();
            }

            string FormatScalar(object value)
            {
                if (value == null) return "$null";
                return value switch
                {
                    string s => Quote(s),
                    bool b => b ? "$true" : "$false",
                    Enum e => Quote(e.ToString()),
                    int or long or short or byte or sbyte or uint or ulong or ushort => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
                    float f => f.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    _ => Quote(value.ToString())
                };
            }

            sb.AppendLine("@{");

            // Ordered sections
            AppendStringList("IncludeRules", settingsData.IncludeRules);
            AppendStringList("ExcludeRules", settingsData.ExcludeRules);
            AppendStringList("Severity", settingsData.Severities);
            AppendStringList("CustomRulePath", settingsData.CustomRulePath);

            if (settingsData.IncludeDefaultRules)
            {
                sb.Append(indent).Append("IncludeDefaultRules = ").AppendLine("$true").AppendLine();
            }
            if (settingsData.RecurseCustomRulePath)
            {
                sb.Append(indent).Append("RecurseCustomRulePath = ").AppendLine("$true").AppendLine();
            }

            // Rules block
            if (settingsData.RuleArguments != null && settingsData.RuleArguments.Count > 0)
            {
                sb.Append(indent).AppendLine("Rules = @{");
                foreach (var ruleKvp in settingsData.RuleArguments)
                {
                    sb.Append(indent).Append(indent).Append(ruleKvp.Key).Append(" = @{").AppendLine();
                    if (ruleKvp.Value != null && ruleKvp.Value.Count > 0)
                    {
                        foreach (var argKvp in ruleKvp.Value)
                        {
                            sb.Append(indent).Append(indent).Append(indent)
                              .Append(argKvp.Key).Append(" = ")
                              .AppendLine(FormatScalar(argKvp.Value));
                        }
                    }
                    sb.Append(indent).Append(indent).AppendLine("}").AppendLine();
                }
                sb.Append(indent).AppendLine("}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}