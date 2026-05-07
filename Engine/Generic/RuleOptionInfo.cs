// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Holds metadata for a single configurable rule property.
    /// </summary>
    public class RuleOptionInfo
    {
        /// <summary>
        /// The name of the configurable property.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The CLR type of the property value.
        /// </summary>
        public Type OptionType { get; internal set; }

        /// <summary>
        /// The default value declared via the ConfigurableRuleProperty attribute.
        /// </summary>
        public object DefaultValue { get; internal set; }

        /// <summary>
        /// The set of valid values for this property, if constrained.
        /// Null when any value of the declared type is acceptable.
        /// </summary>
        public object[] PossibleValues { get; internal set; }

        /// <summary>
        /// Extracts RuleOptionInfo entries for every ConfigurableRuleProperty on
        /// the given rule. For string properties backed by a private enum, the
        /// possible values are populated from the enum members.
        /// </summary>
        /// <param name="rule">The rule instance to inspect.</param>
        /// <returns>
        /// A list of option metadata, ordered with Enable first then the
        /// remainder sorted alphabetically.
        /// </returns>
        public static List<RuleOptionInfo> GetRuleOptions(IRule rule)
        {
            var options = new List<RuleOptionInfo>();
            Type ruleType = rule.GetType();

            PropertyInfo[] properties = ruleType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Collect all private nested enums declared on the rule type so we
            // can match them against string properties whose default value is an
            // enum member name.
            Type[] nestedEnums = ruleType
                .GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
                .Where(t => t.IsEnum)
                .ToArray();

            foreach (PropertyInfo prop in properties)
            {
                var attr = prop.GetCustomAttribute<ConfigurableRulePropertyAttribute>(inherit: true);
                if (attr == null)
                {
                    continue;
                }

                var info = new RuleOptionInfo
                {
                    Name = prop.Name,
                    OptionType = prop.PropertyType,
                    DefaultValue = attr.DefaultValue,
                    PossibleValues = null
                };

                // For string properties, attempt to find a matching private enum
                // whose member names include the default value. This mirrors the
                // pattern used by rules such as UseConsistentIndentation and
                // ProvideCommentHelp where a string property is parsed into a
                // private enum via Enum.TryParse.
                //
                // When multiple enums contain the default value (e.g. both have
                // a "None" member), prefer the enum whose name contains the
                // property name or vice-versa (e.g. property "Kind" matches enum
                // "IndentationKind"). This helps avoid incorrect matches when a rule
                // declares several enums with possible overlapping member names.
                if (prop.PropertyType == typeof(string) && attr.DefaultValue is string defaultStr)
                {
                    Type bestMatch = null;
                    bool bestHasNameRelation = false;

                    foreach (Type enumType in nestedEnums)
                    {
                        if (!Enum.GetNames(enumType).Any(n =>
                            string.Equals(n, defaultStr, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        bool hasNameRelation =
                            enumType.Name.IndexOf(prop.Name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            prop.Name.IndexOf(enumType.Name, StringComparison.OrdinalIgnoreCase) >= 0;

                        // Take this enum if we have no match yet, or if it has a
                        // name-based relationship and the previous match did not.
                        if (bestMatch == null || (hasNameRelation && !bestHasNameRelation))
                        {
                            bestMatch = enumType;
                            bestHasNameRelation = hasNameRelation;
                        }
                    }

                    if (bestMatch != null)
                    {
                        info.PossibleValues = Enum.GetNames(bestMatch);
                    }
                }

                options.Add(info);
            }

            // Sort with "Enable" first, then alphabetically by name for consistent ordering.
            return options
                .OrderBy(o => string.Equals(o.Name, "Enable", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
