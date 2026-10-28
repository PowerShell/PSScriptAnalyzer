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
        private readonly string allowListArgName = "allowlist";
        // keep legacy argument name for next version to allow customers to transition but remove later
        private readonly string allowListLegacyArgName = "whitelist";
        private bool isPropertiesSet;
        public List<string> AllowList { get; private set; }

        public AvoidAlias()
        {
            isPropertiesSet = false;
        }

        /// <summary>
        /// Configure the rule.
        ///
        /// Sets the allowlist of this rule
        /// </summary>
        private void SetProperties()
        {
            AllowList = new List<string>();
            isPropertiesSet = true;
            Dictionary<string, object> ruleArgs = Helper.Instance.GetRuleArguments(GetName());
            if (ruleArgs == null)
            {
                return;
            }
            object objLegacy = null;
            if (!ruleArgs.TryGetValue(allowListArgName, out object obj) &&
                !ruleArgs.TryGetValue(allowListLegacyArgName, out objLegacy))
            {
                return;
            }
            // Fallback for object from legacy allowlist argument name 
            if (obj == null) {
                obj = objLegacy;
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
                        AllowList.Add(y);
                    }
                }
            }
            else
            {
                AllowList.AddRange(aliases);
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
                    || AllowList.Contains<string>(commandName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                string cmdletNameIfCommandNameWasAlias = Helper.Instance.GetCmdletNameFromAlias(commandName);
                bool isDscResourceDeclaration = IsDscResourceDeclaration(cmdAst);
                if (isDscResourceDeclaration
                    && (String.IsNullOrEmpty(cmdletNameIfCommandNameWasAlias) || cmdAst.CommandElements.Count == 2))
                {
                    continue;
                }

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
                    // do not continue the search, but go to the next command
                    continue;
                }

                // If we find match of any kind, do not continue with the Get-{commandname} check
                if ( Helper.Instance.GetCommandInfo(commandName) != null ) {
                    continue;
                }

                var commdNameWithGetPrefix = $"Get-{commandName}";
                var cmdletNameIfCommandWasMissingGetPrefix = Helper.Instance.GetCommandInfo(name: commdNameWithGetPrefix,
                    commandType: CommandTypes.Cmdlet | CommandTypes.Function | CommandTypes.Script);
                if (cmdletNameIfCommandWasMissingGetPrefix != null)
                {
                    if (commandName.Equals("process", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new DiagnosticRecord(
                            Strings.InvalidSyntaxAroundProcessBlockError,
                            GetCommandExtent(cmdAst),
                            "InvalidSyntaxAroundProcessBlock",
                            DiagnosticSeverity.ParseError,
                            fileName,
                            commandName);
                    }
                    else
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

        private bool IsDscResourceDeclaration(CommandAst cmdAst)
        {
            bool hasConfigurationAncestor = false;
            for (Ast parent = cmdAst.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ConfigurationDefinitionAst)
                {
                    hasConfigurationAncestor = true;
                    break;
                }

                if (parent is FunctionDefinitionAst
                    || parent is HashtableAst
                    || parent is AssignmentStatementAst
                    || parent is ParenExpressionAst
                    || parent is SubExpressionAst
                    || parent is ArrayExpressionAst)
                {
                    return false;
                }
            }

            if (!hasConfigurationAncestor || GetDscResourceBody(cmdAst) == null)
            {
                return false;
            }

            for (Ast parent = cmdAst.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ConfigurationDefinitionAst)
                {
                    return true;
                }

                if (parent is FunctionDefinitionAst
                    || parent is HashtableAst
                    || parent is AssignmentStatementAst
                    || parent is ParenExpressionAst
                    || parent is SubExpressionAst
                    || parent is ArrayExpressionAst)
                {
                    return false;
                }

                if (parent is CommandExpressionAst commandExpression
                    && (!(commandExpression.Expression is ScriptBlockExpressionAst body)
                        || !IsFollowingLineDscResourceBody(body)))
                {
                    return false;
                }

                // Script resource properties contain executable code, not resource declarations.
                if (parent is ScriptBlockExpressionAst scriptBlock)
                {
                    if (scriptBlock.Parent is CommandAst enclosingCommand)
                    {
                        if (GetDscResourceBody(enclosingCommand) != scriptBlock
                            || !String.IsNullOrEmpty(Helper.Instance.GetCmdletNameFromAlias(enclosingCommand.GetCommandName()))
                            || (!String.Equals(enclosingCommand.GetCommandName(), "Node", StringComparison.OrdinalIgnoreCase)
                                && Helper.Instance.GetCommandInfo(enclosingCommand.GetCommandName()) != null))
                        {
                            return false;
                        }
                    }
                    else if (!(scriptBlock.Parent is ConfigurationDefinitionAst)
                        && !(scriptBlock.Parent is DynamicKeywordStatementAst)
                        && !(scriptBlock.Parent is CommandExpressionAst))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private bool IsFollowingLineDscResourceBody(ScriptBlockExpressionAst body)
        {
            if (!(body.Parent is CommandExpressionAst bodyExpression)
                || !(bodyExpression.Parent is PipelineAst bodyPipeline)
                || bodyPipeline.PipelineElements.Count != 1
                || bodyPipeline.PipelineElements[0] != bodyExpression)
            {
                return false;
            }

            Ast statement = bodyPipeline;
            for (Ast parent = bodyPipeline.Parent; parent != null; parent = parent.Parent)
            {
                var statements = (parent as NamedBlockAst)?.Statements
                    ?? (parent as StatementBlockAst)?.Statements;
                if (statements == null)
                {
                    statement = parent;
                    continue;
                }

                int index = statements.IndexOf(statement as StatementAst);
                if (index <= 0 || !(statements[index - 1] is PipelineAst declarationPipeline)
                    || declarationPipeline.PipelineElements.Count != 1
                    || !(declarationPipeline.PipelineElements[0] is CommandAst declaration))
                {
                    return false;
                }

                return GetDscResourceBody(declaration) == body;
            }

            return false;
        }

        private ScriptBlockExpressionAst GetDscResourceBody(CommandAst cmdAst)
        {
            if (cmdAst.InvocationOperator != TokenKind.Unknown
                || cmdAst.CommandElements.Count < 2
                || !(cmdAst.CommandElements[1] is ExpressionAst)
                || cmdAst.CommandElements[1].Extent.Text == "="
                || cmdAst.Redirections.Count != 0)
            {
                return null;
            }

            if (!(cmdAst.Parent is PipelineAst commandPipeline)
                || commandPipeline.PipelineElements.Count != 1
                || commandPipeline.PipelineElements[0] != cmdAst
                || !(commandPipeline.Parent is NamedBlockAst || commandPipeline.Parent is StatementBlockAst))
            {
                return null;
            }

            if (cmdAst.CommandElements.Count == 3)
            {
                return cmdAst.CommandElements[2] as ScriptBlockExpressionAst;
            }

            // A resource's opening brace can be on the following line.
            if (cmdAst.CommandElements.Count == 2 && cmdAst.Parent is PipelineAst pipeline)
            {
                Ast statement = pipeline;
                for (Ast parent = pipeline.Parent; parent != null; parent = parent.Parent)
                {
                    var statements = (parent as NamedBlockAst)?.Statements
                        ?? (parent as StatementBlockAst)?.Statements;
                    if (statements == null)
                    {
                        statement = parent;
                        continue;
                    }

                    int index = statements.IndexOf(statement as StatementAst);
                    if (index >= 0 && index + 1 < statements.Count
                        && statements[index + 1] is PipelineAst bodyPipeline
                        && bodyPipeline.PipelineElements.Count == 1
                        && bodyPipeline.PipelineElements[0] is CommandExpressionAst bodyExpression)
                    {
                        if (HasStatementSeparator(pipeline, bodyPipeline)
                            || bodyExpression.Redirections.Count != 0)
                        {
                            return null;
                        }

                        return bodyExpression.Expression as ScriptBlockExpressionAst;
                    }

                    break;
                }
            }

            return null;
        }

        private bool HasStatementSeparator(Ast statement, Ast nextStatement)
        {
            int separatorStart = statement.Extent.EndOffset;
            int separatorLength = nextStatement.Extent.StartOffset - separatorStart;
            if (separatorLength <= 0)
            {
                return false;
            }

            string script = statement.Extent.EndScriptPosition.GetFullScript();
            string separator = script.Substring(separatorStart, separatorLength);
            if (separator.IndexOf(';') < 0 && separator.IndexOf('&') < 0)
            {
                return false;
            }

            Parser.ParseInput(separator, out Token[] tokens, out ParseError[] errors);
            return tokens.Any(token => token.Kind == TokenKind.Semi || token.Kind == TokenKind.Ampersand);
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
