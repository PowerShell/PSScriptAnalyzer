using Microsoft.PowerShell.ScriptAnalyzer.Builtin;
using Microsoft.PowerShell.ScriptAnalyzer.Execution;
using Microsoft.PowerShell.ScriptAnalyzer.Instantiation;
using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builder
{
    public class ScriptAnalyzerBuilder
    {
        private readonly List<IRuleProvider> _ruleProviders;

        private IRuleExecutorFactory _ruleExecutorFactory;

        public ScriptAnalyzerBuilder()
        {
            _ruleProviders = new List<IRuleProvider>();
        }

        public ScriptAnalyzerBuilder WithRuleExecutorFactory(IRuleExecutorFactory ruleExecutorFactory)
        {
            _ruleExecutorFactory = ruleExecutorFactory;
            return this;
        }

        public ScriptAnalyzerBuilder AddRuleProvider(IRuleProvider ruleProvider)
        {
            _ruleProviders.Add(ruleProvider);
            return this;
        }

        public ScriptAnalyzerBuilder AddBuiltinRules()
        {
            _ruleProviders.Add(TypeRuleProvider.FromTypes(
                Default.RuleConfiguration,
                Default.RuleComponentProvider,
                BuiltinRules.DefaultRules));
            return this;
        }

        public ScriptAnalyzerBuilder AddBuiltinRules(Action<BuiltinRulesBuilder> configureBuiltinRules)
        {
            var builtinRulesBuilder = new BuiltinRulesBuilder();
            configureBuiltinRules(builtinRulesBuilder);
            _ruleProviders.Add(builtinRulesBuilder.Build());
            return this;
        }

        public ScriptAnalyzer Build()
        {
            IRuleProvider ruleProvider = _ruleProviders.Count == 1
                ? _ruleProviders[0]
                : new CompositeRuleProvider(_ruleProviders);

            return new ScriptAnalyzer(ruleProvider, _ruleExecutorFactory ?? Default.RuleExecutorFactory);
        }
    }
}
