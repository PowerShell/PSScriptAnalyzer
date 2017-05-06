using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    // TODO Add documentation
    public static class Extensions
    {
        public static IEnumerable<string> GetLines(this string text)
        {
            return text.Split('\n').Select(line => line.TrimEnd('\r'));
        }

        /// <summary>
        /// Converts IScriptExtent to Range
        /// </summary>
        public static Range ToRange(this IScriptExtent extent)
        {
           return new Range(
                extent.StartLineNumber,
                extent.StartColumnNumber,
                extent.EndLineNumber,
                extent.EndColumnNumber);
        }

        public static ParameterAst[] GetParameterAsts(
            this FunctionDefinitionAst functionDefinitionAst,
            out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = null;
            if (functionDefinitionAst.Parameters != null)
            {
                return new List<ParameterAst>(functionDefinitionAst.Parameters).ToArray();
            }
            else if (functionDefinitionAst.Body.ParamBlock?.Parameters != null)
            {
                paramBlockAst = functionDefinitionAst.Body.ParamBlock;
                return new List<ParameterAst>(functionDefinitionAst.Body.ParamBlock.Parameters).ToArray();
            }

            return null;
        }
    }
}
