using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public class PSModuleRuleProvider : IRuleProvider
    {
        private readonly string _module;

        public PSModuleRuleProvider(string module)
        {
            _module = module;
        }

        public IEnumerable<RuleInfo> GetRuleInfos()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ScriptRule> GetScriptRules()
        {
            throw new NotImplementedException();
        }

        public void ReturnRule(Rule rule)
        {
            throw new NotImplementedException();
        }
    }
}
