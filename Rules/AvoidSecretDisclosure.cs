// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidSecretDisclosure: Checks whether a plaintext secret is being retrieved which can lead to
    /// security vulnerabilities such as memory trails or logging trails.
    /// The general approach of dealing with credentials is to avoid them and instead rely on other means
    /// to authenticate, such as certificates or Windows authentication.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidSecretDisclosure : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of AvoidSecretDisclosure type.
        /// </summary>
        public AvoidSecretDisclosure() {
            Enable = true;
        }

        /// <summary>
        /// Analyzes the given ast to find the [violation]
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Check for ConvertFrom-SecureString with -AsPlainText parameter
            IEnumerable<Ast> convertFromSecureStringAsts = ast.FindAll(testAst =>
                testAst is CommandAst cmdAst &&
                cmdAst.GetCommandName() != null &&
                cmdAst.GetCommandName().Equals("ConvertFrom-SecureString", StringComparison.OrdinalIgnoreCase),
                true
            );

            foreach (CommandAst cmdAst in convertFromSecureStringAsts)
            {
                // Use StaticParameterBinder to reliably get parameter values
                var bindingResult = StaticParameterBinder.BindCommand(cmdAst, true);

                // Check for -AsPlainText parameter
                // The constantValue appears true even the value is a variable
                // This is ok because the rule should still trigger in that case since the value of the
                // variable could be true at runtime, and we want to catch all potential violations
                if (
                    bindingResult.BoundParameters.ContainsKey("AsPlainText") &&
                    bindingResult.BoundParameters["AsPlainText"].ConstantValue is bool constantValue &&
                    constantValue == true
                ) {
                    yield return GetDiagnosticRecord(cmdAst.Extent, fileName, "AsPlainText");
                }
            }

            // Check for any invocation of a method that starts with "SecureStringTo"
            // (e.g. SecureStringToBSTR, SecureStringToCoTaskMemAnsi, etc.)
            IEnumerable<Ast> secureStringToAsts = ast.FindAll(testAst =>
                testAst is InvokeMemberExpressionAst invokeAst &&
                invokeAst.Member != null &&
                invokeAst.Member.ToString().StartsWith("SecureStringTo", StringComparison.OrdinalIgnoreCase),
                true
            );

            foreach (InvokeMemberExpressionAst secureStringToAst in secureStringToAsts) {
                yield return GetDiagnosticRecord(secureStringToAst.Extent, fileName, secureStringToAst.Member.ToString());
            }

            // Check for any member access of a property named "Password".
            // Note that this is a heuristic and may lead to false positives,
            // as not all properties named "Password" necessarily contain secrets,
            // and there may be secrets stored in properties with other names.
            // However, it is too complex to reliably determine whether a Password
            // property is a result of e.g. a PSCredential.GetNetworkCredential() call.
            // Anyways, this is still a useful common check to have.
            IEnumerable<Ast> passwordAsts = ast.FindAll(testAst =>
                testAst is MemberExpressionAst memberAst &&
                memberAst.Member != null &&
                string.Equals(memberAst.Member.ToString(), "Password", StringComparison.OrdinalIgnoreCase),
                true
            );

            foreach (MemberExpressionAst passwordAst in passwordAsts) {
                yield return GetDiagnosticRecord(passwordAst.Extent, fileName, passwordAst.Member.ToString());
            }

        }

        /// <summary>
        /// Helper function to create a DiagnosticRecord for a given violation
        /// </summary>
        private DiagnosticRecord GetDiagnosticRecord(IScriptExtent Extent, string fileName, string suppressionId)
        {
            return new DiagnosticRecord(
                Strings.AvoidSecretDisclosureError,
                Extent,
                GetName(),
                DiagnosticSeverity.Warning,
                fileName,
                suppressionId
            );
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidSecretDisclosureCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidSecretDisclosureDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidSecretDisclosureName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}

