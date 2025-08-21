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
        private ConcurrentDictionary<string, string> resolutionCache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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

            var commandAsts = ast.FindAll(testAst => testAst is CommandAst, true).Cast<CommandAst>();

            foreach (var commandAst in commandAsts)
            {
                var commandName = commandAst.GetCommandName();
                if (string.IsNullOrWhiteSpace(commandName) || commandName.Contains("\\"))
                {
                    continue;
                }

                if (!resolutionCache.TryGetValue(commandName, out string fullyQualifiedName))
                {
                    var resolvedCommand = ResolveCommand(commandName);
                    if (resolvedCommand == null)
                    {
                        // Cache null results to avoid repeated lookups
                        resolutionCache[commandName] = null;
                        continue;
                    }

                    if (resolvedCommand.CommandType != CommandTypes.Cmdlet &&
                        resolvedCommand.CommandType != CommandTypes.Function &&
                        resolvedCommand.CommandType != CommandTypes.Alias)
                    {
                        // Cache null results for non-cmdlet/function/alias commands
                        resolutionCache[commandName] = null;
                        continue;
                    }

                    string moduleName = resolvedCommand.ModuleName;
                    string actualCmdletName = resolvedCommand.Name;

                    if (resolvedCommand is AliasInfo aliasInfo)
                    {
                        if (aliasInfo.ResolvedCommand == null)
                        {
                            resolutionCache[commandName] = null;
                            continue;
                        }

                        actualCmdletName = aliasInfo.ResolvedCommand.Name;
                        moduleName = aliasInfo.ResolvedCommand.ModuleName;
                    }

                    if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(actualCmdletName))
                    {
                        resolutionCache[commandName] = null;
                        continue;
                    }

                    // Check if the module is in the ignored list
                    if (IgnoredModules != null && IgnoredModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                    {
                        // Cache null for ignored modules to avoid re-checking
                        resolutionCache[commandName] = null;
                        continue;
                    }

                    fullyQualifiedName = $"{moduleName}\\{actualCmdletName}";
                    resolutionCache[commandName] = fullyQualifiedName;
                }
                else
                {
                    // If we have a cached result but it's null/empty, it means we should skip this command
                    if (string.IsNullOrEmpty(fullyQualifiedName))
                    {
                        continue;
                    }

                    // Re-check ignored modules for cached results (in case IgnoredModules was changed)
                    var moduleName = fullyQualifiedName.Split('\\')[0];
                    if (IgnoredModules != null && IgnoredModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                var extent = commandAst.CommandElements[0].Extent;

                bool isAlias = commandName != fullyQualifiedName.Split('\\')[1];
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    isAlias ? Strings.UseFullyQualifiedCmdletNamesAliasError : Strings.UseFullyQualifiedCmdletNamesCommandError,
                    commandName,
                    fullyQualifiedName);

                string correctionDescription = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UseFullyQualifiedCmdletNamesCorrection,
                    commandName,
                    fullyQualifiedName);

                var suggestedCorrections = new Collection<CorrectionExtent>
                {
                    new CorrectionExtent(
                        extent.StartLineNumber,
                        extent.EndLineNumber,
                        extent.StartColumnNumber,
                        extent.EndColumnNumber,
                        fullyQualifiedName,
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
        /// Resolves the command info for a given name using the shared runspace.
        /// </summary>
        /// <param name="commandName">The command name to resolve.</param>
        /// <returns>The resolved CommandInfo or null if not found.</returns>
        private CommandInfo ResolveCommand(string commandName)
        {
            return Helper.Instance.GetCommandInfo(commandName, CommandTypes.All);
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