// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{

    /// <summary>
    /// Parses JSON settings files (extension .json) into <see cref="SettingsData"/>.
    /// Expected top-level properties:
    ///   Severity              : string or string array
    ///   IncludeRules          : string or string array
    ///   ExcludeRules          : string or string array
    ///   CustomRulePath        : string or string array
    ///   IncludeDefaultRules   : bool
    ///   RecurseCustomRulePath : bool
    ///   Rules                 : object with ruleName -> { argumentName : value } mapping
    /// Parsing logic:
    /// 1. Read entire stream into a string.
    /// 2. Deserialize to DTO with Newtonsoft.Json (case-insensitive by default).
    /// 3. Validate null result -> invalid data.
    /// 4. Normalize each collection to empty lists when absent.
    /// 5. Rebuild rule arguments as case-insensitive dictionaries.
    /// Throws <see cref="InvalidDataException"/> on malformed JSON or missing structure.
    /// </summary>
    internal sealed class JsonSettingsParser : ISettingsParser
    {

        /// <summary>
        /// DTO for deserializing JSON settings.
        /// </summary>
        private sealed class JsonSettingsDto
        {
            public List<string> Severity { get; set; }
            public List<string> IncludeRules { get; set; }
            public List<string> ExcludeRules { get; set; }
            public List<string> CustomRulePath { get; set; }
            public bool? IncludeDefaultRules { get; set; }
            public bool? RecurseCustomRulePath { get; set; }
            public Dictionary<string, Dictionary<string, object>> Rules { get; set; }
        }

        public string FormatName => "json";

        /// <summary>
        /// Determines if this parser can handle the supplied path by checking for .json extension.
        /// </summary>
        /// <param name="pathOrExtension">File path or extension string.</param>
        /// <returns>True if extension is .json.</returns>
        public bool CanParse(string pathOrExtension) =>
            string.Equals(Path.GetExtension(pathOrExtension), ".json", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Parses a JSON settings file stream into <see cref="SettingsData"/>.
        /// </summary>
        /// <param name="content">Readable stream positioned at start of JSON content.</param>
        /// <param name="sourcePath">Original file path (for error context).</param>
        /// <returns>Populated <see cref="SettingsData"/>.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown on JSON deserialization error or invalid/empty root object.
        /// </exception>
        public SettingsData Parse(Stream content, string sourcePath)
        {
            using var reader = new StreamReader(content);
            string json = reader.ReadToEnd();
            JsonSettingsDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<JsonSettingsDto>(json);
            }
            catch (JsonException je)
            {
                throw new InvalidDataException($"Failed to parse settings JSON '{sourcePath}': {je.Message}", je);
            }
            if (dto == null)
                throw new InvalidDataException($"Settings JSON '{sourcePath}' is empty or invalid.");

            // Normalize rule arguments into case-insensitive dictionaries
            var ruleArgs = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            if (dto.Rules != null)
            {
                foreach (var kv in dto.Rules)
                {
                    ruleArgs[kv.Key] = kv.Value != null
                        ? new Dictionary<string, object>(kv.Value, StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
            }

            return new SettingsData
            {
                IncludeRules = dto.IncludeRules ?? new List<string>(),
                ExcludeRules = dto.ExcludeRules ?? new List<string>(),
                Severities = dto.Severity ?? new List<string>(),
                CustomRulePath = dto.CustomRulePath ?? new List<string>(),
                IncludeDefaultRules = dto.IncludeDefaultRules.GetValueOrDefault(),
                RecurseCustomRulePath = dto.RecurseCustomRulePath.GetValueOrDefault(),
                RuleArguments = ruleArgs
            };
        }
    }

}