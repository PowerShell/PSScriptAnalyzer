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
    /// AvoidShouldContinueWithoutForceParameter: Check that if ShouldContinue is used,
    /// the function should have a boolean force parameter
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidShouldContinueWithoutForce : IScriptRule
    {
        /// <summary>
        /// AvoidShouldContinueWithoutForceCheck that if ShouldContinue is used,
        /// the function should have a boolean force parameter
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all ParamAsts.
            IEnumerable<Ast> funcAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);

            // Iterates all ParamAsts and check if there are any force.
            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                IEnumerable<Ast> paramAsts = funcAst.FindAll(testAst => testAst is ParameterAst, true);
                bool hasForce = false;

                foreach (ParameterAst paramAst in paramAsts)
                {
                    if (String.Equals(paramAst.Name.VariablePath.ToString(), "force", StringComparison.OrdinalIgnoreCase)
                        && (String.Equals(paramAst.StaticType.FullName, "System.Boolean", StringComparison.OrdinalIgnoreCase) 
                        || String.Equals(paramAst.StaticType.FullName, "System.Management.Automation.SwitchParameter", StringComparison.OrdinalIgnoreCase)))
                    {
                        hasForce = true;
                        break;
                    }
                }

                if (hasForce)
                {
                    continue;
                }

                IEnumerable<Ast> imeAsts = funcAst.FindAll(testAst => testAst is InvokeMemberExpressionAst, true);

                foreach (InvokeMemberExpressionAst imeAst in imeAsts)
                {
                    VariableExpressionAst typeAst = imeAst.Expression as VariableExpressionAst;
                    if (typeAst == null) continue;

                    if (String.Equals(typeAst.VariablePath.UserPath, "pscmdlet", StringComparison.OrdinalIgnoreCase)
                        && (String.Equals(imeAst.Member.Extent.Text, "shouldcontinue", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (String.IsNullOrWhiteSpace(fileName))
                        {
                            yield return new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueWithoutForceErrorScriptDefinition, funcAst.Name),
                                imeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                        else
                        {
                            yield return new DiagnosticRecord(
                                String.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueWithoutForceError, funcAst.Name,
                                System.IO.Path.GetFileName(fileName)), imeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidShouldContinueWithoutForceName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueWithoutForceCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidShouldContinueWithoutForceDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
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




