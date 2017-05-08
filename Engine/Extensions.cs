using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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

        public static IEnumerable<ParameterAst> GetParameterAsts(
            this FunctionDefinitionAst functionDefinitionAst,
            out ParamBlockAst paramBlockAst)
        {
            paramBlockAst = null;
            if (functionDefinitionAst.Parameters != null)
            {
                return functionDefinitionAst.Parameters;
            }
            else if (functionDefinitionAst.Body.ParamBlock?.Parameters != null)
            {
                paramBlockAst = functionDefinitionAst.Body.ParamBlock;
                return functionDefinitionAst.Body.ParamBlock.Parameters;
            }

            return null;
        }

        /// <summary>
        /// Get the CmdletBinding attribute ast
        /// </summary>
        /// <param name="attributeAsts"></param>
        /// <returns>Returns CmdletBinding attribute ast if it exists, otherwise returns null</returns>
        public static AttributeAst GetCmdletBindingAttributeAst(this ParamBlockAst paramBlockAst)
        {
            var attributeAsts = paramBlockAst.Attributes;
            if (attributeAsts == null)
            {
                return null;
            }

            foreach (var attributeAst in attributeAsts)
            {
                if (attributeAst != null && attributeAst.IsCmdletBindingAttributeAst())
                {
                    return attributeAst;
                }
            }

            return null;
        }

        public static bool IsCmdletBindingAttributeAst(this AttributeAst attributeAst)
        {
            return attributeAst.TypeName.GetReflectionAttributeType() == typeof(CmdletBindingAttribute);
        }

        public static NamedAttributeArgumentAst GetSupportsShouldProcessAst(this AttributeAst attributeAst)
        {
            if (!attributeAst.IsCmdletBindingAttributeAst()
                || attributeAst.NamedArguments == null)
            {
                return null;
            }

            foreach (var namedAttrAst in attributeAst.NamedArguments)
            {
                if (namedAttrAst != null
                    && namedAttrAst.ArgumentName.Equals(
                        "SupportsShouldProcess",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return namedAttrAst;
                }
            }

            return null;
        }

        public static bool IsTrue(this NamedAttributeArgumentAst attrAst, out ExpressionAst argumentAst)
        {
            argumentAst = null;
            if (attrAst.ExpressionOmitted)
            {
                return true;
            }

            var varExpAst = attrAst.Argument as VariableExpressionAst;
            argumentAst = attrAst.Argument;
            if (varExpAst == null)
            {
                var constExpAst = attrAst.Argument as ConstantExpressionAst;
                if (constExpAst == null)
                {
                    return false;
                }

                bool constExpVal;
                if (LanguagePrimitives.TryConvertTo(constExpAst.Value, out constExpVal))
                {
                    return constExpVal;
                }
            }
            else
            {
                return varExpAst.VariablePath.UserPath.Equals(
                    bool.TrueString,
                    StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
