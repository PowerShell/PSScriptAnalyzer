// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidGlobalFunctions: Checks that global functions are not used within modules.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidGlobalFunctions : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> records;
        private string fileName;

        /// <summary>
        /// Analyzes the ast to check that global functions are not used within modules.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            if (fileName != null && Helper.IsModuleScript(fileName))
            {
                ast.Visit(this);
            }

            return records;
        }

        #region VisitCommand functions
        /// <summary>
        /// Analyzes a FunctionDefinitionAst, if it is declared global a diagnostic record is created.
        /// </summary>
        /// <param name="functionDefinitionAst">FunctionDefinitionAst to be analyzed</param>
        /// <returns>AstVisitAction to continue analysis</returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            if (functionDefinitionAst.Name.StartsWith("Global:", StringComparison.OrdinalIgnoreCase))
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
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidGlobalFunctionsName);
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
