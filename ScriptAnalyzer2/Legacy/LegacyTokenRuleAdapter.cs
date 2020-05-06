using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.BackwardCompatibility
{
    internal class LegacyTokenRuleAdapter : ITokenRule
    {
        private readonly Windows.PowerShell.ScriptAnalyzer.Generic.ITokenRule _rule;

        public LegacyTokenRuleAdapter(Windows.PowerShell.ScriptAnalyzer.Generic.ITokenRule rule)
        {
            _rule = rule;
        }

        public string Name => _rule.GetName();

        public string Namespace => _rule.GetSourceName();

        public string Description => _rule.GetDescription();

        public string SourcePath => null;

        public SourceType SourceType => _rule.GetSourceType();

        public DiagnosticSeverity Severity => (DiagnosticSeverity)_rule.GetSeverity();

        public IReadOnlyList<ScriptDiagnostic> AnalyzeScript(IReadOnlyList<Token> tokens, string scriptPath)
        {
            var tokenArray = new Token[tokens.Count];
            for (int i = 0; i < tokens.Count; i++)
            {
                tokenArray[i] = tokens[i];
            }

            string scriptText = tokens[0].Extent.StartScriptPosition.GetFullScript();
            var diagnostics = new List<ScriptDiagnostic>();
            foreach (DiagnosticRecord legacyDiagnostic in _rule.AnalyzeTokens(tokenArray, scriptPath))
            {
                diagnostics.Add(ScriptDiagnostic.FromLegacyDiagnostic(scriptText, legacyDiagnostic));
            }
            return diagnostics;
        }
    }
}
