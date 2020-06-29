using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    public class TypeRuleProviderFactory : IRuleProviderFactory
    {
        public static TypeRuleProviderFactory FromAssemblyFile(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            string assemblyPath)
        {
            return FromAssembly(ruleConfigurationCollection, Assembly.LoadFile(assemblyPath));
        }

        public static TypeRuleProviderFactory FromAssembly(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            Assembly ruleAssembly)
        {
            return new TypeRuleProviderFactory(ruleConfigurationCollection, ruleAssembly.GetExportedTypes());
        }

        private readonly IReadOnlyDictionary<string, IRuleConfiguration> _ruleConfigurationCollection;

        private readonly IReadOnlyList<Type> _types;

        public TypeRuleProviderFactory(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            IReadOnlyList<Type> types)
        {
            _ruleConfigurationCollection = ruleConfigurationCollection;
            _types = types;
        }

        public IRuleProvider CreateRuleProvider(RuleComponentProvider ruleComponentProvider)
        {
            return new TypeRuleProvider(GetRuleFactoriesFromTypes(_ruleConfigurationCollection, ruleComponentProvider, _types));
        }
        
        private static IReadOnlyDictionary<RuleInfo, TypeRuleFactory<ScriptRule>> GetRuleFactoriesFromTypes(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            RuleComponentProvider ruleComponentProvider,
            IReadOnlyList<Type> types)
        {
            var ruleFactories = new Dictionary<RuleInfo, TypeRuleFactory<ScriptRule>>();

            foreach (Type type in types)
            {
                if (RuleGeneration.TryGetRuleFromType(
                    ruleConfigurationCollection,
                    ruleComponentProvider,
                    type,
                    out RuleInfo ruleInfo,
                    out TypeRuleFactory<ScriptRule> factory))
                {
                    ruleFactories[ruleInfo] = factory;
                }
            }

            return ruleFactories;
        }
    }

}
