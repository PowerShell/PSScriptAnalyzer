using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using System;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public class UseCompatibleCmdlets2 : IScriptRule
    {
        private CompatibilityProfileLoader _profileLoader;

        public UseCompatibleCmdlets2()
        {
            _profileLoader = new CompatibilityProfileLoader();
        }

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            CmdletCompatibilityVisitor compatibilityVisitor = CreateVisitorFromConfiguration(fileName);
            ast.Visit(compatibilityVisitor);
            return compatibilityVisitor.GetDiagnosticRecords();
        }

        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdletsDescription);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdletsDescription);
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseCompatibleCmdletsName);
        }

        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        public DiagnosticSeverity DiagnosticSeverity => DiagnosticSeverity.Warning;

        private CmdletCompatibilityVisitor CreateVisitorFromConfiguration(string analyzedFileName)
        {
            IDictionary<string, object> ruleArgs = Helper.Instance.GetRuleArguments(GetName());
            string configPath = ruleArgs["configPath"] as string;

            CompatibilityProfileData profile = _profileLoader.GetProfileFromFilePath(configPath);
            return new CmdletCompatibilityVisitor(analyzedFileName, profile, this);
        }

        private class CmdletCompatibilityVisitor : AstVisitor
        {
            private readonly CompatibilityProfileData _compatibilityTarget;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            private readonly string _analyzedFileName;

            private readonly UseCompatibleCmdlets2 _rule;

            public CmdletCompatibilityVisitor(
                string analyzedFileName,
                CompatibilityProfileData compatibilityTarget,
                UseCompatibleCmdlets2 rule)
            {
                _analyzedFileName = analyzedFileName;
                _compatibilityTarget = compatibilityTarget;
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

                if (_compatibilityTarget.Runtime.Modules.ContainsKey(commandName))
                {
                    return AstVisitAction.Continue;
                }

                Version targetVersion = _compatibilityTarget.Platform[0].PowerShell.Version;
                string platform = _compatibilityTarget.Platform[0].OperatingSystem.Name;
                string message = $"The command '{commandName}' is not compatible with PowerShell v{targetVersion} on platform {platform}";

                var diagnostic = new DiagnosticRecord(
                    message,
                    commandAst.Extent,
                    _rule.GetName(),
                    _rule.DiagnosticSeverity,
                    _analyzedFileName,
                    ruleId: null,
                    suggestedCorrections: null);

                _diagnosticAccumulator.Add(diagnostic);

                return AstVisitAction.Continue;
            }

            public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }
        }
    }
}
