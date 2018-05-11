// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidDefaultValueForMandatoryParameter: Check if a mandatory parameter does not have a default value.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidDefaultValueForMandatoryParameter : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if a mandatory parameter has a default value.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all functionAst
            IEnumerable<Ast> functionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            foreach (FunctionDefinitionAst funcAst in functionAsts)
            {
                if (funcAst.Body != null && funcAst.Body.ParamBlock != null
                    && funcAst.Body.ParamBlock.Attributes != null && funcAst.Body.ParamBlock.Parameters != null)
                {
                    foreach (var paramAst in funcAst.Body.ParamBlock.Parameters)
                    {
                        bool mandatory = false;

                        // check that param is mandatory
                        foreach (var paramAstAttribute in paramAst.Attributes)
                        {
                            if (paramAstAttribute is AttributeAst)
                            {
                                var namedArguments = (paramAstAttribute as AttributeAst).NamedArguments;
                                if (namedArguments != null)
                                {
                                    foreach (NamedAttributeArgumentAst namedArgument in namedArguments)
                                    {
                                        if (String.Equals(namedArgument.ArgumentName, "mandatory", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // 3 cases: [Parameter(Mandatory)], [Parameter(Mandatory=$true)] and [Parameter(Mandatory=value)] where value is not equal to 0.
                                            if (namedArgument.ExpressionOmitted
                                                || (String.Equals(namedArgument.Argument.Extent.Text, "$true", StringComparison.OrdinalIgnoreCase))
                                                || (int.TryParse(namedArgument.Argument.Extent.Text, out int mandatoryValue) && mandatoryValue != 0))
                                            {
                                                mandatory = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!mandatory)
                        {
                            break;
                        }

                        if (paramAst.DefaultValue != null)
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueForMandatoryParameterError, paramAst.Name.VariablePath.UserPath),
                            paramAst.Name.Extent, GetName(), DiagnosticSeverity.Warning, fileName, paramAst.Name.VariablePath.UserPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidDefaultValueForMandatoryParameterName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueForMandatoryParameterCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueForMandatoryParameterDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




