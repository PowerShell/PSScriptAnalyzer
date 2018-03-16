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
    /// AvoidUsingPlainTextForPassword: Check that parameter "password", "passphrase" do not use plaintext
    /// (they should be of the type SecureString).
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingPlainTextForPassword : IScriptRule
    {
        /// <summary>
        /// AvoidUsingPlainTextForPassword: Check that parameter "password", "passphrase" and do not use plaintext.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all ParamAsts.
            IEnumerable<Ast> paramAsts = ast.FindAll(testAst => testAst is ParameterAst, true);

            List<String> passwords = new List<String>() {"Password", "Passphrase", "Cred", "Credential"};

            // Iterates all ParamAsts and check if their names are on the list.
            foreach (ParameterAst paramAst in paramAsts)
            {
                Type paramType = paramAst.StaticType;
                bool hasPwd = false;
                String paramName = paramAst.Name.VariablePath.ToString();

                foreach (String password in passwords)
                {
                    if (paramName.IndexOf(password, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        hasPwd = true;
                        break;
                    }
                }

                if (hasPwd && ((!paramType.IsArray && (paramType == typeof(String) || paramType == typeof(object)))
                              || (paramType.IsArray && (paramType.GetElementType() == typeof(String) || paramType.GetElementType() == typeof(object)))))
                {
                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordError, paramAst.Name),
                        paramAst.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        paramName,
                        suggestedCorrections: GetCorrectionExtent(paramAst));
                }
            }
        }

        private List<CorrectionExtent> GetCorrectionExtent(ParameterAst paramAst)
        {
            //Find the parameter type extent and replace that with secure string
            IScriptExtent extent;
            var typeAttributeAst = GetTypeAttributeAst(paramAst);
            var corrections = new List<CorrectionExtent>();
            string correctionText;
            if (typeAttributeAst == null)
            {
                // cannot find any type attribute
                extent = paramAst.Name.Extent;
                correctionText = string.Format("[SecureString] {0}", paramAst.Name.Extent.Text);
            }
            else
            {
                // replace only the existing type with [SecureString]
                extent = typeAttributeAst.Extent;
                correctionText = typeAttributeAst.TypeName.IsArray ? "[SecureString[]]" : "[SecureString]";
            }
            string description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.AvoidUsingPlainTextForPasswordCorrectionDescription,
                paramAst.Name.Extent.Text);
            corrections.Add(new CorrectionExtent(
                extent.StartLineNumber,
                extent.EndLineNumber,
                extent.StartColumnNumber,
                extent.EndColumnNumber,
                correctionText.ToString(),
                paramAst.Extent.File,
                description));
            return corrections;
        }

        private TypeConstraintAst GetTypeAttributeAst(ParameterAst paramAst)
        {
            if (paramAst.Attributes != null)
            {
                foreach(var attr in paramAst.Attributes)
                {
                    if (attr.GetType() == typeof(TypeConstraintAst))
                    {
                        return attr as TypeConstraintAst;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingPlainTextForPasswordName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPlainTextForPasswordDescription);
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
