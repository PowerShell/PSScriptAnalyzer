// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Return the lines in a text string.
        /// </summary>
        /// <param name="text">Text string to be split around new lines.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the parameter Asts from a function definition Ast.
        ///
        /// If not parameters are found, return null.
        /// </summary>
        public static IEnumerable<ParameterAst> GetParameterAsts(
            this FunctionDefinitionAst functionDefinitionAst)
        {
            ParamBlockAst paramBlockAst;
            return functionDefinitionAst.GetParameterAsts(out paramBlockAst);
        }

        /// <summary>
        /// Get the parameter Asts from a function definition Ast.
        ///
        /// If not parameters are found, return null.
        /// </summary>
        /// <param name="paramBlockAst">If a parameter block is present, set this argument's value to the parameter block.</param>
        /// <returns></returns>
        public static IEnumerable<ParameterAst> GetParameterAsts(
            this FunctionDefinitionAst functionDefinitionAst,
            out ParamBlockAst paramBlockAst)
        {
            // todo instead of returning null return an empty enumerator if no parameter is found
            // this removes the burden from the user for null checking.
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

        /// <summary>
        /// Check if an attribute Ast is of CmdletBindingAttribute type.
        /// </summary>
        public static bool IsCmdletBindingAttributeAst(this AttributeAst attributeAst)
        {
            return attributeAst.TypeName.GetReflectionAttributeType() == typeof(CmdletBindingAttribute);
        }

        /// <summary>
        /// Given a CmdletBinding attribute ast, return the SupportsShouldProcess argument Ast.
        ///
        /// If no SupportsShouldProcess argument is found, return null.
        /// </summary>
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


        /// <summary>
        /// Return the boolean value of a named attribute argument.
        /// </summary>
        public static bool GetValue(this NamedAttributeArgumentAst attrAst)
        {
            ExpressionAst argumentAst;
            return attrAst.GetValue(out argumentAst);
        }

        /// <summary>
        /// Return the boolean value of a named attribute argument.
        /// </summary>
        /// <param name="argumentAst">The ast of the argument's value</param>
        public static bool GetValue(this NamedAttributeArgumentAst attrAst, out ExpressionAst argumentAst)
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
