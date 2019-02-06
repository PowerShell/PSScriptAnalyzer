using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    public class UseCompatibleSyntax : ConfigurableRule
    {
        public DiagnosticSeverity Severity => DiagnosticSeverity.Warning;

        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            var visitor = new SyntaxCompatibilityVisitor(this, fileName);
            ast.Visit(visitor);
            return visitor.GetDiagnosticRecords();
        }

        public override string GetCommonName()
        {
            return "Use compatible syntax";
        }

        public override string GetDescription()
        {
            return "Indicates syntax that is not compatible with any targeted PowerShell version";
        }

        public override string GetName()
        {
            return nameof(UseCompatibleSyntax);
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

#if PSV4
        public class SyntaxCompatibilityVisitor : AstVisitor
#else
        public class SyntaxCompatibilityVisitor : AstVisitor2
#endif
        {
            private readonly UseCompatibleSyntax _rule;

            private readonly string _analyzedFilePath;

            private readonly List<DiagnosticRecord> _diagnosticAccumulator;

            public SyntaxCompatibilityVisitor(UseCompatibleSyntax rule, string analyzedScriptPath)
            {
                _diagnosticAccumulator = new List<DiagnosticRecord>();
                _rule = rule;
                _analyzedFilePath = analyzedScriptPath;
            }

            public IEnumerable<DiagnosticRecord> GetDiagnosticRecords()
            {
                return _diagnosticAccumulator;
            }

            public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
            {
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
                    var sb = new StringBuilder("New-Object ")
                        .Append('\'')
                        .Append(typeName)
                        .Append('\'');

                    if (methodCallAst.Arguments != null && methodCallAst.Arguments.Count > 0)
                    {
                        sb.Append(" @(");
                        int i = 0;
                        for (; i < methodCallAst.Arguments.Count - 1; i++)
                        {
                            ExpressionAst argAst = methodCallAst.Arguments[i];
                            sb.Append(argAst.Extent.Text).Append(", ");
                        }
                        sb.Append(methodCallAst.Arguments[i].Extent.Text).Append(")");
                    }

                    IScriptExtent originalExtent = methodCallAst.Extent;
                    var suggestedCorrection = new CorrectionExtent(
                        methodCallAst.Extent,
                        sb.ToString(),
                        _analyzedFilePath,
                        "Use the New-Object cmdlet instead for PowerShell v3/4 compatibility");

                    _diagnosticAccumulator.Add(new DiagnosticRecord(
                        $"Cannot use [{typeName}]::new(...) constructor syntax in PowerShell v3 and v4",
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
                if (!functionDefinitionAst.IsWorkflow)
                {
                    return AstVisitAction.Continue;
                }

                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        "Workflows are not compatible with PowerShell v6 and up",
                        functionDefinitionAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }

#if !PSV4
            public override AstVisitAction VisitUsingStatement(UsingStatementAst usingStatementAst)
            {
                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        "Cannot use 'using' statements in PowerShell v3 or v4",
                        usingStatementAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }

            public override AstVisitAction VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
            {
                _diagnosticAccumulator.Add(
                    new DiagnosticRecord(
                        "Cannot define classes or enums in PowerShell v3 or v4",
                        typeDefinitionAst.Extent,
                        _rule.GetName(),
                        _rule.Severity,
                        _analyzedFilePath
                    ));

                return AstVisitAction.Continue;
            }
#endif
        }
    }
}