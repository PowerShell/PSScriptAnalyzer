// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using Microsoft.PowerShell.ScriptAnalyzer;
using Microsoft.PowerShell.ScriptAnalyzer.Builtin.Rules;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidGlobalVars: Analyzes the ast to check that global variables are not used.
    /// </summary>
    [IdempotentRule]
    [ThreadsafeRule]
    [RuleDescription(typeof(Strings), nameof(Strings.AvoidGlobalVarsDescription))]
    [Rule("AvoidGlobalVars")]
    public class AvoidGlobalVars : ScriptRule
    {
        public AvoidGlobalVars(RuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that global variables are not used. From the ILintScriptRule interface.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
        {
            IEnumerable<Ast> varAsts = ast.FindAll(testAst => testAst is VariableExpressionAst, true);

            if (varAsts != null)
            {
                foreach (VariableExpressionAst varAst in varAsts)
                {
                    if (varAst.VariablePath.IsGlobal)
                    {
                        yield return CreateDiagnostic(
                                string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalVarsError, varAst.VariablePath.UserPath),
                                varAst);
                    }
                }
            }
        }
    }
}




