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
    /// AvoidEmptyCatchBlock: Check if any empty catch block is used.
    /// </summary>
    [IdempotentRule]
    [ThreadsafeRule]
    [RuleDescription(typeof(Strings), nameof(Strings.AvoidUsingEmptyCatchBlockDescription))]
    [Rule("AvoidUsingEmptyCatchBlock")]
    public class AvoidEmptyCatchBlock : ScriptRule
    {
        public AvoidEmptyCatchBlock(RuleInfo ruleInfo)
            : base(ruleInfo)
        {
        }

        /// <summary>
        /// AnalyzeScript: Analyze the script to check if any empty catch block is used.
        /// </summary>
        public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all CommandAsts.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is CatchClauseAst, true);

            // Iterates all CatchClauseAst and check the statements count.
            foreach (Ast foundAst in foundAsts)
            {
                CatchClauseAst catchAst = (CatchClauseAst)foundAst;

                if (catchAst.Body.Statements.Count == 0)
                {
                    yield return CreateDiagnostic(
                        string.Format(CultureInfo.CurrentCulture, Strings.AvoidEmptyCatchBlockError),
                        catchAst);
                }
            }
        }
    }
}




