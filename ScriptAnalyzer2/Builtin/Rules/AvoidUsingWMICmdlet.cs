// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builtin.Rules
{
    /// <summary>
    /// AvoidUsingWMICmdlet: Avoid Using Get-WMIObject, Remove-WMIObject, Invoke-WmiMethod, Register-WmiEvent, Set-WmiInstance
    /// </summary>
    [ThreadsafeRule]
    [IdempotentRule]
    [RuleDescription(typeof(Strings), nameof(Strings.AvoidUsingWMICmdletDescription))]
    [Rule("AvoidUsingWMICmdlet")]
    public class AvoidUsingWMICmdlet : ScriptRule
    {
        public AvoidUsingWMICmdlet(RuleInfo ruleInfo)
            : base(ruleInfo)
        {
        }

        /// <summary>
        /// AnalyzeScript: Avoid Using Get-WMIObject, Remove-WMIObject, Invoke-WmiMethod, Register-WmiEvent, Set-WmiInstance
        /// </summary>
        public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
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
                            yield return CreateDiagnostic(
                                String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletErrorScriptDefinition),
                                cmdAst.Extent);
                        }
                        else
                        {
                            yield return CreateDiagnostic(
                                String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWMICmdletError, System.IO.Path.GetFileName(fileName)),
                                cmdAst.Extent);
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
    }
}




