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
                throw new ArgumentNullException("attributeAsts");
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
    }
}
