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
    /// AvoidUsingWMICmdlet: Avoid Using Get-WMIObject, Remove-WMIObject, Invoke-WmiMethod, Register-WmiEvent, Set-WmiInstance
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingWMICmdlet : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Avoid Using Get-WMIObject, Remove-WMIObject, Invoke-WmiMethod, Register-WmiEvent, Set-WmiInstance
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Rule is applicable only when PowerShell Version is < 3.0, since CIM cmdlet was introduced in 3.0
            int majorPSVersion = GetPSMajorVersion(ast);
            if (!(3 > majorPSVersion && 0 < majorPSVersion))
            {
                // Finds all CommandAsts.
                IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);

                // Iterate all CommandAsts and check the command name
                foreach (CommandAst cmdAst in commandAsts)
                {
                    if (cmdAst.GetCommandName() != null && 
                        (String.Equals(cmdAst.GetCommandName(), "get-wmiobject", StringComparison.OrdinalIgnoreCase) 
                            || String.Equals(cmdAst.GetCommandName(), "remove-wmiobject", StringComparison.OrdinalIgnoreCase)
                            || String.Equals(cmdAst.GetCommandName(), "invoke-wmimethod", StringComparison.OrdinalIgnoreCase)
                            || String.Equals(cmdAst.GetCommandName(), "register-wmievent", StringComparison.OrdinalIgnoreCase)
                            || String.Equals(cmdAst.GetCommandName(), "set-wmiinstance", StringComparison.OrdinalIgnoreCase))
                        )
                    {
                        if (String.IsNullOrWhiteSpace(fileName))
                        {
                            yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletErrorScriptDefinition),
                                cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                        else
                        {
                            yield return new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletError,
                                System.IO.Path.GetFileName(fileName)), cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetPSMajorVersion: Retrieves Major PowerShell Version when supplied using #requires keyword in the script
        /// </summary>
        /// <returns>The name of this rule</returns>
        private int GetPSMajorVersion(Ast ast)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);
            
            foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts)
            {
                if (null != scriptBlockAst.ScriptRequirements && null != scriptBlockAst.ScriptRequirements.RequiredPSVersion)
                {
                    return scriptBlockAst.ScriptRequirements.RequiredPSVersion.Major;
                }
            }

            // return a non valid Major version if #requires -Version is not supplied in the Script
            return -1;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingWMICmdletName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }


        /// <summary>
        /// GetSeverity:Retrieves the severity of the rule: error, warning of information.
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




