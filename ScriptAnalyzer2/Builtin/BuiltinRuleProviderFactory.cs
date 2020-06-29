using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Instantiation;
using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builtin
{
    internal class BuiltinRuleProviderFactory : TypeRuleProviderFactory
    {
        public BuiltinRuleProviderFactory(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection)
            : base(ruleConfigurationCollection ?? Default.RuleConfiguration, BuiltinRules.DefaultRules)
        {
        }
    }
}
