
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    public class RuleInfo
    {
        private static readonly ConcurrentDictionary<RuleAttribute, RuleInfo> s_ruleInfoCache = new ConcurrentDictionary<RuleAttribute, RuleInfo>();

        public static bool TryGetFromRuleType(Type ruleType, out RuleInfo ruleInfo)
        {
            return TryGetFromRuleType(ruleType, SourceType.Assembly, out ruleInfo);
        }

        internal static bool TryGetBuiltinRule(Type ruleType, out RuleInfo ruleInfo)
        {
            return TryGetFromRuleType(ruleType, SourceType.Builtin, out ruleInfo);
        }

        internal static bool TryGetFromRuleType(Type ruleType, SourceType source, out RuleInfo ruleInfo)
        {
            string defaultRuleCollectionName = ruleType.Assembly.GetCustomAttribute<RuleCollectionAttribute>()?.Name
                ?? ruleType.Assembly.GetName().Name;
            return TryGetFromAttributeList(ruleType.GetCustomAttributes(), source, defaultRuleCollectionName, out ruleInfo);
        }

        internal static bool TryGetFromFunctionInfo(FunctionInfo functionInfo, out RuleInfo ruleInfo)
        {
            return TryGetFromAttributeList(functionInfo.ScriptBlock.Attributes, SourceType.PowerShellModule, functionInfo.ModuleName, out ruleInfo);
        }

        private static bool TryGetFromAttributeList(IEnumerable<Attribute> attributes, SourceType source, string ruleCollectionName, out RuleInfo ruleInfo)
        {
            RuleAttribute ruleAttribute = null;
            ThreadsafeRuleAttribute threadsafeAttribute = null;
            IdempotentRuleAttribute idempotentAttribute = null;
            foreach (Attribute attribute in attributes)
            {
                switch (attribute)
                {
                    case RuleAttribute ruleAttr:
                        ruleAttribute = ruleAttr;
                        continue;

                    case ThreadsafeRuleAttribute tsAttr:
                        threadsafeAttribute = tsAttr;
                        continue;

                    case IdempotentRuleAttribute idempotentAttr:
                        idempotentAttribute = idempotentAttr;
                        continue;
                }
            }

            if (ruleAttribute == null)
            {
                ruleInfo = null;
                return false;
            }

            string ruleNamespace = ruleAttribute.Namespace ?? ruleCollectionName;
            ruleInfo = new RuleInfo(ruleAttribute.Name, ruleNamespace)
            {
                DefaultSeverity = ruleAttribute.Severity,
                Description = ruleAttribute.Description,
                Source = source,
                IsIdempotent = idempotentAttribute != null,
                IsThreadsafe = threadsafeAttribute != null,
            };
            return true;
        }

        private RuleInfo(
            string name,
            string @namespace)
        {
            Name = name;
            Namespace = @namespace;
            FullName = $"{@namespace}/{name}";
        }

        public string Name { get; }

        public string Namespace { get; }

        public string FullName { get; }

        public string Description { get; private set; }

        public DiagnosticSeverity DefaultSeverity { get; private set; }

        public bool IsThreadsafe { get; private set; }

        public bool IsIdempotent { get; private set;  }

        public SourceType Source { get; private set;  }
    }
}
