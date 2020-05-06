using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Execution
{
    public interface IRuleExecutorFactory
    {
        IRuleExecutor CreateRuleExecutor(Ast ast, IReadOnlyList<Token> tokens, string scriptPath);
    }

    public class SequentialRuleExecutorFactory : IRuleExecutorFactory
    {
        public IRuleExecutor CreateRuleExecutor(Ast ast, IReadOnlyList<Token> tokens, string scriptPath)
        {
            return new SequentialRuleExecutor(ast, tokens, scriptPath);
        }
    }

    public class ParallelLinqRuleExecutorFactory : IRuleExecutorFactory
    {
        public IRuleExecutor CreateRuleExecutor(Ast ast, IReadOnlyList<Token> tokens, string scriptPath)
        {
            return new ParallelLinqRuleExecutor(ast, tokens, scriptPath);
        }
    }
}
