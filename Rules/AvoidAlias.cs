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
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidAlias: Check if cmdlet alias is used.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidAlias : IScriptRule
    {
        private readonly string whiteListArgName = "whitelist";
        private bool isPropertiesSet;
        private List<string> whiteList;
        public List<string> WhiteList
        {
            get { return whiteList; }
        }

        public AvoidAlias()
        {
            isPropertiesSet = false;
        }

        /// <summary>
        /// Configure the rule.
        ///
        /// Sets the whitelist of this rule
        /// </summary>
        private void SetProperties()
        {
            whiteList = new List<string>();
            isPropertiesSet = true;
            Dictionary<string, object> ruleArgs = Helper.Instance.GetRuleArguments(GetName());
            if (ruleArgs == null)
            {
                return;
            }
            object obj;
            if (!ruleArgs.TryGetValue(whiteListArgName, out obj))
            {
                return;
            }
            IEnumerable<string> aliases = obj as IEnumerable<string>;
            if (aliases == null)
            {
                // try with enumerable objects
                var enumerableObjs = obj as IEnumerable<object>;
                if (enumerableObjs == null)
                {
                    return;
                }
                foreach (var x in enumerableObjs)
                {
                    var y = x as string;
                    if (y == null)
                    {
                        return;
                    }
                    else
                    {
                        whiteList.Add(y);
                    }
                }
            }
            else
            {
                whiteList.AddRange(aliases);
            }
        }

        /// <summary>
        /// AnalyzeScript: Analyze the script to check if cmdlet alias is used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);
            if (!isPropertiesSet)
            {
                SetProperties();
            }
            // Finds all CommandAsts.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // Iterates all CommandAsts and check the command name.
            foreach (CommandAst cmdAst in foundAsts)
            {
                // Check if the command ast should be ignored
                if (IgnoreCommandast(cmdAst))
                {
                    continue;
                }

                string commandName = cmdAst.GetCommandName();

                // Handles the exception caused by commands like, {& $PLINK $args 2> $TempErrorFile}.
                // You can also review the remark section in following document,
                // MSDN: CommandAst.GetCommandName Method
                if (commandName == null
                    || whiteList.Contains<string>(commandName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                string cmdletNameIfCommandNameWasAlias = Helper.Instance.GetCmdletNameFromAlias(commandName);
                if (!String.IsNullOrEmpty(cmdletNameIfCommandNameWasAlias))
                {
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingCmdletAliasesError, commandName, cmdletNameIfCommandNameWasAlias),
                        GetCommandExtent(cmdAst),
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        commandName,
                        suggestedCorrections: GetCorrectionExtent(cmdAst, cmdletNameIfCommandNameWasAlias));
                }

                var isNativeCommand = Helper.Instance.GetCommandInfo(commandName, CommandTypes.Application | CommandTypes.ExternalScript) != null;
                if (!isNativeCommand)
                {
                    var commdNameWithGetPrefix = $"Get-{commandName}";
                    var cmdletNameIfCommandWasMissingGetPrefix = Helper.Instance.GetCommandInfo($"Get-{commandName}");
                    if (cmdletNameIfCommandWasMissingGetPrefix != null)
                    {
                        yield return new DiagnosticRecord(
                            string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingCmdletAliasesMissingGetPrefixError, commandName, commdNameWithGetPrefix),
                            GetCommandExtent(cmdAst),
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName,
                            commandName,
                            suggestedCorrections: GetCorrectionExtent(cmdAst, commdNameWithGetPrefix));
                    }
                }
            }
        }

        /// <summary>
        /// Checks commandast of the form "[commandElement0] = [CommandElement2]". This typically occurs in a DSC configuration.
        /// </summary>
        private bool IgnoreCommandast(CommandAst cmdAst)
        {
            if (cmdAst.CommandElements.Count == 3)
            {
                var element = cmdAst.CommandElements[1] as StringConstantExpressionAst;
                if (element != null && element.Value.Equals("="))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// For a command like "gci -path c:", returns the extent of "gci" in the command
        /// </summary>
        private IScriptExtent GetCommandExtent(CommandAst commandAst)
        {
            var cmdName = commandAst.GetCommandName();
            foreach (var cmdElement in commandAst.CommandElements)
            {
                var stringConstExpressinAst = cmdElement as StringConstantExpressionAst;
                if (stringConstExpressinAst != null)
                {
                    if (stringConstExpressinAst.Value.Equals(cmdName))
                    {
                        return stringConstExpressinAst.Extent;
                    }
                }
            }
            return commandAst.Extent;
        }

        /// <summary>
        /// Creates a list containing suggested correction
        /// </summary>
        /// <param name="cmdAst">Command AST of an alias</param>
        /// <param name="cmdletName">Full name of the alias</param>
        /// <returns>Retruns a list of suggested corrections</returns>
        private List<CorrectionExtent> GetCorrectionExtent(CommandAst cmdAst, string cmdletName)
        {
            var corrections = new List<CorrectionExtent>();
            var alias = cmdAst.GetCommandName();
            var description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.AvoidUsingCmdletAliasesCorrectionDescription,
                alias,
                cmdletName);
            var cmdExtent = GetCommandExtent(cmdAst);
            corrections.Add(new CorrectionExtent(
                cmdExtent.StartLineNumber,
                cmdExtent.EndLineNumber,
                cmdExtent.StartColumnNumber,
                cmdExtent.EndColumnNumber,
                cmdletName,
                cmdAst.Extent.File,
                description));
            return corrections;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingCmdletAliasesName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingCmdletAliasesCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingCmdletAliasesDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
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
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}