using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{

#if !CORECLR
    [System.ComponentModel.Composition.Export(typeof(IScriptRule))]
#endif
    public class UseCompatibleSyntax : ConfigurableRule
    {
        private static readonly Version s_v3 = new Version(3,0);

        private static readonly Version s_v4 = new Version(4,0);

        private static readonly Version s_v5 = new Version(5,0);

        private static readonly Version s_v6 = new Version(6,0);

        private static readonly IReadOnlyList<Version> s_targetableVersions = new []
        {
            s_v3,
            s_v4,
            s_v5,
            s_v6
        };

        [ConfigurableRuleProperty(new string[]{})]
        public string[] TargetVersions { get; set; }

        public DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            HashSet<Version> targetVersions = GetTargetedVersions(TargetVersions);

            var visitor = new SyntaxCompatibilityVisitor(this, fileName, targetVersions);
            ast.Visit(visitor);
            return visitor.GetDiagnosticRecords();
        }

        public override string GetCommonName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleSyntaxCommonName);
        }

        public override string GetDescription()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.UseCompatibleSyntaxDescription);
        }

        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.UseCompatibleSyntaxName);
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

        private static HashSet<Version> GetTargetedVersions(string[] versionSettings)
        {
            if (versionSettings == null || versionSettings.Length <= 0)
            {
                return new HashSet<Version>(){ s_v5, s_v6 };
            }

            var targetVersions = new HashSet<Version>();
            foreach (string versionStr in versionSettings)
            {
                if (!Version.TryParse(versionStr, out Version version))
                {
                    throw new ArgumentException($"Invalid version string: {versionStr}");
                }

                foreach (Version targetableVersion in s_targetableVersions)
                {
                    if (version.Major == targetableVersion.Major)
                    {
                        targetVersions.Add(targetableVersion);
                        break;
                    }
                }
            }
            return targetVersions;
        }

#if !PSV3
        private class SyntaxCompatibilityVisitor : AstVisitor2
#else
        private class SyntaxCompatibilityVisitor : AstVisitor
#endif
        {
            private readonly UseCompatibleSyntax _rule;

            private readonly string _analyzedFilePath;

            private readonly HashSet<Version> _targetVersions;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            public SyntaxCompatibilityVisitor(
                UseCompatibleSyntax rule,
                string analyzedScriptPath,
                HashSet<Version> targetVersions)
            {
                _diagnosticAccumulator = new List<DiagnosticRecord>();
                _rule = rule;
                _analyzedFilePath = analyzedScriptPath;
                _targetVersions = targetVersions;
            }

            public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }

            public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
            {
                if (!_targetVersions.Contains(s_v3) && !_targetVersions.Contains(s_v4))
                {
                    return AstVisitAction.Continue;
                }

                if (!(methodCallAst.Expression is TypeExpressionAst typeExpressionAst))
                {
                    return AstVisitAction.Continue;
                }

                if (!(methodCallAst.Member is StringConstantExpressionAst stringConstantAst))
                {
                    return AstVisitAction.Continue;
                }

                if (stringConstantAst.Value.Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    string typeName = typeExpressionAst.TypeName.FullName;

                    CorrectionExtent suggestedCorrection = CreateNewObjectCorrection(
                        _analyzedFilePath,
                        methodCallAst.Extent,
                        typeName,
                        methodCallAst.Arguments);

                    string message = string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.UseCompatibleSyntaxError,
                        "constructor",
                        methodCallAst.Extent.Text,
                        "3,4");

                    _diagnosticAccumulator.Add(new DiagnosticRecord(
                        message,
                        methodCallAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath,
                        ruleId: null,
                        suggestedCorrections: new [] { suggestedCorrection }
                    ));

                    return AstVisitAction.Continue;
                }

                return AstVisitAction.Continue;
            }

            public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
            {
                if (!_targetVersions.Contains(s_v6))
                {
                    return AstVisitAction.Continue;
                }

                if (!functionDefinitionAst.IsWorkflow)
                {
                    return AstVisitAction.Continue;
                }

                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UseCompatibleSyntaxError,
                    "workflow",
                    "workflow { ... }",
                    "6");

                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        message,
                        functionDefinitionAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }

#if !PSV3
            public override AstVisitAction VisitUsingStatement(UsingStatementAst usingStatementAst)
            {
                if (!_targetVersions.Contains(s_v3) && !_targetVersions.Contains(s_v4))
                {
                    return AstVisitAction.Continue;
                }

                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UseCompatibleSyntaxError,
                    "using statement",
                    "using ...;",
                    "3,4");

                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        message,
                        usingStatementAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }

            public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
            {
                if (!_targetVersions.Contains(s_v3) && !_targetVersions.Contains(s_v4))
                {
                    return AstVisitAction.Continue;
                }

                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    "type definition",
                    "class MyClass { ... } | enum MyEnum { ... }",
                    "3,4");

                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        message,
                        typeDefinitionAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }
#endif

            private static CorrectionExtent CreateNewObjectCorrection(
                string filePath,
                IScriptExtent offendingExtent,
                string typeName,
                IReadOnlyList<ExpressionAst> argumentAsts)
            {
                var sb = new StringBuilder("New-Object ")
                    .Append('\'')
                    .Append(typeName)
                    .Append('\'');

                if (argumentAsts != null && argumentAsts.Count > 0)
                {
                    sb.Append(" @(");
                    int i = 0;
                    for (; i < argumentAsts.Count - 1; i++)
                    {
                        ExpressionAst argAst = argumentAsts[i];
                        sb.Append(argAst.Extent.Text).Append(", ");
                    }
                    sb.Append(argumentAsts[i].Extent.Text).Append(")");
                }

                return new CorrectionExtent(
                    offendingExtent,
                    sb.ToString(),
                    filePath,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.UseCompatibleSyntaxCorrection,
                        "New-Object [@($args)]",
                        "3,4"));
            }
        }
    }
}