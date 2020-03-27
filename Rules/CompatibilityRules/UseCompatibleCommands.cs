// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{

    /// <summary>
    /// Rule to identify when commands used in a script will not
    /// be available in target PowerShell runtimes.
    /// </summary>
#if !CORECLR
    [System.ComponentModel.Composition.Export(typeof(IScriptRule))]
#endif
    public class UseCompatibleCommands : CompatibilityRule
    {
        /// <summary>
        /// List of commands to ignore the compatibility of.
        /// </summary>
        [ConfigurableRuleProperty(new string[] {})]
        public string[] IgnoreCommands { get; set; }

        /// <summary>
        /// Get the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCommandsCommonName);
        }

        /// <summary>
        /// Get the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCommandsDescription);
        }

        /// <summary>
        /// Get the localized name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseCompatibleCommandsName);
        }

        /// <summary>
        /// Create an AST visitor to generate command-compatiblity diagnostics.
        /// </summary>
        /// <param name="analyzedFileName">The full path of the script analyzer.</param>
        /// <returns>A command-compatibility assessing AST visitor.</returns>
        protected override CompatibilityVisitor CreateVisitor(string analyzedFileName)
        {
            IEnumerable<CompatibilityProfileData> compatibilityTargets = LoadCompatibilityProfiles(out CompatibilityProfileData unionProfile);
            return new CommandCompatibilityVisitor(analyzedFileName, compatibilityTargets, unionProfile, IgnoreCommands, rule: this);
        }

        private class CommandCompatibilityVisitor : CompatibilityVisitor
        {
            private readonly IEnumerable<CompatibilityProfileData> _compatibilityTargets;

            private readonly CompatibilityProfileData _anyProfile;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly string _analyzedFileName;

            private readonly UseCompatibleCommands _rule;

            private readonly HashSet<string> _commandsToIgnore;

            public CommandCompatibilityVisitor(
                string analyzedFileName,
                IEnumerable<CompatibilityProfileData> compatibilityTargets,
                CompatibilityProfileData anyProfile,
                IEnumerable<string> commandsToIgnore,
                UseCompatibleCommands rule)
            {
                _analyzedFileName = analyzedFileName;
                _compatibilityTargets = compatibilityTargets;
                _anyProfile = anyProfile;
                _diagnosticAccumulator = new List<DiagnosticRecord>();
                _rule = rule;
                _commandsToIgnore = new HashSet<string>(commandsToIgnore, StringComparer.OrdinalIgnoreCase);
            }

            public override AstVisitAction VisitCommand(CommandAst commandAst)
            {
                string commandName = commandAst?.GetCommandName();
                if (commandName == null)
                {
                    return AstVisitAction.Continue;
                }

                if (_commandsToIgnore.Contains(commandName))
                {
                    return AstVisitAction.Continue;
                }

                // Note:
                // The "right" way to eliminate user-defined commands would be to build
                // a list of:
                //  - all functions defined above this point
                //  - all modules imported
                // However, looking for imported modules could prove very expensive
                // and we would still miss things like assignments to the function: provider.
                // Instead, we look to see if a command of the given name is present in any
                // known profile, which is something of a hack.

                // This is not present in any known profiles, so assume it is user defined
                if (!_anyProfile.Runtime.Commands.ContainsKey(commandName))
                {
                    return AstVisitAction.Continue;
                }

                // Check each target platform
                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    // If the target has this command, everything is good
                    if (targetProfile.Runtime.Commands.TryGetValue(commandName, out IReadOnlyList<CommandData> matchedCommands))
                    {
                        // Now check that the parameters on the command are available on all target platforms
                        CheckCommandInvocationParameters(targetProfile, commandName, commandAst, matchedCommands);
                        continue;
                    }

                    var diagnostic = CommandCompatibilityDiagnostic.Create(
                        commandName,
                        targetProfile.Platform,
                        commandAst.Extent,
                        _analyzedFileName,
                        _rule);

                    _diagnosticAccumulator.Add(diagnostic);
                }

                return AstVisitAction.Continue;
            }

            public override IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }

            private void CheckCommandInvocationParameters(
                CompatibilityProfileData targetProfile,
                string commandName,
                CommandAst commandAst,
                IEnumerable<CommandData> commandsToCheck)
            {
                // TODO:
                // Ideally we would go through each command and emulate the parameter binding algorithm
                // to work out what positions and what parameters will and won't work,
                // but this is very involved.
                // For now, we'll just check that the parameters exist

                for (int i = 0; i < commandAst.CommandElements.Count; i++)
                {
                    CommandElementAst commandElement = commandAst.CommandElements[i];
                    if (!(commandElement is CommandParameterAst parameterAst))
                    {
                        continue;
                    }

                    bool isGoodParam = false;
                    foreach (CommandData command in commandsToCheck)
                    {
                        if ((command.Parameters != null && command.Parameters.ContainsKey(parameterAst.ParameterName))
                            || (command.IsCmdletBinding && targetProfile.Runtime.Common.Parameters.ContainsKey(parameterAst.ParameterName)))
                        {
                            isGoodParam = true;
                            break;
                        }
                    }

                    if (isGoodParam)
                    {
                        continue;
                    }

                    _diagnosticAccumulator.Add(CommandCompatibilityDiagnostic.CreateForParameter(
                        parameterAst.ParameterName,
                        commandName,
                        targetProfile.Platform,
                        parameterAst.Extent,
                        _analyzedFileName,
                        _rule));
                }
            }
        }
    }

    /// <summary>
    /// A compatibility diagnostic that carries details of the
    /// command warned about and the target platform it is incompatible with.
    /// </summary>
    public class CommandCompatibilityDiagnostic : CompatibilityDiagnostic
    {
        /// <summary>
        /// Create a new command compatibility diagnostic.
        /// </summary>
        /// <param name="commandName">The name of the incompatible command.</param>
        /// <param name="platform">An object detailing the target platform that the command is incompatible with.</param>
        /// <param name="extent">The AST extent of the incompatible command.</param>
        /// <param name="analyzedFileName">The path of the script where the incompatibility is.</param>
        /// <param name="rule">The compatibility rule generating the diagnostic.</param>
        /// <param name="suggestedCorrections">Any suggested corrections for the diagnosed issue.</param>
        /// <returns></returns>
        public static CommandCompatibilityDiagnostic Create(
            string commandName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleCommandsCommandError,
                commandName,
                platform.PowerShell.Version,
                platform.OperatingSystem.FriendlyName);

            return new CommandCompatibilityDiagnostic(
                commandName,
                platform,
                message,
                extent,
                rule.GetName(),
                ruleId: commandName,
                analyzedFileName: analyzedFileName,
                suggestedCorrections: suggestedCorrections);
        }

        /// <summary>
        /// Create a compatibility diagnostic for an incompatible parameter.
        /// </summary>
        /// <param name="parameterName">The name of the incompatible parameter.</param>
        /// <param name="commandName">The name of the command where the parameter is incompatible.</param>
        /// <param name="platform">The platform where the parameter is incompatible.</param>
        /// <param name="extent">The AST extent of the incompatible invocation.</param>
        /// <param name="analyzedFileName">The path of the script where the incompatibility has been found.</param>
        /// <param name="rule">The rule that found the incompatibility.</param>
        /// <param name="suggestedCorrections">Any suggested corrections, may be null.</param>
        /// <returns></returns>
        public static CommandCompatibilityDiagnostic CreateForParameter(
            string parameterName,
            string commandName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleCommandsParameterError,
                parameterName,
                commandName,
                platform.PowerShell.Version,
                platform.OperatingSystem.FriendlyName);

            return new CommandCompatibilityDiagnostic(
                commandName,
                platform,
                message,
                extent,
                rule.GetName(),
                ruleId: $"{commandName}/{parameterName}",
                analyzedFileName: analyzedFileName,
                parameterName: parameterName);
        }

        private CommandCompatibilityDiagnostic(
            string incompatibleCommand,
            PlatformData targetPlatform,
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
            string parameterName = null,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
            : base(
                message,
                extent,
                ruleName,
                ruleId,
                analyzedFileName,
                suggestedCorrections)
        {
            Command = incompatibleCommand;
            TargetPlatform = targetPlatform;
            Parameter = parameterName;
        }

        /// <summary>
        /// The name of the command that is incompatible.
        /// </summary>
        /// <value></value>
        public string Command { get; }

        /// <summary>
        /// The name of the incompatible command, if any
        /// </summary>
        public string Parameter { get; }

        /// <summary>
        /// The platform where the command is incompatible.
        /// </summary>
        public PlatformData TargetPlatform { get; }
    }
}
