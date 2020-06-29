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
        private readonly Lazy<string> _descriptionLazy;

        public RuleAttribute(string name, string description)
        {
            Name = name;
            _descriptionLazy = new Lazy<string>(() => description);
        }


        public RuleAttribute(string name, Type descriptionResourceProvider, string descriptionResourceKey)
        {
            Name = name;
            _descriptionLazy = new Lazy<string>(() => GetStringFromResourceProvider(descriptionResourceProvider, descriptionResourceKey));
        }

        public string Name { get; }

        public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Warning;

        public string Namespace { get; set; }

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
