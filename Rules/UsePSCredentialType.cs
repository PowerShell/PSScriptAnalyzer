// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{

    /// <summary>
    /// UsePSCredentialType: Checks if a parameter named Credential is of type PSCredential. Also checks if there is a credential transformation attribute defined after the PSCredential type attribute. The order between credential transformation attribute and PSCredential type attribute is applicable only to Poweshell 4.0 and earlier.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UsePSCredentialType : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check if a parameter named Credential is of type PSCredential. Also checks if there is a credential transformation attribute defined after the PSCredential type attribute. The order between the credential transformation attribute and PSCredential type attribute is applicable only to Poweshell 4.0 and earlier.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            var sbAst = ast as ScriptBlockAst;

            var requiresTransformationAttribute = false;
            var psVersion = Helper.Instance.GetPSVersion();
            if (psVersion != null && psVersion.Major < 5)
            {
                requiresTransformationAttribute = true;
            }

            // do not run the rule if the script requires PS version 5
            // but PSSA in invoked through PS version < 5
            if (sbAst != null
                && sbAst.ScriptRequirements != null
                && sbAst.ScriptRequirements.RequiredPSVersion != null
                && sbAst.ScriptRequirements.RequiredPSVersion.Major == 5
                && requiresTransformationAttribute)
            {
                        yield break;
            }

            IEnumerable<Ast> funcDefAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);

            List<DiagnosticRecord> diagnosticRecords = new List<DiagnosticRecord>();

            foreach (FunctionDefinitionAst funcDefAst in funcDefAsts)
            {
                IEnumerable<ParameterAst> parameterAsts = null;
                if (funcDefAst.Parameters != null)
                {
                    parameterAsts = funcDefAst.Parameters;
                }

                if (funcDefAst.Body.ParamBlock != null
                    && funcDefAst.Body.ParamBlock.Parameters != null)
                {
                    parameterAsts = funcDefAst.Body.ParamBlock.Parameters;
                }

                if (parameterAsts != null)
                {
                    diagnosticRecords.AddRange(GetViolations(
                        parameterAsts,
                        string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeError, funcDefAst.Name),
                        fileName,
                        requiresTransformationAttribute));
                }
            }

            foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts)
            {
                // check for the case where it's parent is function, in that case we already processed above
                if (scriptBlockAst.Parent != null && scriptBlockAst.Parent is FunctionDefinitionAst)
                {
                    continue;
                }

                if (scriptBlockAst.ParamBlock != null && scriptBlockAst.ParamBlock.Parameters != null)
                {
                    diagnosticRecords.AddRange(GetViolations(
                        scriptBlockAst.ParamBlock.Parameters,
                        string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeErrorSB),
                        fileName,
                        requiresTransformationAttribute));
                }
            }

            foreach (var dr in diagnosticRecords)
            {
                yield return dr;
            }
        }

        private IEnumerable<DiagnosticRecord> GetViolations(
            IEnumerable<ParameterAst> parameterAsts,
            string errorMessage,
            string fileName,
            bool requiresTransformationAttribute)
        {
                foreach (ParameterAst parameter in parameterAsts)
                {
                    if (WrongCredentialUsage(parameter, requiresTransformationAttribute))
                    {
                        yield return new DiagnosticRecord(
                            errorMessage,
                            parameter.Extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName);
                    }
                }
        }

        private bool WrongCredentialUsage(ParameterAst parameter, bool requiresTransformationAttribute)
        {
            if (parameter.Name.VariablePath.UserPath.Equals("Credential", StringComparison.OrdinalIgnoreCase))
            {
                var psCredentialType = parameter.Attributes.FirstOrDefault(paramAttribute => (paramAttribute.TypeName.IsArray && (paramAttribute.TypeName as ArrayTypeName).ElementType.GetReflectionType() == typeof(PSCredential))
                    || paramAttribute.TypeName.GetReflectionType() == typeof(PSCredential));

                if (psCredentialType == null)
                {
                    return true;
                }

                if (!requiresTransformationAttribute && psCredentialType != null)
                {
                    return false;
                }

                var credentialAttribute = parameter.Attributes.FirstOrDefault(
                    paramAttribute =>
                        paramAttribute.TypeName.GetReflectionType() == typeof(CredentialAttribute)
                        || paramAttribute.TypeName.FullName.Equals(
                            "System.Management.Automation.Credential",
                            StringComparison.OrdinalIgnoreCase));

                // check that both exists and pscredentialtype comes before credential attribute
                if (psCredentialType != null
                        && credentialAttribute != null
                        && psCredentialType.Extent.EndOffset <= credentialAttribute.Extent.StartOffset)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UsePSCredentialTypeName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeDescription);
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




