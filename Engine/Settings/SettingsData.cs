// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Data container representing fully parsed and normalized PSScriptAnalyzer settings.
    /// </summary>
    public sealed class SettingsData
    {
        /// <summary>
        /// Explicit rule names to include.
        /// </summary>
        public List<string> IncludeRules { get; set; } = new List<string>();

        /// <summary>
        /// Rule names to exclude from analysis even if they are part of defaults or includes.
        /// </summary>
        public List<string> ExcludeRules { get; set; } = new List<string>();

        /// <summary>
        /// Ordered severity list used for filtering or overriding rule output (e.g. Error, Warning,
        /// Information).
        /// </summary>
        public List<string> Severities { get; set; } = new List<string>();

        /// <summary>
        /// Paths (files or directories) where custom rule assemblies/modules are located.
        /// </summary>
        public List<string> CustomRulePath { get; set; } = new List<string>();

        /// <summary>
        /// Indicates whether built-in default rules should be included when resolving effective
        /// rule set.
        /// </summary>
        public bool IncludeDefaultRules { get; set; }

        /// <summary>
        /// If true, recursively searches each CustomRulePath directory for rules.
        /// </summary>
        public bool RecurseCustomRulePath { get; set; }

        /// <summary>
        /// Per-rule argument maps: rule name -> (argument name -> value). Case-insensitive outer
        /// and inner dictionaries.
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> RuleArguments { get; set; } =
            new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
    }
}