using Microsoft.PowerShell.ScriptAnalyzer.Configuration;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.ScriptAnalyzer.Rules
{
    public interface IResettable
    {
        void Reset();
    }

    public abstract class Rule
    {
        protected Rule(RuleInfo ruleInfo)
        {
            RuleInfo = ruleInfo;
        }

        public RuleInfo RuleInfo { get; }

        protected ScriptDiagnostic CreateDiagnostic(string message, IScriptExtent extent)
            => CreateDiagnostic(message, extent, RuleInfo.Severity);

        protected ScriptDiagnostic CreateDiagnostic(string message, IScriptExtent extent, DiagnosticSeverity severity)
            => CreateDiagnostic(message, extent, severity, corrections: null);

        protected ScriptDiagnostic CreateDiagnostic(string message, IScriptExtent extent, IReadOnlyList<Correction> corrections)
            => CreateDiagnostic(message, extent, RuleInfo.Severity, corrections);

        protected ScriptDiagnostic CreateDiagnostic(string message, IScriptExtent extent, DiagnosticSeverity severity, IReadOnlyList<Correction> corrections)
        {
            return new ScriptDiagnostic(RuleInfo, message, extent, severity, corrections);
        }
    }

    public abstract class Rule<TConfiguration> : Rule where TConfiguration : IRuleConfiguration
    {
        protected Rule(RuleInfo ruleInfo, TConfiguration ruleConfiguration)
            : base(ruleInfo)
        {
            Configuration = ruleConfiguration;
        }

        public TConfiguration Configuration { get; }
    }

    public abstract class ScriptRule : Rule
    {
        protected ScriptRule(RuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        public abstract IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string scriptPath);

        protected ScriptAstDiagnostic CreateDiagnostic(string message, Ast ast)
            => CreateDiagnostic(message, ast, RuleInfo.Severity);

        protected ScriptAstDiagnostic CreateDiagnostic(string message, Ast ast, DiagnosticSeverity severity)
            => CreateDiagnostic(message, ast, severity, corrections: null);

        protected ScriptAstDiagnostic CreateDiagnostic(string message, Ast ast, IReadOnlyList<Correction> corrections)
            => CreateDiagnostic(message, ast, RuleInfo.Severity, corrections);

        protected ScriptAstDiagnostic CreateDiagnostic(string message, Ast ast, DiagnosticSeverity severity, IReadOnlyList<Correction> corrections)
        {
            return new ScriptAstDiagnostic(RuleInfo, message, ast, severity, corrections);
        }

        protected ScriptTokenDiagnostic CreateDiagnostic(string message, Token token)
            => CreateDiagnostic(message, token, RuleInfo.Severity);

        protected ScriptTokenDiagnostic CreateDiagnostic(string message, Token token, DiagnosticSeverity severity)
            => CreateDiagnostic(message, token, severity, corrections: null);

        protected ScriptTokenDiagnostic CreateDiagnostic(string message, Token token, IReadOnlyList<Correction> corrections)
            => CreateDiagnostic(message, token, RuleInfo.Severity, corrections);

        protected ScriptTokenDiagnostic CreateDiagnostic(string message, Token token, DiagnosticSeverity severity, IReadOnlyList<Correction> corrections)
        {
            return new ScriptTokenDiagnostic(RuleInfo, message, token, severity, corrections);
        }
    }

    public abstract class ScriptRule<TConfiguration> : Rule<TConfiguration> where TConfiguration : IRuleConfiguration
    {
        protected ScriptRule(RuleInfo ruleInfo, TConfiguration ruleConfiguration) : base(ruleInfo, ruleConfiguration)
        {
        }

        public abstract IReadOnlyList<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string scriptPath);
    }
}
