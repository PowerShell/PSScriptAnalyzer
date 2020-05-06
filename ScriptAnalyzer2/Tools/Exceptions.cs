using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.ScriptAnalyzer.Tools
{
    public class PowerShellParseErrorException : ScriptAnalyzerException
    {
        public PowerShellParseErrorException(string message, Ast parsedAst, IReadOnlyList<ParseError> parseErrors)
            : base(message)
        {
            ParsedAst = parsedAst;
            ParseErrors = parseErrors;
        }

        public Ast ParsedAst { get; }

        public IReadOnlyList<ParseError> ParseErrors { get; }
    }

    public class InvalidPowerShellExpressionException : ScriptAnalyzerException
    {
        public InvalidPowerShellExpressionException(string message)
            : base(message)
        {
        }
    }
}
