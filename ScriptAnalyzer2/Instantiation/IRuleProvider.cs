using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public interface IRuleProvider
    {
        IEnumerable<RuleInfo> GetRuleInfos();

        IEnumerable<ScriptRule> GetScriptRules();

        void ReturnRule(Rule rule);
    }
}
