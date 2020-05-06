using System.Collections.Generic;

namespace Microsoft.PowerShell.ScriptAnalyzer.BackwardCompatibility
{
    internal class LegacyScriptRuleAdapter : IAstRule
    {
        private readonly IScriptRule _rule;

        public LegacyScriptRuleAdapter(IScriptRule rule)
        {
            _rule = rule;
        }

        public string Name => _rule.GetName();

        public string Namespace => _rule.GetSourceName();

        public string Description => _rule.GetDescription();

        public string SourcePath => null;

        public SourceType SourceType => _rule.GetSourceType();

        public DiagnosticSeverity Severity => (DiagnosticSeverity)_rule.GetSeverity();

        public IReadOnlyList<ScriptDiagnostic> AnalyzeScript(Ast ast, string scriptPath)
        {
            var diagnostics = new List<ScriptDiagnostic>();
            foreach (DiagnosticRecord legacyDiagnostic in _rule.AnalyzeScript(ast, scriptPath))
            {
                diagnostics.Add(ScriptDiagnostic.FromLegacyDiagnostic(ast.Extent.Text, legacyDiagnostic));
            }
            return diagnostics;
        }
    }

}
