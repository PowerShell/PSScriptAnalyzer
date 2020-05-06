using System;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class RuleCollectionAttribute : ScriptAnalyzerAttribute
    {
        public string Name { get; set; }
    }
}
