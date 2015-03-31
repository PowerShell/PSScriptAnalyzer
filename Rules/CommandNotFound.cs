using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using Microsoft.Windows.Powershell.ScriptAnalyzer;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// CommandNotFound: Check that all the commands in the script exist.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class CommandNotFound : SkipNamedBlock, IScriptRule
    {
        IEnumerable<Ast> functionAsts = null;

        /// <summary>
        /// AnalyzeScript: Run get-command to check that all commands are found
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            ClearList();
            this.AddNames(new List<string>() { "Configuration", "Workflow" });
            this.fileName = fileName;
            functionAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            ast.Visit(this);

            return DiagnosticRecords;
        }

        /// <summary>
        /// Visit CommandAst to check that commands are found
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns></returns>
        public override AstVisitAction VisitCommand(CommandAst cmdAst)
        {
            var astAction = base.VisitCommand(cmdAst);

            if (astAction == AstVisitAction.SkipChildren)
            {
                return AstVisitAction.SkipChildren;
            }

            if (cmdAst != null && cmdAst.GetCommandName() != null
                && !TestCommandName(cmdAst))
            {
                DiagnosticRecords.Add(new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.CommandNotFoundError, cmdAst.GetCommandName()),
                    cmdAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName));
            }

            return AstVisitAction.Continue;
        }

        private bool TestCommandName(CommandAst cmdAst)
        {
            if (Helper.Instance.GetCommandInfo(cmdAst.GetCommandName()) != null
                || Helper.Instance.GetCommandInfo(Helper.Instance.GetCmdletNameFromAlias(cmdAst.GetCommandName())) != null)
            {
                return true;
            }

            if (functionAsts != null)
            {
                Ast targetAst = functionAsts.FirstOrDefault<Ast>(
                    item => (item as FunctionDefinitionAst).Name.Equals(cmdAst.GetCommandName(), StringComparison.OrdinalIgnoreCase));

                if (targetAst != null)
                {
                    Ast parent = targetAst.Parent;
                    while (parent != null)
                    {
                        if (parent.Extent.StartOffset <= targetAst.Extent.EndOffset && parent.Extent.EndOffset >= targetAst.Extent.StartOffset)
                        {
                            Ast match = parent.Find(item => item.Extent.StartOffset == cmdAst.Extent.StartOffset
                                && item.Extent.EndOffset == cmdAst.Extent.EndOffset
                                && cmdAst.Extent.Text.Equals(item.Extent.Text, StringComparison.OrdinalIgnoreCase), true);

                            if (match != null)
                            {
                                return true;
                            }
                        }

                        parent = parent.Parent;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.CommandNotFoundName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.CommandNotFoundCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.CommandNotFoundDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
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
