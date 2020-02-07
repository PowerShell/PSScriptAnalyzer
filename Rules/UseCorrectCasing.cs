// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Management.Automation;
using System.Linq;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseCorrectCasing: Check if cmdlet is cased correctly.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseCorrectCasing : ConfigurableRule
    {
        /// <summary>
        /// AnalyzeScript: Analyze the script to check if cmdlet alias is used.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> commandAsts = ast.FindAll(testAst => testAst is CommandAst, true);

            // Iterates all CommandAsts and check the command name.
            foreach (CommandAst commandAst in commandAsts)
            {
                string commandName = commandAst.GetCommandName();

                // Handles the exception caused by commands like, {& $PLINK $args 2> $TempErrorFile}.
                // You can also review the remark section in following document,
                // MSDN: CommandAst.GetCommandName Method
                if (commandName == null)
                {
                    continue;
                }

                var commandInfo = Helper.Instance.GetCommandInfo(commandName);
                if (commandInfo == null || commandInfo.CommandType == CommandTypes.ExternalScript || commandInfo.CommandType == CommandTypes.Application)
                {
                    continue;
                }

                var shortName = commandInfo.Name;
                var fullyqualifiedName = $"{commandInfo.ModuleName}\\{shortName}";
                var isFullyQualified = commandName.Equals(fullyqualifiedName, StringComparison.OrdinalIgnoreCase);
                var correctlyCasedCommandName = isFullyQualified ? fullyqualifiedName : shortName;

                if (!commandName.Equals(correctlyCasedCommandName, StringComparison.Ordinal))
                {
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingError, commandName, shortName),
                        GetCommandExtent(commandAst),
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        commandName,
                        suggestedCorrections: GetCorrectionExtent(commandAst, correctlyCasedCommandName));
                }

                var commandParameterAsts = commandAst.FindAll(
                    testAst => testAst is CommandParameterAst, true).Cast<CommandParameterAst>();
                var availableParameters = commandInfo.Parameters;
                foreach (var commandParameterAst in commandParameterAsts)
                {
                    var parameterName = commandParameterAst.ParameterName;
                    if (availableParameters.TryGetValue(parameterName, out ParameterMetadata parameterMetaData))
                    {
                        var correctlyCasedParameterName = parameterMetaData.Name;
                        if (!parameterName.Equals(correctlyCasedParameterName, StringComparison.Ordinal))
                        {
                            yield return new DiagnosticRecord(
                                string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingError, commandName, parameterName),
                                GetCommandExtent(commandAst),
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName,
                                commandName,
                                suggestedCorrections: GetCorrectionExtent(commandParameterAst, correctlyCasedParameterName));
                        }
                    }
                }
            }
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

        private IEnumerable<CorrectionExtent> GetCorrectionExtent(CommandAst commandAst, string correctlyCaseName)
        {
            var description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCorrectCasingDescription,
                correctlyCaseName,
                correctlyCaseName);
            var cmdExtent = GetCommandExtent(commandAst);
            var correction = new CorrectionExtent(
                cmdExtent.StartLineNumber,
                cmdExtent.EndLineNumber,
                cmdExtent.StartColumnNumber,
                cmdExtent.EndColumnNumber,
                correctlyCaseName,
                commandAst.Extent.File,
                description);
            yield return correction;
        }

        private IEnumerable<CorrectionExtent> GetCorrectionExtent(CommandParameterAst commandParameterAst, string correctlyCaseName)
        {
            var description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCorrectCasingDescription,
                correctlyCaseName,
                correctlyCaseName);
            var cmdExtent = commandParameterAst.Extent;
            var correction = new CorrectionExtent(
                cmdExtent.StartLineNumber,
                cmdExtent.EndLineNumber,
                // +1 because of the dash before the parameter name
                cmdExtent.StartColumnNumber + 1,
                // do not use EndColumnNumber property as it would not cover the case where the colon syntax: -ParameterName:$ParameterValue
                cmdExtent.StartColumnNumber + 1 + commandParameterAst.ParameterName.Length,
                correctlyCaseName,
                commandParameterAst.Extent.File,
                description);
            yield return correction;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseCorrectCasingName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCorrectCasingDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
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
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
