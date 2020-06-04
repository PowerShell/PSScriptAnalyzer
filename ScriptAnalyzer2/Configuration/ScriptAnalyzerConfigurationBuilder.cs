using Microsoft.PowerShell.ScriptAnalyzer.Configuration.Json;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration.Psd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Configuration
{
    public class ScriptAnalyzerConfigurationBuilder
    {
        private readonly List<string> _rulePaths;

        private readonly Dictionary<string, IRuleConfiguration> _ruleConfigurations;

        private BuiltinRulePreference? _builtinRulePreference;

        private RuleExecutionMode? _executionMode;

        public ScriptAnalyzerConfigurationBuilder()
        {
            _rulePaths = new List<string>();
            _ruleConfigurations = new Dictionary<string, IRuleConfiguration>();
        }

        public ScriptAnalyzerConfigurationBuilder WithBuiltinRuleSet(BuiltinRulePreference builtinRulePreference)
        {
            _builtinRulePreference = builtinRulePreference;
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder WithRuleExecutionMode(RuleExecutionMode executionMode)
        {
            _executionMode = executionMode;
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddNonConfiguredRule(string ruleName)
        {
            _ruleConfigurations[ruleName] = RuleConfiguration.Default;
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddRuleConfiguration(string ruleName, IRuleConfiguration ruleConfiguration)
        {
            _ruleConfigurations[ruleName] = ruleConfiguration;
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddRuleConfigurations(IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurations)
        {
            foreach (KeyValuePair<string, IRuleConfiguration> entry in ruleConfigurations)
            {
                _ruleConfigurations[entry.Key] = entry.Value;
            }
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddRulePath(string rulePath)
        {
            _rulePaths.Add(rulePath);
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddRulePaths(IEnumerable<string> rulePaths)
        {
            _rulePaths.AddRange(rulePaths);
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder ExcludeRule(string rule)
        {
            _ruleConfigurations.Remove(rule);
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder ExcludeRules(IEnumerable<string> rules)
        {
            foreach (string rule in rules)
            {
                _ruleConfigurations.Remove(rule);
            }
            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddConfiguration(IScriptAnalyzerConfiguration configuration)
        {
            if (configuration.BuiltinRules != null)
            {
                WithBuiltinRuleSet(configuration.BuiltinRules.Value);
            }

            if (configuration.RuleExecution != null)
            {
                WithRuleExecutionMode(configuration.RuleExecution.Value);
            }

            AddRulePaths(configuration.RulePaths);
            AddRuleConfigurations(configuration.RuleConfiguration);

            return this;
        }

        public ScriptAnalyzerConfigurationBuilder AddConfiguration(Action<ScriptAnalyzerConfigurationBuilder> configureSubConfiguration)
        {
            var subConfiguration = new ScriptAnalyzerConfigurationBuilder();
            configureSubConfiguration(subConfiguration);
            return AddConfiguration(subConfiguration.Build());
        }

        public ScriptAnalyzerConfigurationBuilder AddConfigurationFile(string filePath)
        {
            if (string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase))
            {
                AddConfiguration(JsonScriptAnalyzerConfiguration.FromFile(filePath));
            }
            else
            {
                AddConfiguration(PsdScriptAnalyzerConfiguration.FromFile(filePath));
            }

            return this;
        }

        public IScriptAnalyzerConfiguration Build()
        {
            return new MemoryScriptAnalyzerConfiguration(_builtinRulePreference, _executionMode, _rulePaths, _ruleConfigurations);
        }
    }

    internal class MemoryScriptAnalyzerConfiguration : IScriptAnalyzerConfiguration
    {
        public MemoryScriptAnalyzerConfiguration(
            BuiltinRulePreference? builtinRulePreference,
            RuleExecutionMode? ruleExecutionMode,
            IReadOnlyList<string> rulePaths,
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurations)
        {
            BuiltinRules = builtinRulePreference;
            RuleExecution = ruleExecutionMode;
            RulePaths = rulePaths;
            RuleConfiguration = ruleConfigurations;
        }

        public IReadOnlyList<string> RulePaths { get; }

        public RuleExecutionMode? RuleExecution { get; }

        public IReadOnlyDictionary<string, IRuleConfiguration> RuleConfiguration { get; }

        public BuiltinRulePreference? BuiltinRules { get; }
    }
}
