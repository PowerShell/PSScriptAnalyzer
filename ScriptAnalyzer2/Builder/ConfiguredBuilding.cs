using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Execution;
using Microsoft.PowerShell.ScriptAnalyzer.Instantiation;
using Microsoft.PowerShell.ScriptAnalyzer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builder
{
    public static class ConfiguredBuilding
    {
        public static ScriptAnalyzer CreateScriptAnalyzer(this IScriptAnalyzerConfiguration configuration)
        {
            var analyzerBuilder = new ScriptAnalyzerBuilder()
                .WithRuleComponentProvider(new RuleComponentProviderBuilder().Build());

            switch (configuration.BuiltinRules ?? BuiltinRulePreference.Default)
            {
                case BuiltinRulePreference.Aggressive:
                case BuiltinRulePreference.Default:
                    analyzerBuilder.AddBuiltinRules();
                    break;
            }

            switch (configuration.RuleExecution ?? RuleExecutionMode.Default)
            {
                case RuleExecutionMode.Default:
                case RuleExecutionMode.Parallel:
                    analyzerBuilder.WithRuleExecutorFactory(new ParallelLinqRuleExecutorFactory());
                    break;

                case RuleExecutionMode.Sequential:
                    analyzerBuilder.WithRuleExecutorFactory(new SequentialRuleExecutorFactory());
                    break;
            }

            if (configuration.RulePaths != null)
            {
                foreach (string rulePath in configuration.RulePaths)
                {
                    string extension = Path.GetExtension(rulePath);

                    if (extension.CaseInsensitiveEquals(".dll"))
                    {
                        analyzerBuilder.AddRuleProviderFactory(TypeRuleProviderFactory.FromAssemblyFile(configuration.RuleConfiguration, rulePath));
                        break;
                    }
                }
            }

            return analyzerBuilder.Build();
        }
    }
}
