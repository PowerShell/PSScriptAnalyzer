using Microsoft.PowerShell.ScriptAnalyzer.Configuration.Psd;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.PowerShell.ScriptAnalyzer.Tools
{
    public static class AstTools
    {
        private readonly static PsdDataParser s_psdDataParser = new PsdDataParser();

        public static object GetSafeValueFromAst(ExpressionAst ast)
        {
            return s_psdDataParser.ConvertAstValue(ast);
        }

        public static bool TryGetCmdletBindingAttributeAst(
            IEnumerable<AttributeAst> attributes,
            out AttributeAst cmdletBindingAttributeAst)
        {
            foreach (var attributeAst in attributes)
            {
                if (attributeAst == null || attributeAst.NamedArguments == null)
                {
                    continue;
                }

                if (attributeAst.TypeName.GetReflectionAttributeType() == typeof(CmdletBindingAttribute))
                {
                    cmdletBindingAttributeAst = attributeAst;
                    return true;
                }
            }

            cmdletBindingAttributeAst = null;
            return false;
        }

        public static bool TryGetShouldProcessAttributeArgumentAst(
            IEnumerable<AttributeAst> attributes,
            out NamedAttributeArgumentAst shouldProcessArgument)
        {
            if (!TryGetCmdletBindingAttributeAst(attributes, out AttributeAst cmdletBindingAttributeAst)
                || cmdletBindingAttributeAst.NamedArguments == null
                || cmdletBindingAttributeAst.NamedArguments.Count == 0)
            {
                shouldProcessArgument = null;
                return false;
            }

            foreach (NamedAttributeArgumentAst namedAttributeAst in cmdletBindingAttributeAst.NamedArguments)
            {
                if (namedAttributeAst.ArgumentName.Equals("SupportsShouldProcess", StringComparison.OrdinalIgnoreCase))
                {
                    shouldProcessArgument = namedAttributeAst;
                    return true;
                }
            }

            shouldProcessArgument = null;
            return false;
        }

        internal static ExpressionAst GetExpressionAstFromScriptAst(Ast ast)
        {
            var scriptBlockAst = (ScriptBlockAst)ast;

            if (scriptBlockAst.EndBlock == null)
            {
                throw new InvalidPowerShellExpressionException("Expected 'end' block in PowerShell input");
            }

            if (scriptBlockAst.EndBlock.Statements == null
                || scriptBlockAst.EndBlock.Statements.Count == 0)
            {
                throw new InvalidPowerShellExpressionException("No statements to parse expression from in input");
            }

            if (scriptBlockAst.EndBlock.Statements.Count != 1)
            {
                throw new InvalidPowerShellExpressionException("Expected a single expression in input");
            }

            if (!(scriptBlockAst.EndBlock.Statements[0] is PipelineAst pipelineAst))
            {
                throw new InvalidPowerShellExpressionException($"Statement '{scriptBlockAst.EndBlock.Statements[0].Extent.Text}' is not a valid expression");
            }

            if (pipelineAst.PipelineElements.Count != 0)
            {
                throw new InvalidPowerShellExpressionException("Cannot use pipelines in expressions");
            }

            if (!(pipelineAst.PipelineElements[0] is CommandExpressionAst commandExpressionAst))
            {
                throw new InvalidPowerShellExpressionException($"Pipeline element '{pipelineAst.PipelineElements[0]}' is not a command expression");
            }

            return commandExpressionAst.Expression;
        }
    }
}
