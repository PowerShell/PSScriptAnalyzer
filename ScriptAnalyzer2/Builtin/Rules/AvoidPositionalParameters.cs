// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.PowerShell.ScriptAnalyzer.Rules;
using Microsoft.PowerShell.ScriptAnalyzer;

namespace Microsoft.PowerShell.ScriptAnalyzer.Builtin.Rules
{
    /// <summary>
    /// AvoidPositionalParameters: Check to make sure that positional parameters are not used.
    /// </summary>
    public class AvoidPositionalParameters : ScriptRule
    {
        public AvoidPositionalParameters(RuleInfo ruleInfo) : base(ruleInfo)
        {
        }

        /// <summary>
        /// AnalyzeScript: Analyze the ast to check that positional parameters are not used.
        /// </summary>
        public override IEnumerable<ScriptDiagnostic> AnalyzeScript(Ast ast, IReadOnlyList<Token> tokens, string fileName)
        {
            // Find all function definitions in the script and add them to the set.
            IEnumerable<Ast> functionDefinitionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            var declaredFunctionNames = new HashSet<string>();

            foreach (FunctionDefinitionAst functionDefinitionAst in functionDefinitionAsts)
            {
                if (string.IsNullOrEmpty(functionDefinitionAst.Name))
                {
                    continue;
                }
                declaredFunctionNames.Add(functionDefinitionAst.Name);
            }

            // Finds all CommandAsts.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // Iterates all CommandAsts and check the command name.
            foreach (Ast foundAst in foundAsts)
            {
                CommandAst cmdAst = (CommandAst)foundAst;
                // Handles the exception caused by commands like, {& $PLINK $args 2> $TempErrorFile}.
                // You can also review the remark section in following document,
                // MSDN: CommandAst.GetCommandName Method
                if (cmdAst.GetCommandName() == null) continue;

                throw new NotImplementedException();
                
                /*
                if ((Helper.Instance.IsKnownCmdletFunctionOrExternalScript(cmdAst) || declaredFunctionNames.Contains(cmdAst.GetCommandName())) &&
                    (Helper.Instance.PositionalParameterUsed(cmdAst, true)))
                {
                    PipelineAst parent = cmdAst.Parent as PipelineAst;

                    if (parent != null && parent.PipelineElements.Count > 1)
                    {
                        // raise if it's the first element in pipeline. otherwise no.
                        if (parent.PipelineElements[0] == cmdAst)
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersError, cmdAst.GetCommandName()),
                                cmdAst.Extent, GetName(), DiagnosticSeverity.Information, fileName, cmdAst.GetCommandName());
                        }
                    }
                    // not in pipeline so just raise it normally
                    else
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersError, cmdAst.GetCommandName()),
                            cmdAst.Extent, GetName(), DiagnosticSeverity.Information, fileName, cmdAst.GetCommandName());
                    }
                }
                */
            }

            yield break;
        }
    }
}

