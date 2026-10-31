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
        private sealed class FunctionAnalysisContext
        {
            public Dictionary<ScriptBlockAst, Dictionary<string, List<FunctionDefinitionAst>>> FunctionNameIndex { get; set; }

            public Dictionary<CommandAst, bool?> ScriptDefinedFunctionCache { get; } = new Dictionary<CommandAst, bool?>();

            public Dictionary<ScriptBlockAst, bool?> CallerScopedScriptBlockCache { get; } = new Dictionary<ScriptBlockAst, bool?>();

            public Dictionary<ScriptBlockAst, bool?> IsolatedRunspaceScriptBlockCache { get; } = new Dictionary<ScriptBlockAst, bool?>();

            public void ClearCaches()
            {
                ScriptDefinedFunctionCache.Clear();
                CallerScopedScriptBlockCache.Clear();
                IsolatedRunspaceScriptBlockCache.Clear();
            }
        }

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
            FunctionAnalysisContext functionAnalysis = GetFunctionNameIndex(ast);

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
                bool isDscResourceDeclaration = IsDscResourceDeclaration(cmdAst, functionAnalysis);
                if (isDscResourceDeclaration && String.IsNullOrEmpty(cmdletNameIfCommandNameWasAlias))
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
                if (Helper.Instance.GetCommandInfo(commandName) != null || IsScriptDefinedFunction(cmdAst, functionAnalysis)) {
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

        private bool IsDscResourceDeclaration(
            CommandAst cmdAst,
            FunctionAnalysisContext functionAnalysis)
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

            if (!hasConfigurationAncestor || GetDscResourceBody(cmdAst) == null
                || !IsDscResourceCommand(cmdAst, functionAnalysis))
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
                        || !IsFollowingLineDscResourceBody(body, functionAnalysis)))
                {
                    return false;
                }

                // Script resource properties contain executable code, not resource declarations.
                if (parent is ScriptBlockExpressionAst scriptBlock)
                {
                    if (scriptBlock.Parent is CommandAst enclosingCommand)
                    {
                        if (GetDscResourceBody(enclosingCommand) != scriptBlock
                            || !IsDscResourceCommand(enclosingCommand, functionAnalysis))
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

        private bool IsFollowingLineDscResourceBody(
            ScriptBlockExpressionAst body,
            FunctionAnalysisContext functionAnalysis)
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

                return GetDscResourceBody(declaration) == body
                    && IsDscResourceCommand(declaration, functionAnalysis);
            }

            return false;
        }

        private bool IsDscResourceCommand(
            CommandAst cmdAst,
            FunctionAnalysisContext functionAnalysis)
        {
            string commandName = cmdAst.GetCommandName();
            return !IsScriptDefinedFunction(cmdAst, functionAnalysis)
                && String.IsNullOrEmpty(Helper.Instance.GetCmdletNameFromAlias(commandName))
                && (String.Equals(commandName, "Node", StringComparison.OrdinalIgnoreCase)
                    || Helper.Instance.GetCommandInfo(commandName) == null);
        }

        private bool IsScriptDefinedFunction(
            CommandAst cmdAst,
            FunctionAnalysisContext functionAnalysis)
        {
            string commandName = cmdAst.GetCommandName();
            if (commandName == null)
            {
                return false;
            }

            if (!String.IsNullOrEmpty(Helper.Instance.GetCmdletNameFromAlias(commandName)))
            {
                return false;
            }

            if (functionAnalysis.ScriptDefinedFunctionCache.TryGetValue(cmdAst, out bool? cachedResult))
            {
                return cachedResult.GetValueOrDefault();
            }

            functionAnalysis.ScriptDefinedFunctionCache[cmdAst] = null;
            ScriptBlockAst currentScope = null;
            for (Ast parent = cmdAst.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ScriptBlockAst scope)
                {
                    if (currentScope == null && !IsCallerScopedScriptBlock(scope, functionAnalysis))
                    {
                        currentScope = scope;
                    }

                    if (functionAnalysis.FunctionNameIndex.TryGetValue(
                        scope,
                        out Dictionary<string, List<FunctionDefinitionAst>> functionNames)
                        && functionNames.TryGetValue(commandName, out List<FunctionDefinitionAst> functionDefinitions)
                        && functionDefinitions.Any(function =>
                            !IsDescendantOf(function, cmdAst)
                            && (!function.Name.StartsWith("private:", StringComparison.OrdinalIgnoreCase)
                                || currentScope == scope)
                            && !IsFunctionDeclaredAfterCommand(function, cmdAst, currentScope, functionAnalysis)))
                    {
                        functionAnalysis.ScriptDefinedFunctionCache[cmdAst] = true;
                        return true;
                    }

                    if (IsIsolatedRunspaceScriptBlock(scope, functionAnalysis))
                    {
                        break;
                    }
                }
            }

            functionAnalysis.ScriptDefinedFunctionCache[cmdAst] = false;
            return false;
        }

        private bool IsFunctionDeclaredAfterCommand(
            FunctionDefinitionAst function,
            CommandAst cmdAst,
            ScriptBlockAst commandScope,
            FunctionAnalysisContext functionAnalysis)
        {
            if (commandScope == null)
            {
                return false;
            }

            ScriptBlockAst functionScope = GetFunctionScope(function, functionAnalysis);
            if (functionScope == null)
            {
                return false;
            }

            CommandAst immediateInvocation = null;
            if (commandScope != functionScope
                && !IsImmediatelyInvokedChildScope(commandScope, functionScope, out immediateInvocation))
            {
                return false;
            }

            int functionNamedBlockPhase = GetNamedBlockExecutionPhase(function, out FunctionDefinitionAst functionOwner);
            int commandNamedBlockPhase = GetNamedBlockExecutionPhase(cmdAst, out FunctionDefinitionAst commandOwner);
            if (functionOwner != null
                && functionOwner == commandOwner
                && functionNamedBlockPhase >= 0
                && commandNamedBlockPhase >= 0
                && functionNamedBlockPhase != commandNamedBlockPhase)
            {
                return functionNamedBlockPhase > commandNamedBlockPhase;
            }

            int functionPhase = GetForEachObjectExecutionPhase(function, out CommandAst functionCallback);
            int commandPhase = GetForEachObjectExecutionPhase(cmdAst, out CommandAst commandCallback);
            if (functionCallback == commandCallback
                && functionPhase >= 0
                && commandPhase >= 0
                && functionPhase != commandPhase)
            {
                return functionPhase > commandPhase;
            }

            int commandExecutionOffset = immediateInvocation == null
                ? cmdAst.Extent.StartOffset
                : immediateInvocation.Extent.StartOffset;
            return function.Extent.StartOffset > commandExecutionOffset;
        }

        private bool IsImmediatelyInvokedChildScope(
            ScriptBlockAst commandScope,
            ScriptBlockAst functionScope,
            out CommandAst invocation)
        {
            invocation = null;
            if (!(commandScope.Parent is ScriptBlockExpressionAst scriptBlock))
            {
                return false;
            }

            CommandAst command = GetScriptBlockCommand(scriptBlock);
            if (command == null || command.InvocationOperator != TokenKind.Ampersand)
            {
                return false;
            }

            ScriptBlockAst parentScope = null;
            for (Ast parent = commandScope.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ScriptBlockAst scope)
                {
                    parentScope = scope;
                    break;
                }
            }

            if (parentScope != functionScope)
            {
                return false;
            }

            invocation = command;
            return true;
        }

        private int GetNamedBlockExecutionPhase(Ast ast, out FunctionDefinitionAst containingFunction)
        {
            NamedBlockAst namedBlock = null;
            containingFunction = null;
            for (Ast current = ast; current != null; current = current.Parent)
            {
                if (namedBlock == null && current is NamedBlockAst block)
                {
                    namedBlock = block;
                }

                if (current != ast && current is FunctionDefinitionAst function)
                {
                    containingFunction = function;
                    break;
                }
            }

            if (namedBlock == null || containingFunction == null)
            {
                return -1;
            }

            switch (namedBlock.BlockKind)
            {
                case TokenKind.Begin:
                    return 0;
                case TokenKind.Process:
                    return 1;
                case TokenKind.End:
                    return 2;
                default:
                    return -1;
            }
        }

        private int GetForEachObjectExecutionPhase(Ast ast, out CommandAst callbackCommand)
        {
            callbackCommand = null;
            for (Ast current = ast; current != null; current = current.Parent)
            {
                if (!(current is ScriptBlockAst scope)
                    || !(scope.Parent is ScriptBlockExpressionAst scriptBlock))
                {
                    continue;
                }

                CommandAst commandAst = GetScriptBlockCommand(scriptBlock);
                if (commandAst == null
                    || !IsForEachObjectCommand(GetCallbackCommandName(commandAst)))
                {
                    continue;
                }

                string parameterName = GetScriptBlockParameterName(commandAst, scriptBlock);
                if (IsParameter(parameterName, "Begin"))
                {
                    callbackCommand = commandAst;
                    return 0;
                }

                if (IsParameter(parameterName, "Process"))
                {
                    callbackCommand = commandAst;
                    return 1;
                }

                if (IsParameter(parameterName, "End"))
                {
                    callbackCommand = commandAst;
                    return 2;
                }
            }

            return -1;
        }

        private FunctionAnalysisContext GetFunctionNameIndex(Ast ast)
        {
            FunctionDefinitionAst[] functionDefinitions = ast.FindAll(
                candidate => candidate is FunctionDefinitionAst,
                true).OfType<FunctionDefinitionAst>().ToArray();

            var functionAnalysis = new FunctionAnalysisContext
            {
                FunctionNameIndex = new Dictionary<ScriptBlockAst, Dictionary<string, List<FunctionDefinitionAst>>>()
            };

            foreach (FunctionDefinitionAst function in functionDefinitions)
            {
                if (IsInConstantFalseBranch(function))
                {
                    continue;
                }

                ScriptBlockAst scope = GetNearestFunctionScope(function);
                if (scope != null)
                {
                    AddFunctionToIndex(functionAnalysis.FunctionNameIndex, scope, function);
                }
            }

            var previousScopes = new Dictionary<FunctionDefinitionAst, ScriptBlockAst>();
            foreach (FunctionDefinitionAst function in functionDefinitions)
            {
                if (!IsInConstantFalseBranch(function))
                {
                    previousScopes[function] = GetNearestFunctionScope(function);
                }
            }

            for (int iteration = 0; iteration <= previousScopes.Count; iteration++)
            {
                functionAnalysis.ClearCaches();
                var functionNameIndex = new Dictionary<ScriptBlockAst, Dictionary<string, List<FunctionDefinitionAst>>>();
                var currentScopes = new Dictionary<FunctionDefinitionAst, ScriptBlockAst>();
                foreach (FunctionDefinitionAst function in previousScopes.Keys)
                {
                    ScriptBlockAst scope = GetFunctionScope(function, functionAnalysis);
                    currentScopes[function] = scope;
                    if (scope != null)
                    {
                        AddFunctionToIndex(functionNameIndex, scope, function);
                    }
                }

                bool scopesChanged = previousScopes.Any(pair => currentScopes[pair.Key] != pair.Value);
                functionAnalysis.FunctionNameIndex = functionNameIndex;
                previousScopes = currentScopes;
                if (!scopesChanged)
                {
                    break;
                }
            }

            functionAnalysis.ClearCaches();
            return functionAnalysis;
        }

        private void AddFunctionToIndex(
            Dictionary<ScriptBlockAst, Dictionary<string, List<FunctionDefinitionAst>>> functionNameIndex,
            ScriptBlockAst scope,
            FunctionDefinitionAst function)
        {
            string functionName = Helper.Instance.FunctionNameWithoutScope(function.Name);
            if (String.IsNullOrEmpty(functionName))
            {
                return;
            }

            if (!functionNameIndex.TryGetValue(
                scope,
                out Dictionary<string, List<FunctionDefinitionAst>> functionNames))
            {
                functionNames = new Dictionary<string, List<FunctionDefinitionAst>>(StringComparer.OrdinalIgnoreCase);
                functionNameIndex.Add(scope, functionNames);
            }

            if (functionNames.TryGetValue(functionName, out List<FunctionDefinitionAst> functionDefinitions))
            {
                functionDefinitions.Add(function);
            }
            else
            {
                functionNames.Add(functionName, new List<FunctionDefinitionAst> { function });
            }
        }

        private ScriptBlockAst GetNearestFunctionScope(FunctionDefinitionAst function)
        {
            for (Ast parent = function.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ScriptBlockAst scope)
                {
                    return scope;
                }
            }

            return null;
        }

        private ScriptBlockAst GetFunctionScope(
            FunctionDefinitionAst function,
            FunctionAnalysisContext functionAnalysis)
        {
            for (Ast parent = function.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ScriptBlockAst scope
                    && !IsCallerScopedScriptBlock(scope, functionAnalysis))
                {
                    return scope;
                }
            }

            return null;
        }

        private bool IsCallerScopedScriptBlock(
        ScriptBlockAst scope,
        FunctionAnalysisContext functionAnalysis)
        {
        if (functionAnalysis.CallerScopedScriptBlockCache.TryGetValue(scope, out bool? cachedResult))
        {
            return cachedResult.GetValueOrDefault();
        }

        functionAnalysis.CallerScopedScriptBlockCache[scope] = null;
        bool result = false;
        if (!(scope.Parent is ScriptBlockExpressionAst scriptBlock))
        {
            functionAnalysis.CallerScopedScriptBlockCache[scope] = result;
            return result;
        }

        CommandAst commandAst = GetScriptBlockCommand(scriptBlock);
        if (commandAst == null || IsScriptDefinedFunction(commandAst, functionAnalysis))
        {
            functionAnalysis.CallerScopedScriptBlockCache[scope] = result;
            return result;
        }

        if (commandAst.InvocationOperator == TokenKind.Dot
            && commandAst.CommandElements[0] == scriptBlock)
        {
            result = true;
        }
        else
        {
            string commandName = GetCallbackCommandName(commandAst);
            if (IsForEachObjectCommand(commandName))
            {
                if (HasCommandParameter(commandAst, "MemberName")
                    || HasPositionalCommandParameter(commandAst, scriptBlock, "MemberName"))
                {
                    functionAnalysis.CallerScopedScriptBlockCache[scope] = result;
                    return result;
                }

                string parameterName = GetScriptBlockParameterName(commandAst, scriptBlock);
                result = !IsParameter(parameterName, "Parallel")
                    && (IsParameter(parameterName, "Begin")
                        || IsParameter(parameterName, "Process")
                        || IsParameter(parameterName, "End")
                        || IsParameter(parameterName, "RemainingScripts"));
            }
            else if (IsWhereObjectCommand(commandName))
            {
                result = IsParameter(GetScriptBlockParameterName(commandAst, scriptBlock), "FilterScript");
            }
            else if (IsInvokeCommand(commandName)
                && IsParameter(GetScriptBlockParameterName(commandAst, scriptBlock), "ScriptBlock"))
            {
                result = HasEnabledCommandParameter(commandAst, "NoNewScope")
                    && !IsRemoteInvokeCommand(commandAst);
            }
            }

        functionAnalysis.CallerScopedScriptBlockCache[scope] = result;
        return result;
        }

        private bool IsIsolatedRunspaceScriptBlock(
        ScriptBlockAst scope,
        FunctionAnalysisContext functionAnalysis)
        {
        if (functionAnalysis.IsolatedRunspaceScriptBlockCache.TryGetValue(scope, out bool? cachedResult))
        {
            return cachedResult.GetValueOrDefault();
        }

        functionAnalysis.IsolatedRunspaceScriptBlockCache[scope] = null;
        bool result = false;
        if (!(scope.Parent is ScriptBlockExpressionAst scriptBlock))
        {
            functionAnalysis.IsolatedRunspaceScriptBlockCache[scope] = result;
            return result;
        }

        CommandAst commandAst = GetScriptBlockCommand(scriptBlock);
        if (commandAst == null)
        {
            commandAst = GetPrecedingDscScriptPropertyCommand(scriptBlock);
        }

        if (commandAst == null || IsScriptDefinedFunction(commandAst, functionAnalysis))
        {
            functionAnalysis.IsolatedRunspaceScriptBlockCache[scope] = result;
            return result;
        }

        string commandName = GetCallbackCommandName(commandAst);
        string parameterName = GetScriptBlockParameterName(commandAst, scriptBlock);
        if (IsForEachObjectCommand(commandName) && IsParameter(parameterName, "Parallel"))
        {
            result = true;
        }
        else if ((String.Equals(commandName, "Start-Job", StringComparison.OrdinalIgnoreCase)
                || String.Equals(commandName, "Start-ThreadJob", StringComparison.OrdinalIgnoreCase))
            && (IsParameter(parameterName, "ScriptBlock")
                || IsParameter(parameterName, "InitializationScript")))
        {
            result = true;
        }
        else if (String.Equals(commandName, "Invoke-Command", StringComparison.OrdinalIgnoreCase)
            && IsParameter(parameterName, "ScriptBlock")
            && IsRemoteInvokeCommand(commandAst))
        {
            result = true;
        }
        else if (String.Equals(commandName, "InlineScript", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
        }
        else
        {
            result = (String.Equals(commandName, "GetScript", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(commandName, "TestScript", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(commandName, "SetScript", StringComparison.OrdinalIgnoreCase))
                && commandAst.CommandElements.Count > 1
                && String.Equals(commandAst.CommandElements[1].ToString(), "=", StringComparison.Ordinal);
        }

        functionAnalysis.IsolatedRunspaceScriptBlockCache[scope] = result;
        return result;
        }

        private CommandAst GetPrecedingDscScriptPropertyCommand(ScriptBlockExpressionAst scriptBlock)
        {
        if (!(scriptBlock.Parent is CommandExpressionAst commandExpression)
            || !(commandExpression.Parent is PipelineAst pipeline))
        {
            return null;
        }

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
            if (index <= 0
                || !(statements[index - 1] is PipelineAst propertyPipeline)
                || propertyPipeline.PipelineElements.Count != 1
                || !(propertyPipeline.PipelineElements[0] is CommandAst propertyCommand)
                || propertyCommand.CommandElements.Count != 2
                || !String.Equals(propertyCommand.CommandElements[1].ToString(), "=", StringComparison.Ordinal))
            {
                return null;
            }

            string propertyName = propertyCommand.GetCommandName();
            return String.Equals(propertyName, "GetScript", StringComparison.OrdinalIgnoreCase)
                || String.Equals(propertyName, "TestScript", StringComparison.OrdinalIgnoreCase)
                || String.Equals(propertyName, "SetScript", StringComparison.OrdinalIgnoreCase)
                    ? propertyCommand
                    : null;
        }

        return null;
        }

        private CommandAst GetScriptBlockCommand(ScriptBlockExpressionAst scriptBlock)
        {
            Ast current = scriptBlock;
            while (current.Parent != null)
            {
                if (current.Parent is CommandAst commandAst)
                {
                    return commandAst;
                }

                if (current.Parent is ArrayLiteralAst arrayLiteral)
                {
                    current = arrayLiteral;
                    continue;
                }

                if (current.Parent is ArrayExpressionAst arrayExpression)
                {
                    current = arrayExpression;
                    continue;
                }

                if (current.Parent is ParenExpressionAst
                    || current.Parent is CommandExpressionAst
                    || current.Parent is PipelineAst
                    || current.Parent is StatementBlockAst)
                {
                    current = current.Parent;
                    continue;
                }

                if (current.Parent is CommandParameterAst parameter
                    && parameter.Argument == current)
                {
                    current = parameter;
                    continue;
                }

                return null;
            }

            return null;
        }

        private string GetCallbackCommandName(CommandAst commandAst)
        {
            string commandName = commandAst.GetCommandName();
            if (commandName == null)
            {
                return null;
            }

            const string coreModulePrefix = "Microsoft.PowerShell.Core\\";
            if (commandName.StartsWith(coreModulePrefix, StringComparison.OrdinalIgnoreCase))
            {
                commandName = commandName.Substring(coreModulePrefix.Length);
            }
            else if (commandName.EndsWith("\\Start-ThreadJob", StringComparison.OrdinalIgnoreCase))
            {
                commandName = "Start-ThreadJob";
            }
            else if (commandName.IndexOf('\\') >= 0)
            {
                return null;
            }

            string cmdletName = Helper.Instance.GetCmdletNameFromAlias(commandName);
            return String.IsNullOrEmpty(cmdletName) ? commandName : cmdletName;
        }

        private string GetScriptBlockParameterName(
            CommandAst commandAst,
            ScriptBlockExpressionAst scriptBlock)
        {
            CommandInfo commandInfo = Helper.Instance.GetCommandInfo(GetCallbackCommandName(commandAst));
            if (commandInfo == null)
            {
                return null;
            }

            Dictionary<string, ParameterMetadata> parameters = GetCommandParameters(commandInfo);

            CommandParameterAst attachedParameter = commandAst.CommandElements
                .OfType<CommandParameterAst>()
                .FirstOrDefault(parameter => parameter.Argument != null
                    && (parameter.Argument == scriptBlock
                        || IsDescendantOf(scriptBlock, parameter.Argument)));
            if (parameters == null)
            {
                return IsInvokeCommand(GetCallbackCommandName(commandAst))
                    && attachedParameter != null
                    && IsInvokeCommandScriptBlockParameter(attachedParameter.ParameterName)
                        ? "ScriptBlock"
                        : null;
            }

            if (attachedParameter != null)
            {
                return ResolveParameter(parameters, attachedParameter.ParameterName)?.Name;
            }

            if (IsWhereObjectCommand(GetCallbackCommandName(commandAst))
                && commandAst.CommandElements
                    .OfType<CommandParameterAst>()
                    .Any(parameter => IsParameter(
                        ResolveParameter(parameters, parameter.ParameterName)?.Name,
                        "Property")))
            {
                return null;
            }

            CommandElementAst argumentElement = commandAst.CommandElements
                .FirstOrDefault(element => element == scriptBlock
                    || IsDescendantOf(scriptBlock, element));
            int scriptBlockIndex = argumentElement == null
                ? -1
                : commandAst.CommandElements.IndexOf(argumentElement);
            if (scriptBlockIndex < 0)
            {
                return null;
            }

            int positionalIndex = 0;
            for (int i = 1; i <= scriptBlockIndex; i++)
            {
                CommandElementAst commandElement = commandAst.CommandElements[i];
                if (i == scriptBlockIndex)
                {
                    return GetPositionalScriptBlockParameter(parameters, positionalIndex)?.Name;
                }

                if (!(commandElement is CommandParameterAst parameter))
                {
                    positionalIndex++;
                    continue;
                }

                ParameterMetadata parameterMetadata = ResolveParameter(parameters, parameter.ParameterName);
                if (parameter.Argument != null
                    || parameterMetadata?.ParameterType == typeof(SwitchParameter))
                {
                    continue;
                }

                if (i + 1 == scriptBlockIndex)
                {
                    return parameterMetadata?.Name;
                }

                if (i + 1 < scriptBlockIndex
                    && !(commandAst.CommandElements[i + 1] is CommandParameterAst))
                {
                    i++;
                }
            }

            return null;
        }

        private bool HasPositionalCommandParameter(
            CommandAst commandAst,
            ScriptBlockExpressionAst scriptBlock,
            string parameterName)
        {
            CommandInfo commandInfo = Helper.Instance.GetCommandInfo(GetCallbackCommandName(commandAst));
            Dictionary<string, ParameterMetadata> parameters = commandInfo == null
                ? null
                : GetCommandParameters(commandInfo);
            if (parameters == null)
            {
                return false;
            }

            CommandElementAst argumentElement = commandAst.CommandElements
                .FirstOrDefault(element => element == scriptBlock
                    || IsDescendantOf(scriptBlock, element));
            int scriptBlockIndex = argumentElement == null
                ? -1
                : commandAst.CommandElements.IndexOf(argumentElement);
            if (scriptBlockIndex < 0)
            {
                return false;
            }

            int positionalIndex = 0;
            for (int i = 1; i < scriptBlockIndex; i++)
            {
                CommandElementAst commandElement = commandAst.CommandElements[i];
                if (!(commandElement is CommandParameterAst parameter))
                {
                    if (IsPositionalParameter(parameters, positionalIndex, parameterName))
                    {
                        return true;
                    }

                    positionalIndex++;
                    continue;
                }

                ParameterMetadata metadata = ResolveParameter(parameters, parameter.ParameterName);
                if (parameter.Argument != null || metadata?.ParameterType == typeof(SwitchParameter))
                {
                    continue;
                }

                if (i + 1 < scriptBlockIndex
                    && !(commandAst.CommandElements[i + 1] is CommandParameterAst))
                {
                    i++;
                }
            }

            return false;
        }

        private bool IsPositionalParameter(
            Dictionary<string, ParameterMetadata> parameters,
            int position,
            string parameterName)
        {
            return parameters.Values.Any(parameter =>
                String.Equals(parameter.Name, parameterName, StringComparison.OrdinalIgnoreCase)
                && parameter.Attributes.OfType<ParameterAttribute>()
                    .Any(attribute => attribute.Position == position));
        }

        private ParameterMetadata ResolveParameter(
            Dictionary<string, ParameterMetadata> parameters,
            string parameterName)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                return null;
            }

            ParameterMetadata exactMatch = null;
            var prefixMatches = new List<ParameterMetadata>();
            foreach (ParameterMetadata parameter in parameters.Values)
            {
                if (String.Equals(parameter.Name, parameterName, StringComparison.OrdinalIgnoreCase)
                    || parameter.Aliases.Contains(parameterName, StringComparer.OrdinalIgnoreCase))
                {
                    exactMatch = parameter;
                    break;
                }

                if (parameter.Name.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase)
                    || parameter.Aliases.Any(alias => alias.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase)))
                {
                    prefixMatches.Add(parameter);
                }
            }

            return exactMatch ?? (prefixMatches.Count == 1 ? prefixMatches[0] : null);
        }

        private ParameterMetadata GetPositionalScriptBlockParameter(
            Dictionary<string, ParameterMetadata> parameters,
            int position)
        {
            ParameterMetadata match = null;
            ParameterMetadata remainingScriptsParameter = null;
            foreach (ParameterMetadata parameter in parameters.Values)
            {
                Type parameterType = parameter.ParameterType;
                if (parameterType.IsArray)
                {
                    parameterType = parameterType.GetElementType();
                }

                if (!typeof(ScriptBlock).IsAssignableFrom(parameterType))
                {
                    continue;
                }

                IEnumerable<ParameterAttribute> parameterAttributes = parameter.Attributes.OfType<ParameterAttribute>();
                if (parameterAttributes.Any(attribute => attribute.Position == position))
                {
                    if (match != null)
                    {
                        return null;
                    }

                    match = parameter;
                }

                if (parameterAttributes.Any(attribute => attribute.ValueFromRemainingArguments))
                {
                    if (remainingScriptsParameter != null)
                    {
                        return null;
                    }

                    remainingScriptsParameter = parameter;
                }
            }

            return match ?? remainingScriptsParameter;
        }

        private bool HasCommandParameter(CommandAst commandAst, string parameterName)
        {
            CommandInfo commandInfo = Helper.Instance.GetCommandInfo(GetCallbackCommandName(commandAst));
            if (commandInfo == null)
            {
                return false;
            }

            Dictionary<string, ParameterMetadata> parameters = GetCommandParameters(commandInfo);
            return commandAst.CommandElements
                .OfType<CommandParameterAst>()
                .Any(parameter =>
                {
                    string resolvedName = parameters == null
                        ? null
                        : ResolveParameter(parameters, parameter.ParameterName)?.Name;
                    return resolvedName?.Equals(parameterName, StringComparison.OrdinalIgnoreCase) == true
                        || (resolvedName == null
                            && IsInvokeCommand(GetCallbackCommandName(commandAst))
                            && IsInvokeCommandParameter(parameter.ParameterName, parameterName));
                });
        }

        private bool HasEnabledCommandParameter(CommandAst commandAst, string parameterName)
        {
            CommandInfo commandInfo = Helper.Instance.GetCommandInfo(GetCallbackCommandName(commandAst));
            Dictionary<string, ParameterMetadata> parameters = commandInfo == null
                ? null
                : GetCommandParameters(commandInfo);
            foreach (CommandParameterAst parameter in commandAst.CommandElements.OfType<CommandParameterAst>())
            {
                string resolvedName = parameters == null
                    ? null
                    : ResolveParameter(parameters, parameter.ParameterName)?.Name;
                if (!String.Equals(resolvedName, parameterName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (parameter.Argument == null)
                {
                    return true;
                }

                try
                {
                    return LanguagePrimitives.IsTrue(parameter.Argument.SafeGetValue());
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRemoteInvokeCommand(CommandAst commandAst)
        {
            CommandInfo commandInfo = Helper.Instance.GetCommandInfo(GetCallbackCommandName(commandAst));
            if (commandInfo == null)
            {
                return false;
            }

            Dictionary<string, ParameterMetadata> parameters = GetCommandParameters(commandInfo);
            if (parameters == null)
            {
                return false;
            }

            string[] remoteTargetParameters =
            {
                "ComputerName",
                "Session",
                "ConnectionUri",
                "HostName",
                "VMId",
                "VMName",
                "ContainerId"
            };

            foreach (CommandParameterAst parameter in commandAst.CommandElements.OfType<CommandParameterAst>())
            {
                string parameterName = ResolveParameter(parameters, parameter.ParameterName)?.Name;
                if (remoteTargetParameters.Contains(parameterName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            int positionalIndex = 0;
            for (int i = 1; i < commandAst.CommandElements.Count; i++)
            {
                CommandElementAst element = commandAst.CommandElements[i];
                if (element is CommandParameterAst parameter)
                {
                    ParameterMetadata metadata = ResolveParameter(parameters, parameter.ParameterName);
                    if (parameter.Argument == null
                        && metadata?.ParameterType != typeof(SwitchParameter)
                        && (i + 1 < commandAst.CommandElements.Count
                            && !(commandAst.CommandElements[i + 1] is CommandParameterAst)))
                    {
                        i++;
                    }

                    continue;
                }

                bool isScriptBlock = element is ScriptBlockExpressionAst
                    || element.FindAll(candidate => candidate is ScriptBlockExpressionAst, true).Any();
                if (!isScriptBlock
                    && remoteTargetParameters.Any(name =>
                        parameters.TryGetValue(name, out ParameterMetadata metadata)
                        && metadata.Attributes.OfType<ParameterAttribute>()
                            .Any(attribute => attribute.Position == positionalIndex)))
                {
                    return true;
                }

                positionalIndex++;
            }

            return false;
        }

        private Dictionary<string, ParameterMetadata> GetCommandParameters(CommandInfo commandInfo)
        {
            if ((commandInfo.CommandType & CommandTypes.Application) != 0)
            {
                return null;
            }

            try
            {
                return commandInfo.Parameters;
            }
            catch (InvalidOperationException)
            {
                return GetCommandParametersWithoutCache(commandInfo);
            }
            catch (NotSupportedException)
            {
                return GetCommandParametersWithoutCache(commandInfo);
            }
        }

        private Dictionary<string, ParameterMetadata> GetCommandParametersWithoutCache(CommandInfo commandInfo)
        {
            try
            {
                return Helper.Instance.GetCommandInfo(commandInfo.Name, bypassCache: true)?.Parameters;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        private bool IsInvokeCommand(string commandName)
        {
            return String.Equals(commandName, "Invoke-Command", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsInvokeCommandScriptBlockParameter(string parameterName)
        {
            return String.Equals(parameterName, "ScriptBlock", StringComparison.OrdinalIgnoreCase)
                || String.Equals(parameterName, "Command", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsInvokeCommandParameter(string parameterName, string expectedName)
        {
            if (String.Equals(expectedName, "ScriptBlock", StringComparison.OrdinalIgnoreCase))
            {
                return IsInvokeCommandScriptBlockParameter(parameterName);
            }

            if (String.Equals(expectedName, "ComputerName", StringComparison.OrdinalIgnoreCase))
            {
                return String.Equals(parameterName, "ComputerName", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(parameterName, "Cn", StringComparison.OrdinalIgnoreCase);
            }

            if (String.Equals(expectedName, "Session", StringComparison.OrdinalIgnoreCase))
            {
                return String.Equals(parameterName, "Session", StringComparison.OrdinalIgnoreCase)
                    || String.Equals(parameterName, "PSSession", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private bool IsParameter(string parameter, string parameterName)
        {
            return String.Equals(parameter, parameterName, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsForEachObjectCommand(string commandName)
        {
            return String.Equals(commandName, "ForEach-Object", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsWhereObjectCommand(string commandName)
        {
            return String.Equals(commandName, "Where-Object", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsInConstantFalseBranch(FunctionDefinitionAst function)
        {
            for (Ast parent = function.Parent; parent != null; parent = parent.Parent)
            {
                if (!(parent is IfStatementAst ifStatement))
                {
                    continue;
                }

                foreach (Tuple<PipelineBaseAst, StatementBlockAst> clause in ifStatement.Clauses)
                {
                    if (IsDescendantOf(function, clause.Item2) && IsConstantFalseCondition(clause.Item1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsDescendantOf(Ast ast, Ast ancestor)
        {
            for (Ast parent = ast.Parent; parent != null; parent = parent.Parent)
            {
                if (parent == ancestor)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsConstantFalseCondition(PipelineBaseAst condition)
        {
            ExpressionAst expression = condition.GetPureExpression();
            if (expression == null)
            {
                return false;
            }

            try
            {
                return !LanguagePrimitives.IsTrue(expression.SafeGetValue());
            }
            catch (InvalidOperationException)
            {
                return false;
            }
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
#if CORECLR
                || commandPipeline.Background
#endif
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
#if CORECLR
                        && !bodyPipeline.Background
#endif
                        && bodyPipeline.PipelineElements[0] is CommandExpressionAst bodyExpression)
                    {
                        if (HasStatementSeparator(cmdAst, bodyPipeline)
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

        private bool HasStatementSeparator(CommandAst statement, Ast nextStatement)
        {
            int separatorStart = statement.CommandElements[statement.CommandElements.Count - 1].Extent.EndOffset;
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
