// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidUsingWriteHost: Check that Write-Host or Console.Write are not used
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingWriteHost : AstVisitor, IScriptRule
    {
        List<DiagnosticRecord> records;
        string fileName;

        /// <summary>
        /// AnalyzeScript: check that Write-Host or Console.Write are not used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            records = new List<DiagnosticRecord>();
            this.fileName = fileName;

            ast.Visit(this);

            return records;
        }


        /// <summary>
        /// Visit function and skips any function that starts with show
        /// </summary>
        /// <param name="funcAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst funcAst)
        {
            if (funcAst == null || funcAst.Name == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (funcAst.Name.StartsWith("show", StringComparison.OrdinalIgnoreCase))
            {
                return AstVisitAction.SkipChildren;
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Checks that write-host command is not used
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitCommand(CommandAst cmdAst)
        {
            if (cmdAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (cmdAst.GetCommandName() != null && String.Equals(cmdAst.GetCommandName(), "write-host", StringComparison.OrdinalIgnoreCase))
            {
                if (String.IsNullOrWhiteSpace(fileName))
                {
                    records.Add(new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWriteHostErrorScriptDefinition),
                        cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
                }
                else
                {
                    records.Add(new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWriteHostError,
                        System.IO.Path.GetFileName(fileName)), cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
                }
            }

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst imeAst)
        {
            if (imeAst == null)
            {
                return AstVisitAction.SkipChildren;
            }

            TypeExpressionAst typeAst = imeAst.Expression as TypeExpressionAst;

            if (typeAst == null || typeAst.TypeName == null || typeAst.TypeName.FullName == null)
            {
                return AstVisitAction.SkipChildren;
            }

            if (typeAst.TypeName.FullName.EndsWith("console", StringComparison.OrdinalIgnoreCase)
                && !String.IsNullOrWhiteSpace(imeAst.Member.Extent.Text) && imeAst.Member.Extent.Text.StartsWith("Write", StringComparison.OrdinalIgnoreCase))
            {
                records.Add(new DiagnosticRecord(String.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingConsoleWriteError,
                    String.IsNullOrWhiteSpace(fileName) ? Strings.ScriptDefinitionName : System.IO.Path.GetFileName(fileName), imeAst.Member.Extent.Text),
                    imeAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
            }

            return AstVisitAction.Continue;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidUsingWriteHostName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWriteHostCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingWriteHostDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




