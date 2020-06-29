using Microsoft.PowerShell.ScriptAnalyzer.Builtin;
using Microsoft.PowerShell.ScriptAnalyzer.Execution;
using Microsoft.PowerShell.ScriptAnalyzer.Instantiation;
using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builder
{
    public class ScriptAnalyzerBuilder
    {
        private readonly List<IRuleProviderFactory> _ruleProviderFactories;

        private IRuleExecutorFactory _ruleExecutorFactory;

        private RuleComponentProvider _ruleComponentProvider;

        public ScriptAnalyzerBuilder()
        {
            _ruleProviderFactories = new List<IRuleProviderFactory>();
        }

        public ScriptAnalyzerBuilder WithRuleExecutorFactory(IRuleExecutorFactory ruleExecutorFactory)
        {
            _ruleExecutorFactory = ruleExecutorFactory;
            return this;
        }

        public ScriptAnalyzerBuilder WithRuleComponentProvider(RuleComponentProvider ruleComponentProvider)
        {
            _ruleComponentProvider = ruleComponentProvider;
            return this;
        }

        public ScriptAnalyzerBuilder WithRuleComponentProvider(Action<RuleComponentProviderBuilder> configureComponentProviderBuilder)
        {
            var componentProviderBuilder = new RuleComponentProviderBuilder();
            configureComponentProviderBuilder(componentProviderBuilder);
            WithRuleComponentProvider(componentProviderBuilder.Build());
            return this;
        }

        public ScriptAnalyzerBuilder AddRuleProviderFactory(IRuleProviderFactory ruleProvider)
        {
            _ruleProviderFactories.Add(ruleProvider);
            return this;
        }

        public ScriptAnalyzerBuilder AddBuiltinRules()
        {
            _ruleProviderFactories.Add(
                new BuiltinRuleProviderFactory(Default.RuleConfiguration));
            return this;
        }

        public ScriptAnalyzerBuilder AddBuiltinRules(Action<BuiltinRulesBuilder> configureBuiltinRules)
        {
            var builtinRulesBuilder = new BuiltinRulesBuilder();
            configureBuiltinRules(builtinRulesBuilder);
            _ruleProviderFactories.Add(builtinRulesBuilder.Build());
            return this;
        }

        public ScriptAnalyzer Build()
        {
            return ScriptAnalyzer.Create(_ruleComponentProvider, _ruleExecutorFactory, _ruleProviderFactories);
        }
    }
}
