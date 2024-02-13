// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingAllowUnencryptedAuthentication: Avoid sending credentials and secrets over unencrypted connections.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingAllowUnencryptedAuthentication : AvoidParameterGeneric
    {
        /// <summary>
        /// Condition on the cmdlet that must be satisfied for the error to be raised
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override bool CommandCondition(CommandAst CmdAst)
        {
            return true;
        }

        /// <summary>
        /// Condition on the parameter that must be satisfied for the error to be raised.
        /// </summary>
        /// <param name="CmdAst"></param>
        /// <param name="CeAst"></param>
        /// <returns></returns>
        public override bool ParameterCondition(CommandAst CmdAst, CommandElementAst CeAst)
        {
            return CeAst is CommandParameterAst && String.Equals((CeAst as CommandParameterAst).ParameterName, "AllowUnencryptedAuthentication", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Retrieves the error message
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="CmdAst"></param>
        /// <returns></returns>
        public override string GetError(string fileName, CommandAst cmdAst)
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingAllowUnencryptedAuthenticationError);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingAllowUnencryptedAuthenticationName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingAllowUnencryptedAuthenticationCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingAllowUnencryptedAuthenticationDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
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
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
