// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{

    /// <summary>
    /// Handles JSON settings files (extension .json).
    /// Expected top-level properties:
    ///   Severity              : string or string array
    ///   IncludeRules          : string or string array
    ///   ExcludeRules          : string or string array
    ///   CustomRulePath        : string or string array
    ///   IncludeDefaultRules   : bool
    ///   RecurseCustomRulePath : bool
    ///   Rules                 : object with ruleName -> { argumentName : value } mapping
    /// </summary>
    internal sealed class JsonSettingsFormat : ISettingsFormat
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
        /// Determines if this format can handle the supplied path by checking for .json extension.
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>True if extension is .json.</returns>
        public bool Supports(string path) =>
            string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Parses a JSON settings file stream into <see cref="SettingsData"/>.
        /// </summary>
        /// <param name="content">Readable stream positioned at start of JSON content.</param>
        /// <param name="sourcePath">Original file path (for error context).</param>
        /// <returns>Populated <see cref="SettingsData"/>.</returns>
        /// <exception cref="InvalidDataException">
        /// Thrown on JSON deserialization error or invalid/empty root object.
        /// </exception>
        public SettingsData Deserialize(string content, string sourcePath)
        {
            JsonSettingsDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<JsonSettingsDto>(content);
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

        /// <summary>
        /// Serializes a <see cref="SettingsData"/> instance into a PSScriptAnalyzerSettings.json
        /// formatted string. Omits empty collections and false boolean flags for brevity.
        /// </summary>
        /// <param name="data">Settings snapshot to serialize.</param>
        /// <param name="pretty">True for indented JSON, false for minified.</param>
        /// <returns>JSON string suitable for saving as PSScriptAnalyzerSettings.json.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null.</exception>
        public string Serialize(SettingsData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var root = new JObject();
            var serializer = JsonSerializer.CreateDefault();

            void AddArray(string name, IList<string> list)
            {
                if (list != null && list.Count > 0)
                {
                    root[name] = new JArray(list);
                }
            }

            AddArray("IncludeRules", data.IncludeRules);
            AddArray("ExcludeRules", data.ExcludeRules);
            AddArray("Severity", data.Severities);
            AddArray("CustomRulePath", data.CustomRulePath);

            if (data.IncludeDefaultRules)
            {
                root["IncludeDefaultRules"] = true;
            }
            if (data.RecurseCustomRulePath)
            {
                root["RecurseCustomRulePath"] = true;
            }

            if (data.RuleArguments != null && data.RuleArguments.Count > 0)
            {
                var rulesObj = new JObject();
                foreach (var rule in data.RuleArguments)
                {
                    var argsObj = new JObject();
                    if (rule.Value != null)
                    {
                        foreach (var arg in rule.Value)
                        {
                            // Serialize scalar or complex value
                            argsObj[arg.Key] = arg.Value != null
                                ? JToken.FromObject(arg.Value, serializer)
                                : JValue.CreateNull();
                        }
                    }
                    rulesObj[rule.Key] = argsObj;
                }
                root["Rules"] = rulesObj;
            }
            return root.ToString(Formatting.Indented);
        }
    }

}