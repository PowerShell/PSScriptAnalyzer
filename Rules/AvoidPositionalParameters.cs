// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Linq;
using System.Management.Automation;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidPositionalParameters: Check to make sure that positional parameters are not used.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidPositionalParameters : ConfigurableRule
    {
        [ConfigurableRuleProperty(defaultValue: new string[] { })]
        public string[] CommandAllowList { get; set; }

        public AvoidPositionalParameters()
        {
            Enable = true; // keep it enabled by default, user can still override this with settings
        }

        /// <summary>
        /// AnalyzeScript: Analyze the ast to check that positional parameters are not used.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all function definitions in the script and add them to the set.
            IEnumerable<Ast> functionDefinitionAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            HashSet<String> declaredFunctionNames = new HashSet<String>();

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
                
                if ((Helper.Instance.IsKnownCmdletFunctionOrExternalScript(cmdAst, out CommandInfo commandInfo) || declaredFunctionNames.Contains(cmdAst.GetCommandName())) &&
                    (Helper.Instance.PositionalParameterUsed(cmdAst, true)))
                {
                    if (commandInfo?.CommandType == CommandTypes.Application) continue;

                    PipelineAst parent = cmdAst.Parent as PipelineAst;

                    string commandName = cmdAst.GetCommandName();
                    if (parent != null && parent.PipelineElements.Count > 1)
                    {
                        // raise if it's the first element in pipeline. otherwise no.
                        if (parent.PipelineElements[0] == cmdAst && !CommandAllowList.Contains(commandName, StringComparer.OrdinalIgnoreCase))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersError, commandName),
                                cmdAst.Extent, GetName(), DiagnosticSeverity.Information, fileName, commandName);
                        }
                    }
                    // not in pipeline so just raise it normally
                    else
                    {
                        if (!CommandAllowList.Contains(commandName, StringComparer.OrdinalIgnoreCase))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersError, commandName),
                                cmdAst.Extent, GetName(), DiagnosticSeverity.Information, fileName, commandName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingPositionalParametersName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingPositionalParametersDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Information;
        }

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




