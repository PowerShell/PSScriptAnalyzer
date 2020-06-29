using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public interface IRuleProviderFactory
    {
        IRuleProvider CreateRuleProvider(RuleComponentProvider ruleComponentProvider);
    }
}
