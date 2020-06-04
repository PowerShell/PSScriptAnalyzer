
using System;
using System.Data;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    public class RuleInfo
    {
        public static bool TryGetFromRuleType(Type ruleType, out RuleInfo ruleInfo)
        {
            return TryGetFromRuleType(ruleType, SourceType.Assembly, out ruleInfo);
        }

        internal static bool TryGetFromRuleType(Type ruleType, SourceType source, out RuleInfo ruleInfo)
        {
            var ruleAttr = ruleType.GetCustomAttribute<RuleAttribute>();

            if (ruleAttr == null)
            {
                ruleInfo = null;
                return false;
            }

            var ruleDescriptionAttr = ruleType.GetCustomAttribute<RuleDescriptionAttribute>();
            var threadsafeAttr = ruleType.GetCustomAttribute<ThreadsafeRuleAttribute>();
            var idempotentAttr = ruleType.GetCustomAttribute<IdempotentRuleAttribute>();

            string ruleNamespace = ruleAttr.Namespace
                ?? ruleType.Assembly.GetCustomAttribute<RuleCollectionAttribute>()?.Name
                ?? ruleType.Assembly.GetName().Name;

            ruleInfo = new RuleInfo(ruleAttr.Name, ruleNamespace)
            {
                Description = ruleDescriptionAttr.Description,
                Severity = ruleAttr.Severity,
                Source = source,
                IsIdempotent = idempotentAttr != null,
                IsThreadsafe = threadsafeAttr != null,
            };
            return true;
        }

        internal static bool TryGetBuiltinRule(Type ruleType, out RuleInfo ruleInfo)
        {
            return TryGetFromRuleType(ruleType, SourceType.Builtin, out ruleInfo);
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

        public DiagnosticSeverity Severity { get; private set; }

        public bool IsThreadsafe { get; private set; }

        public bool IsIdempotent { get; private set;  }

        public SourceType Source { get; private set;  }
    }
}
