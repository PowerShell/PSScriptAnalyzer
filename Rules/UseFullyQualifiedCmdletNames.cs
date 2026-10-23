//---------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// The MIT License (MIT)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseFullyQualifiedCmdletNames: Checks if cmdlet and function invocations use fully qualified module names.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseFullyQualifiedCmdletNames : ConfigurableRule
    {
        private readonly ConcurrentDictionary<string, ResolvedCommand> resolutionCache =
            new ConcurrentDictionary<string, ResolvedCommand>(StringComparer.OrdinalIgnoreCase);

        internal const string AnalyzerName = "Microsoft.Windows.PowerShell.ScriptAnalyzer";

        /// <summary>
        /// Modules to ignore when applying this rule.
        /// Commands from these modules will not be expanded to their fully qualified names.
        /// Default is empty array (no modules ignored - all cmdlets are processed).
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: new string[] { })]
        public string[] IgnoredModules { get; protected set; }  

        /// <summary>
        /// Analyzes the given ast to find cmdlet invocations that are not fully qualified.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>The diagnostic results of this rule</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var functionDefinitions = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>().ToList();

            var commandAsts = ast.FindAll(testAst => testAst is CommandAst, true).Cast<CommandAst>();

            foreach (var commandAst in commandAsts)
            {
                var commandName = commandAst.GetCommandName();
                if (string.IsNullOrWhiteSpace(commandName) || commandName.Contains("\\"))
                {
                    continue;
                }

                // Skip commands that resolve to a locally declared function, since qualifying them would change behavior.
                if (IsShadowedByLocalFunction(commandAst, commandName, functionDefinitions))
                {
                    continue;
                }

                var resolvedCommand = resolutionCache.GetOrAdd(commandName, ResolveCommand);
                if (resolvedCommand.FullyQualifiedName == null)
                {
                    continue;
                }

                // Re-check ignored modules for cached results (in case IgnoredModules was changed).
                if (IgnoredModules != null && IgnoredModules.Contains(resolvedCommand.ModuleName, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var extent = commandAst.CommandElements[0].Extent;

                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    GetErrorResource(resolvedCommand.CommandType),
                    commandName,
                    resolvedCommand.FullyQualifiedName);

                string correctionDescription = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UseFullyQualifiedCmdletNamesCorrection,
                    commandName,
                    resolvedCommand.FullyQualifiedName);

                var suggestedCorrections = new Collection<CorrectionExtent>
                {
                    new CorrectionExtent(
                        extent.StartLineNumber,
                        extent.EndLineNumber,
                        extent.StartColumnNumber,
                        extent.EndColumnNumber,
                        resolvedCommand.FullyQualifiedName,
                        fileName,
                        correctionDescription)
                };

                yield return new DiagnosticRecord(
                    message,
                    extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName,
                    null,
                    suggestedCorrections);
            }
        }

        /// <summary>
        /// Checks whether a command name matches a function declared in a scope that is visible at the
        /// command's location.
        /// </summary>
        private static bool IsShadowedByLocalFunction(
            CommandAst commandAst,
            string commandName,
            IEnumerable<FunctionDefinitionAst> functionDefinitions)
        {
            var commandScope = GetContainingScriptBlock(commandAst);

            foreach (var functionDefinition in functionDefinitions)
            {
                if (!functionDefinition.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var functionScope = GetContainingScriptBlock(functionDefinition);
                if (functionScope != null &&
                    (functionScope == commandScope || IsAncestorOf(functionScope, commandScope)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the nearest enclosing script block, which represents the scope where a function is
        /// declared or a command is invoked.
        /// </summary>
        private static ScriptBlockAst GetContainingScriptBlock(Ast node)
        {
            for (Ast current = node; current != null; current = current.Parent)
            {
                if (current is ScriptBlockAst scriptBlock)
                {
                    return scriptBlock;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the first AST is an ancestor of the second.
        /// </summary>
        private static bool IsAncestorOf(Ast ancestor, Ast descendant)
        {
            for (Ast current = descendant.Parent; current != null; current = current.Parent)
            {
                if (current == ancestor)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the message resource appropriate for the resolved command type.
        /// </summary>
        private static string GetErrorResource(CommandTypes commandType)
        {
            switch (commandType)
            {
                case CommandTypes.Alias:
                    return Strings.UseFullyQualifiedCmdletNamesAliasError;
                case CommandTypes.Function:
                    return Strings.UseFullyQualifiedCmdletNamesFunctionError;
                default:
                    return Strings.UseFullyQualifiedCmdletNamesCommandError;
            }
        }

        /// <summary>
        /// Resolves the command info for a given name using the shared runspace.
        /// </summary>
        /// <param name="commandName">The command name to resolve.</param>
        /// <returns>A cached result describing the resolved command.</returns>
        private ResolvedCommand ResolveCommand(string commandName)
        {
            var commandInfo = Helper.Instance.GetCommandInfo(commandName, CommandTypes.All);
            if (commandInfo == null)
            {
                return new ResolvedCommand(null, null, CommandTypes.Application);
            }

            if (commandInfo.CommandType != CommandTypes.Cmdlet &&
                commandInfo.CommandType != CommandTypes.Function &&
                commandInfo.CommandType != CommandTypes.Alias)
            {
                return new ResolvedCommand(null, null, commandInfo.CommandType);
            }

            var commandType = commandInfo.CommandType;

            string moduleName = commandInfo.ModuleName;
            string resolvedName = commandInfo.Name;

            if (commandInfo is AliasInfo aliasInfo)
            {
                if (aliasInfo.ResolvedCommand == null)
                {
                    return new ResolvedCommand(null, null, commandType);
                }

                resolvedName = aliasInfo.ResolvedCommand.Name;
                moduleName = aliasInfo.ResolvedCommand.ModuleName;
            }

            if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(resolvedName))
            {
                return new ResolvedCommand(null, null, commandType);
            }

            return new ResolvedCommand($"{moduleName}\\{resolvedName}", moduleName, commandType);
        }

        /// <summary>
        /// Holds the result of resolving a command name.
        /// </summary>
        private sealed class ResolvedCommand
        {
            public string FullyQualifiedName { get; }

            public string ModuleName { get; }

            public CommandTypes CommandType { get; }

            public ResolvedCommand(string fullyQualifiedName, string moduleName, CommandTypes commandType)
            {
                FullyQualifiedName = fullyQualifiedName;
                ModuleName = moduleName;
                CommandType = commandType;
            }
        }

        /// <summary>
        /// Retrieves the localized name of this rule.
        /// </summary>
        /// <returns>The localized name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseFullyQualifiedCmdletNamesName);
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseFullyQualifiedCmdletNamesCommonName);
        }

        /// <summary>
        /// Retrieves the localized description of this rule.
        /// </summary>
        /// <returns>The localized description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseFullyQualifiedCmdletNamesDescription);
        }

        /// <summary>
        /// Retrieves the source type of this rule.
        /// </summary>
        /// <returns>The source type of this rule</returns>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Retrieves the source name of this rule.
        /// </summary>
        /// <returns>The source name of this rule</returns>
        public override string GetSourceName()
        {
            return "PS";
        }

        /// <summary>
        /// Retrieves the severity of this rule.
        /// </summary>
        /// <returns>The severity of this rule</returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }
    }
}