using Microsoft.PowerShell.ScriptAnalyzer.Builder;
using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Instantiation
{
    internal static class RuleGeneration
    {
        public static bool TryGetRuleFromType(
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            RuleComponentProvider ruleComponentProvider,
            Type type,
            out RuleInfo ruleInfo,
            out TypeRuleFactory<ScriptRule> ruleFactory)
        {
            ruleFactory = null;
            return RuleInfo.TryGetFromRuleType(type, out ruleInfo)
                && typeof(ScriptRule).IsAssignableFrom(type)
                && TryGetRuleFactory(ruleInfo, type, ruleConfigurationCollection, ruleComponentProvider, out ruleFactory);
        }

        private static bool TryGetRuleFactory<TRuleBase>(
            RuleInfo ruleInfo,
            Type ruleType,
            IReadOnlyDictionary<string, IRuleConfiguration> ruleConfigurationCollection,
            RuleComponentProvider ruleComponentProvider,
            out TypeRuleFactory<TRuleBase> factory)
        {
            ConstructorInfo[] ruleConstructors = ruleType.GetConstructors();
            if (ruleConstructors.Length != 1)
            {
                factory = null;
                return false;
            }
            ConstructorInfo ruleConstructor = ruleConstructors[0];

            Type baseType = ruleType.BaseType;
            Type configurationType = null;
            while (baseType != null)
            {
                if (baseType.IsGenericType
                    && baseType.GetGenericTypeDefinition() == typeof(Rule<>))
                {
                    configurationType = baseType.GetGenericArguments()[0];
                    break;
                }

                baseType = baseType.BaseType;
            }

            if (ruleConfigurationCollection.TryGetValue(ruleInfo.FullName, out IRuleConfiguration ruleConfiguration)
                && configurationType != null)
            {
                ruleConfiguration = ruleConfiguration.AsTypedConfiguration(configurationType);
            }

            if (ruleInfo.IsIdempotent)
            {
                factory = new ConstructorInjectionIdempotentRuleFactory<TRuleBase>(
                    ruleComponentProvider,
                    ruleInfo,
                    ruleConstructor,
                    ruleConfiguration);
                return true;
            }

            if (typeof(IResettable).IsAssignableFrom(ruleType))
            {
                factory = new ConstructorInjectingResettableRulePoolingFactory<TRuleBase>(
                    ruleComponentProvider,
                    ruleInfo,
                    ruleConstructor,
                    ruleConfiguration);
                return true;
            }

            if (typeof(IDisposable).IsAssignableFrom(ruleType))
            {
                factory = new ConstructorInjectingDisposableRuleFactory<TRuleBase>(
                    ruleComponentProvider,
                    ruleInfo,
                    ruleConstructor,
                    ruleConfiguration);
                return true;
            }

            factory = new ConstructorInjectingRuleFactory<TRuleBase>(
                ruleComponentProvider,
                ruleInfo,
                ruleConstructor,
                ruleConfiguration);
            return true;
        }
    }
}
