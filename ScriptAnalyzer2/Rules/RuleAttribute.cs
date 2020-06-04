using System;
using System.Reflection;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    public abstract class ScriptAnalyzerAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RuleAttribute : ScriptAnalyzerAttribute
    {
        public RuleAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Warning;

        public string Namespace { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RuleDescriptionAttribute : ScriptAnalyzerAttribute
    {
        private readonly Lazy<string> _descriptionLazy;

        public RuleDescriptionAttribute(string description)
        {
            _descriptionLazy = new Lazy<string>(() => description);
        }

        public RuleDescriptionAttribute(Type resourceProvider, string resourceKey)
        {
            _descriptionLazy = new Lazy<string>(() => GetStringFromResourceProvider(resourceProvider, resourceKey));
        }

        public string Description => _descriptionLazy.Value;

        private static string GetStringFromResourceProvider(Type resourceProvider, string resourceKey)
        {
            PropertyInfo resourceProperty = resourceProvider.GetProperty(resourceKey, BindingFlags.Static | BindingFlags.NonPublic);
            return (string)resourceProperty.GetValue(null);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ThreadsafeRuleAttribute : ScriptAnalyzerAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IdempotentRuleAttribute : ScriptAnalyzerAttribute
    {
    }
}
