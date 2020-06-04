using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Tools
{
    public static class PowerShellParsing
    {
        public static HashtableAst ParseHashtableFromInput(string input)
        {
            ExpressionAst ast = ParseExpressionFromInput(input);

            if (!(ast is HashtableAst hashtableAst))
            {
                throw new InvalidPowerShellExpressionException($"Expression '{ast.Extent.Text}' was expected to be a hashtable");
            }

            return hashtableAst;
        }

        public static HashtableAst ParseHashtableFromFile(string filePath)
        {
            ExpressionAst ast = ParseExpressionFromFile(filePath);

            if (!(ast is HashtableAst hashtableAst))
            {
                throw new InvalidPowerShellExpressionException($"Expression '{ast.Extent.Text}' was expected to be a hashtable");
            }

            return hashtableAst;
        }

        public static ExpressionAst ParseExpressionFromInput(string input)
        {
            Ast ast = Parser.ParseInput(input, out Token[] _, out ParseError[] errors);

            if (errors != null && errors.Length > 0)
            {
                throw new PowerShellParseErrorException("Unable to parse input", ast, errors);
            }

            return AstTools.GetExpressionAstFromScriptAst(ast);
        }

        public static ExpressionAst ParseExpressionFromFile(string filePath)
        {
            Ast ast = Parser.ParseFile(filePath, out Token[] _, out ParseError[] errors);

            if (errors != null && errors.Length > 0)
            {
                throw new PowerShellParseErrorException("Unable to parse input", ast, errors);
            }

            return AstTools.GetExpressionAstFromScriptAst(ast);
        }

    }
}
