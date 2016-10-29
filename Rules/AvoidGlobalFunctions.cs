using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidGlobalFunctions : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> records;
        private string fileName;

        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            ast.Visit(this);

            return records;
        }

        #region VisitCommand functions
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if (functionDefinitionAst.Name.StartsWith("Global:", StringComparison.OrdinalIgnoreCase) && IsModule())
            {
                var functionNameExtent = Helper.Instance.GetScriptExtentForFunctionName(functionDefinitionAst);

                records.Add(new DiagnosticRecord(
                                string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalFunctionsError),
                                functionNameExtent,
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName,
                                functionDefinitionAst.Name));
            }

            return AstVisitAction.Continue;
        }
        #endregion

        private bool IsModule()
        {
            return fileName.EndsWith(".psm1");
        }

        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalFunctionsCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalFunctionsDescription);
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalFunctionsName);
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
    }
}
