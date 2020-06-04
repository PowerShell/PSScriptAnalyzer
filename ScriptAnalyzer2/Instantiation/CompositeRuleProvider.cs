using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public class CompositeRuleProvider : IRuleProvider
    {
        private readonly IReadOnlyList<IRuleProvider> _ruleProviders;

        private readonly ConcurrentDictionary<RuleInfo, IRuleProvider> _ruleReturnDictionary;

        public CompositeRuleProvider(IReadOnlyList<IRuleProvider> ruleProviders)
        {
            _ruleReturnDictionary = new ConcurrentDictionary<RuleInfo, IRuleProvider>();
            _ruleProviders = ruleProviders;
        }

        public IEnumerable<RuleInfo> GetRuleInfos()
        {
            foreach (IRuleProvider ruleProvider in _ruleProviders)
            {
                foreach (RuleInfo ruleInfo in ruleProvider.GetRuleInfos())
                {
                    yield return ruleInfo;
                }
            }
        }

        public IEnumerable<ScriptRule> GetScriptRules()
        {
            foreach (IRuleProvider ruleProvider in _ruleProviders)
            {
                foreach (ScriptRule rule in ruleProvider.GetScriptRules())
                {
                    _ruleReturnDictionary.TryAdd(rule.RuleInfo, ruleProvider);
                    yield return rule;
                }
            }
        }

        public void ReturnRule(Rule rule)
        {
            if (_ruleReturnDictionary.TryGetValue(rule.RuleInfo, out IRuleProvider ruleProvider))
            {
                ruleProvider.ReturnRule(rule);
            }
        }
    }
}
