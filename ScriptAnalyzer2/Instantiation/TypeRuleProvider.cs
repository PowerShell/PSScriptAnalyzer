using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public class TypeRuleProvider : IRuleProvider
    {

        private readonly IReadOnlyDictionary<RuleInfo, TypeRuleFactory<ScriptRule>> _scriptRuleFactories;

        internal TypeRuleProvider(
            IReadOnlyDictionary<RuleInfo, TypeRuleFactory<ScriptRule>> scriptRuleFactories)
        {
            _scriptRuleFactories = scriptRuleFactories;
        }

        public IEnumerable<RuleInfo> GetRuleInfos()
        {
            return _scriptRuleFactories.Keys;
        }

        public IEnumerable<ScriptRule> GetScriptRules()
        {
            foreach (TypeRuleFactory<ScriptRule> ruleFactory in _scriptRuleFactories.Values)
            {
                yield return ruleFactory.GetRuleInstance();
            }
        }

        public void ReturnRule(Rule rule)
        {
            if (!(rule is ScriptRule scriptRule))
            {
                return;
            }

            if (_scriptRuleFactories.TryGetValue(rule.RuleInfo, out TypeRuleFactory<ScriptRule> astRuleFactory))
            {
                astRuleFactory.ReturnRuleInstance(scriptRule);
            }
        }

    }
}
