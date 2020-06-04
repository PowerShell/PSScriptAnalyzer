// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using Microsoft.PowerShell.ScriptAnalyzer.Tools;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builtin.Rules
{
    /// <summary>
    /// UseShouldProcessForStateChangingFunctions: Analyzes the ast to check if ShouldProcess is included in Advanced functions if the Verb of the function could change system state.
    /// </summary>
    [RuleDescription(typeof(Strings), nameof(Strings.UseShouldProcessForStateChangingFunctionsDescrption))]
    [Rule("UseShouldProcessForStateChangingFunctions")]
    public class UseShouldProcessForStateChangingFunctions : ScriptRule
    {
        private static readonly IReadOnlyList<string> s_stateChangingVerbs = new List<string>
        {
            { "New-" },
            { "Set-" },
            { "Remove-" },
            { "Start-" },
            { "Stop-" },
            { "Restart-" },
            { "Reset-" },
            { "Update-" }
        };

        public UseShouldProcessForStateChangingFunctions(RuleInfo ruleInfo)
            : base(ruleInfo)
        {
        }

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check if ShouldProcess is included in Advanced functions if the Verb of the function could change system state.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
        {
            IEnumerable<Ast> funcDefWithNoShouldProcessAttrAsts = ast.FindAll(IsStateChangingFunctionWithNoShouldProcessAttribute, true);            

            foreach (FunctionDefinitionAst funcDefAst in funcDefWithNoShouldProcessAttrAsts)
            {
                yield return new ScriptDiagnostic(
                    RuleInfo,
                    string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsError, funcDefAst.Name),
                    funcDefAst.GetFunctionNameExtent(tokens),
                    DiagnosticSeverity.Warning);
            }
                            
        }
        /// <summary>
        /// Checks if the ast defines a state changing function
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>Returns true or false</returns>
        private bool IsStateChangingFunctionWithNoShouldProcessAttribute(Ast ast)
        {
            var funcDefAst = ast as FunctionDefinitionAst;
            // SupportsShouldProcess is not supported in workflows
            if (funcDefAst == null || funcDefAst.IsWorkflow)
            {
                return false;
            }

            return IsStateChangingFunctionName(funcDefAst.Name) 
                    && (funcDefAst.Body.ParamBlock == null
                        || funcDefAst.Body.ParamBlock.Attributes == null
                        || !HasShouldProcessTrue(funcDefAst.Body.ParamBlock.Attributes));
        }

        /// <summary>
        /// Checks if an attribute has SupportShouldProcess set to $true
        /// </summary>
        /// <param name="attributeAsts"></param>
        /// <returns>Returns true or false</returns>
        private bool HasShouldProcessTrue(IEnumerable<AttributeAst> attributeAsts)
        {
            return AstTools.TryGetShouldProcessAttributeArgumentAst(attributeAsts, out NamedAttributeArgumentAst shouldProcessArgument)
                && object.Equals(shouldProcessArgument.GetValue(), true);
        }

        private static bool IsStateChangingFunctionName(string functionName)
        {
            foreach (string verb in s_stateChangingVerbs)
            {
                if (functionName.StartsWith(verb, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}




