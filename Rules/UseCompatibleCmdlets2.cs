using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using System;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.PowerShell.CrossCompatibility.Query.Platform;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public class UseCompatibleCmdlets2 : ConfigurableRule
    {
        private CompatibilityProfileLoader _profileLoader;

        public UseCompatibleCmdlets2()
        {
            _profileLoader = new CompatibilityProfileLoader();
        }

        [ConfigurableRuleProperty(defaultValue: "")]
        public string AnyProfilePath { get; set; }

        [ConfigurableRuleProperty(defaultValue: new string[] {})]
        public string[] TargetProfilePaths { get; set; }

        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            string cwd = Directory.GetCurrentDirectory();
            CmdletCompatibilityVisitor compatibilityVisitor = CreateVisitorFromConfiguration(fileName);
            ast.Visit(compatibilityVisitor);
            return compatibilityVisitor.GetDiagnosticRecords();
        }

        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdlets2Description);
        }

        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdlets2Description);
        }

        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdlets2Name);
        }

        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        public DiagnosticSeverity DiagnosticSeverity => DiagnosticSeverity.Warning;

        private CmdletCompatibilityVisitor CreateVisitorFromConfiguration(string analyzedFileName)
        {
            if (string.IsNullOrEmpty(AnyProfilePath))
            {
                throw new InvalidOperationException($"{nameof(AnyProfilePath)} cannot be null or empty");
            }

            if (TargetProfilePaths == null)
            {
                throw new InvalidOperationException($"{nameof(TargetProfilePaths)} cannot be null");
            }

            if (TargetProfilePaths.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(TargetProfilePaths)} cannot be empty");
            }

            var targetProfiles = new List<CompatibilityProfileData>();
            foreach (string configPath in TargetProfilePaths)
            {
                targetProfiles.Add(_profileLoader.GetProfileFromFilePath(configPath));
            }

            CompatibilityProfileData anyProfile = _profileLoader.GetProfileFromFilePath(AnyProfilePath);
            return new CmdletCompatibilityVisitor(analyzedFileName, targetProfiles, anyProfile, rule: this);
        }

        private class CmdletCompatibilityVisitor : AstVisitor
        {
            private readonly IList<CompatibilityProfileData> _compatibilityTargets;

            private readonly CompatibilityProfileData _anyProfileCompatibilityList;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly string _analyzedFileName;

            private readonly UseCompatibleCmdlets2 _rule;

            public CmdletCompatibilityVisitor(
                string analyzedFileName,
                IList<CompatibilityProfileData> compatibilityTarget,
                CompatibilityProfileData anyProfileCompatibilityList,
                UseCompatibleCmdlets2 rule)
            {
                _analyzedFileName = analyzedFileName;
                _compatibilityTargets = compatibilityTarget;
                _anyProfileCompatibilityList = anyProfileCompatibilityList;
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

            public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }
        }
    }

    public abstract class IncompatibilityDiagnostic : DiagnosticRecord
    {
        protected IncompatibilityDiagnostic(
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
            IEnumerable<CorrectionExtent> suggestedCorrections)
            : base(
                message,
                extent,
                ruleName,
                DiagnosticSeverity.Warning,
                analyzedFileName,
                ruleId: null,
                suggestedCorrections: suggestedCorrections)
        {
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
            targetPlatform = TargetPlatform;
        }

        public string Command { get; }

        public PlatformData TargetPlatform { get; }
    }
}
