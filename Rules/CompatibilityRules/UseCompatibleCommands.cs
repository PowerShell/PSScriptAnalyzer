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
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public class UseCompatibleCommands : CompatibilityRule
    {
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCommandsCommonName);
        }

        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCommandsDescription);
        }

        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCommandsName);
        }

        protected override CompatibilityVisitor CreateVisitor(string analyzedFileName)
        {
            Tuple<CompatibilityProfileData, CompatibilityProfileData[]> profiles = LoadCompatibilityProfiles();
            return new CmdletCompatibilityVisitor(analyzedFileName, compatibilityTargets: profiles.Item2, anyProfile: profiles.Item1, rule: this);
        }

        private class CmdletCompatibilityVisitor : CompatibilityVisitor
        {
            private readonly IList<CompatibilityProfileData> _compatibilityTargets;

            private readonly CompatibilityProfileData _anyProfileCompatibilityList;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly string _analyzedFileName;

            private readonly UseCompatibleCommands _rule;

            public CmdletCompatibilityVisitor(
                string analyzedFileName,
                CompatibilityProfileData[] compatibilityTargets,
                CompatibilityProfileData anyProfile,
                UseCompatibleCommands rule)
            {
                _analyzedFileName = analyzedFileName;
                _compatibilityTargets = compatibilityTargets;
                _anyProfileCompatibilityList = anyProfile;
                _diagnosticAccumulator = new List<DiagnosticRecord>();
                _rule = rule;
            }

            public override AstVisitAction VisitCommand(CommandAst commandAst)
            {
                if (commandAst == null)
                {
                    return AstVisitAction.SkipChildren;
                }

                string commandName = commandAst.GetCommandName();
                if (commandName == null)
                {
                    return AstVisitAction.SkipChildren;
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
                if (!_anyProfileCompatibilityList.Runtime.Commands.ContainsKey(commandName))
                {
                    return AstVisitAction.Continue;
                }

                // Check each target platform
                foreach (CompatibilityProfileData targetProfile in _compatibilityTargets)
                {
                    // If the target has this command, everything is good
                    if (targetProfile.Runtime.Commands.ContainsKey(commandName))
                    {
                        // TODO: Check parameters
                        continue;
                    }

                    var diagnostic = IncompatibleCommandDiagnostic.Create(
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
        }
    }

    public class IncompatibleCommandDiagnostic : IncompatibilityDiagnostic
    {
        public static IncompatibleCommandDiagnostic Create(
            string commandName,
            PlatformData platform,
            IScriptExtent extent,
            string analyzedFileName,
            IRule rule,
            IEnumerable<CorrectionExtent> suggestedCorrections = null)
        {
            string message = String.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleCmdlets2Error,
                commandName,
                platform.PowerShell.Version,
                platform.OperatingSystem.Name);

            return new IncompatibleCommandDiagnostic(
                commandName,
                platform,
                message,
                extent,
                rule.GetName(),
                ruleId: null,
                analyzedFileName: analyzedFileName,
                suggestedCorrections: suggestedCorrections);
        }

        private IncompatibleCommandDiagnostic(
            string incompatibleCommand,
            PlatformData targetPlatform,
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
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
        }

        public string Command { get; }

        public PlatformData TargetPlatform { get; }
    }
}
