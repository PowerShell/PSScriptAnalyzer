// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// InvokeExpressionRule: Check to make sure that Invoke-Expression is not used.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class InvokeExpressionRule : AvoidCmdletGeneric
    {
        /// <summary>
        /// GetCmdletName: Retrieves the name of the cmdlet
        /// </summary>
        /// <returns></returns>
        public override string GetCmdletName()
        {
            return "invoke-expression";
        }

        /// <summary>
        /// GetError: Retrieves the error message
        /// </summary>
        /// <returns></returns>
        public override string GetError(String FileName)
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionError);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingInvokeExpressionRuleName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionRuleCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingInvokeExpressionRuleDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity:Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// DiagnosticSeverity: Retrieves the severity of the rule of type DiagnosticSeverity: error, warning or information.
        /// </summary>
        /// <returns></returns>
        public override DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

    }
}




