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
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    class AvoidGlobalAliases : AstVisitor, IScriptRule
    {
        private List<DiagnosticRecord> records;
        private string fileName;

        /// <summary>
        /// Analyzes the ast to check that global aliases are not used.
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

            if (IsScriptModule())
            {
                ast.Visit(this);
            }

            return records;
        }

        #region VisitCommand functions
        /// <summary>
        /// Analyzes a CommandAst, if it is a New-Alias command, the AST is further analyzed.
        /// </summary>
        /// <param name="commandAst">The CommandAst to be analyzed</param>
        /// <returns>AstVisitAction to continue to analyze the ast's children</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            if (IsNewAliasCmdlet(commandAst))
            {
                // check the parameters of the New-Alias cmdlet for scope
                var parameterBindings = StaticParameterBinder.BindCommand(commandAst);

                if (parameterBindings.BoundParameters.ContainsKey("Scope"))
                {
                    var scopeValue = parameterBindings.BoundParameters["Scope"].ConstantValue;

                    if ((scopeValue != null) && (scopeValue.ToString().Equals("Global", StringComparison.OrdinalIgnoreCase)))
                    {
                        records.Add(new DiagnosticRecord(
                                         string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesError),
                                         commandAst.Extent,
                                         GetName(),
                                         DiagnosticSeverity.Warning,
                                         fileName));
                    }
                }
            }

            return AstVisitAction.SkipChildren;
        }
        #endregion

        /// <summary>
        /// Determines if CommandAst is for the "New-Alias" command, checking aliases.
        /// </summary>
        /// <param name="commandAst">CommandAst to validate</param>
        /// <returns>True if the CommandAst is for the "New-Alias" command</returns>
        private bool IsNewAliasCmdlet(CommandAst commandAst)
        {
            if (commandAst == null || commandAst.GetCommandName() == null)
            {
                return false;
            }

            var AliasList = Helper.Instance.CmdletNameAndAliases("New-Alias");
            if (AliasList.Contains(commandAst.GetCommandName()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if analyzing a script module.
        /// </summary>
        /// <returns>True is file name ends with ".psm1"</returns>
        private bool IsScriptModule()
        {
            return fileName.EndsWith(".psm1");
        }

        public string GetCommonName()
        {
             return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesCommonName);
        }

        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesDescription);
        }

        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidGlobalAliasesName);
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
