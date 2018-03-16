// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingNullOrEmptyHelpMessageAttribute: Check if the HelpMessage parameter is set to a non-empty string.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidNullOrEmptyHelpMessageAttribute : IScriptRule
    {               
        /// <summary>
        /// AvoidUsingNullOrEmptyHelpMessageAttribute: Check if the HelpMessage parameter is set to a non-empty string.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                if (funcAst.Body == null || funcAst.Body.ParamBlock == null || funcAst.Body.ParamBlock.Parameters == null)
                {
                    continue;
                }
                                           
                foreach (ParameterAst paramAst in funcAst.Body.ParamBlock.Parameters)
                {
                    if (paramAst == null)
                    {
                        continue;
                    }

                    foreach (AttributeBaseAst attributeAst in paramAst.Attributes)
                    {
                        var paramAttributeAst = attributeAst as AttributeAst;
                        if (paramAttributeAst == null)
                        {
                            continue;
                        }

                        var namedArguments = paramAttributeAst.NamedArguments;
                        if (namedArguments == null)
                        {
                            continue;
                        }
                                               
                        foreach (NamedAttributeArgumentAst namedArgument in namedArguments)
                        {
                            if (namedArgument == null || !(namedArgument.ArgumentName.Equals("HelpMessage", StringComparison.OrdinalIgnoreCase)))
                            {
                                continue;
                            }
                            
                            string errCondition;
                            if (namedArgument.ExpressionOmitted || HasEmptyStringInExpression(namedArgument.Argument))
                            {
                                errCondition = "empty";
                            }                                                        
                            else if (HasNullInExpression(namedArgument.Argument))
                            {
                                errCondition = "null";
                            }
                            else
                            {
                                errCondition = null;
                            }
                            if (!String.IsNullOrEmpty(errCondition))
                            {
                                string message = string.Format(CultureInfo.CurrentCulture,
                                                                Strings.AvoidNullOrEmptyHelpMessageAttributeError,
                                                                paramAst.Name.VariablePath.UserPath);
                                yield return new DiagnosticRecord(message,
                                                                    paramAst.Extent, 
                                                                    GetName(), 
                                                                    DiagnosticSeverity.Warning, 
                                                                    fileName, 
                                                                    paramAst.Name.VariablePath.UserPath);
                            }                            
                        }                                                
                    }
                }
                
            }
        }

        /// <summary>
        /// Checks if the given ast is an empty string.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private bool HasEmptyStringInExpression(ExpressionAst ast)
        {
            var constStrAst = ast as StringConstantExpressionAst;
            return constStrAst != null && constStrAst.Value.Equals(String.Empty);
        }

        /// <summary>
        /// Checks if the ast contains null expression.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private bool HasNullInExpression(Ast ast)
        {
            var varExprAst = ast as VariableExpressionAst;
            return varExprAst != null
                    && varExprAst.VariablePath.IsUnqualified
                    && varExprAst.VariablePath.UserPath.Equals("null", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidNullOrEmptyHelpMessageAttributeName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidNullOrEmptyHelpMessageAttributeCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidNullOrEmptyHelpMessageAttributeDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




