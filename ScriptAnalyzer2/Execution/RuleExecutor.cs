using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.ScriptAnalyzer
{
    public interface IRuleExecutor
    {
        void AddRule(ScriptRule rule);

        IReadOnlyCollection<ScriptDiagnostic> CollectDiagnostics();
    }

    internal class SequentialRuleExecutor : IRuleExecutor
    {
        private readonly Ast _scriptAst;

        private readonly IReadOnlyList<Token> _scriptTokens;

        private readonly string _scriptPath;

        private readonly List<ScriptDiagnostic> _diagnostics;

        public SequentialRuleExecutor(Ast ast, IReadOnlyList<Token> tokens, string scriptPath)
        {
            _scriptAst = ast;
            _scriptTokens = tokens;
            _scriptPath = scriptPath;
            _diagnostics = new List<ScriptDiagnostic>();
        }

        public void AddRule(ScriptRule rule)
        {
            _diagnostics.AddRange(rule.AnalyzeScript(_scriptAst, _scriptTokens, _scriptPath));
        }

        public IReadOnlyCollection<ScriptDiagnostic> CollectDiagnostics()
        {
            return _diagnostics;
        }
    }

    internal class ParallelLinqRuleExecutor : IRuleExecutor
    {
        private readonly Ast _scriptAst;

        private readonly IReadOnlyList<Token> _scriptTokens;

        private readonly string _scriptPath;

        private readonly List<ScriptRule> _parallelRules;

        private readonly List<ScriptRule> _sequentialRules;

        public ParallelLinqRuleExecutor(Ast scriptAst, IReadOnlyList<Token> scriptTokens, string scriptPath)
        {
            _scriptAst = scriptAst;
            _scriptTokens = scriptTokens;
            _scriptPath = scriptPath;
            _parallelRules = new List<ScriptRule>();
            _sequentialRules = new List<ScriptRule>();
        }

        public void AddRule(ScriptRule rule)
        {
            if (rule.RuleInfo.IsThreadsafe)
            {
                _parallelRules.Add(rule);
                return;
            }

            _sequentialRules.Add(rule);
        }

        public IReadOnlyCollection<ScriptDiagnostic> CollectDiagnostics()
        {
            List<ScriptDiagnostic> diagnostics = _parallelRules.AsParallel()
                .SelectMany(rule => rule.AnalyzeScript(_scriptAst, _scriptTokens, _scriptPath))
                .ToList();

            foreach (ScriptRule sequentialRule in _sequentialRules)
            {
                diagnostics.AddRange(sequentialRule.AnalyzeScript(_scriptAst, _scriptTokens, _scriptPath));
            }

            return diagnostics;
        }
    }

    /*
    internal class DataflowRuleExecutor : IRuleExecutor
    {
        private static readonly ExecutionDataflowBlockOptions s_parallelExecutionOptions = new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
            EnsureOrdered = false,
        };

        private readonly Ast _scriptAst;

        private readonly IReadOnlyList<Token> _scriptTokens;

        private readonly string _scriptPath;

        private readonly TransformManyBlock<ScriptRule, ScriptDiagnostic> _parallelRulePipeline;

        private readonly List<ScriptRule> _sequentialRules;

        public DataflowRuleExecutor(Ast scriptAst, IReadOnlyList<Token> scriptTokens, string scriptPath)
        {
            _scriptAst = scriptAst;
            _scriptTokens = scriptTokens;
            _scriptPath = scriptPath;
            _parallelRulePipeline = new TransformManyBlock<ScriptRule, ScriptDiagnostic>(RunScriptRule, s_parallelExecutionOptions);
            _sequentialRules = new List<ScriptRule>();
        }

        public void AddRule(ScriptRule rule)
        {
            if (rule.RuleInfo.IsThreadsafe)
            {
                _parallelRulePipeline.Post(rule);
                return;
            }

            _sequentialRules.Add(rule);
        }

        public IReadOnlyCollection<ScriptDiagnostic> CollectDiagnostics()
        {
            _parallelRulePipeline.Complete();
            _parallelRulePipeline.TryReceiveAll(out IList<ScriptDiagnostic> parallelDiagnostics);

            var diagnostics = new List<ScriptDiagnostic>(parallelDiagnostics);
            foreach (ScriptRule rule in _sequentialRules)
            {
                diagnostics.AddRange(rule.AnalyzeScript(_scriptAst, _scriptTokens, _scriptPath));
            }

            return diagnostics;
        }

        private IEnumerable<ScriptDiagnostic> RunScriptRule(ScriptRule rule)
        {
            return rule.AnalyzeScript(_scriptAst, _scriptTokens, _scriptPath);
        }
    }
    */
}
