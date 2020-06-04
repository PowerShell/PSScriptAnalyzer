using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Builtin.Rules;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Execution;
using Microsoft.PowerShell.ScriptAnalyzer.Runtime;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules;
using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builtin
{
    public static class BuiltinRules
    {
        public static IReadOnlyList<Type> DefaultRules { get; } = new[]
        {
            typeof(AvoidEmptyCatchBlock),
            typeof(AvoidGlobalVars),
            typeof(AvoidPositionalParameters),
            typeof(AvoidUsingWMICmdlet),
            typeof(UseDeclaredVarsMoreThanAssignments),
            typeof(UseShouldProcessForStateChangingFunctions),
        };
    }

    public static class Default
    {
        private static readonly Lazy<IRuleComponentProvider> s_ruleComponentProviderLazy = new Lazy<IRuleComponentProvider>(BuildRuleComponentProvider);

        public static IReadOnlyDictionary<string, IRuleConfiguration> RuleConfiguration { get; } = new Dictionary<string, IRuleConfiguration>(StringComparer.OrdinalIgnoreCase)
        {
            { "PS/AvoidUsingEmptyCatchBlock", null },
            { "PS/AvoidGlobalVars", null },
            { "PS/AvoidUsingPositionalParameters", null },
            { "PS/AvoidUsingWMICmdlet", null },
            { "PS/UseDeclaredVarsMoreThanAssignments", null },
            { "PS/UseShouldProcessForStateChangingFunctions", null },
        };

        public static IRuleExecutorFactory RuleExecutorFactory { get; } = new ParallelLinqRuleExecutorFactory();

        public static IRuleComponentProvider RuleComponentProvider => s_ruleComponentProviderLazy.Value;

        private static IRuleComponentProvider BuildRuleComponentProvider()
        {
            return new RuleComponentProviderBuilder()
                .AddSingleton(InstantiatePowerShellCommandDatabase())
                .Build();
        }

        private static IPowerShellCommandDatabase InstantiatePowerShellCommandDatabase()
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                return SessionStateCommandDatabase.Create(runspace.SessionStateProxy.InvokeCommand);
            }
        }
    }

}
