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
using Microsoft.Windows.Powershell.ScriptAnalyzer;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// CommandNotFound: Check that all the commands in the script exist.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UseTypeAtVariableAssignment : IScriptRule
    {
        /// <summary>
        /// AnalyzerScript: Check that variable types are used at assignment
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all AssignmentStatementAsts, then check the type of left side.
            IEnumerable<Ast> foundAsts = ast.FindAll(testAst => testAst is AssignmentStatementAst, true);

            // Groups AssignmentStatementAsts by function name property.
            // If the variable is not defined in a function, it will be categorized to 'script'.
            // Otherwise, the category name is a function name.
            IEnumerable<IGrouping<string, Ast>> varByScopes = foundAsts.GroupBy(item =>
                GetScopeName(((AssignmentStatementAst)item)));

            foreach (IGrouping<string, Ast> varByScope in varByScopes)
            {
                IEnumerable<IGrouping<string, Ast>> varByNames = varByScope.GroupBy(item =>
                    ((AssignmentStatementAst)item).Left.Extent.Text.ToLower());

                foreach (IGrouping<string, Ast> varByName in varByNames)
                {
                    if (varByName.ToList().Count(testAst =>
                        IsInFlowControlStatement(testAst) == true) == varByName.Count())
                    {
                        // Finds all AssignmentStatementAsts inside a IfStatementAst/SwitchStatementAst. 
                        foreach (AssignmentStatementAst gpAst in varByName)
                        {
                            if (gpAst.Left is VariableExpressionAst && !Helper.Instance.HasSpecialVars((gpAst.Left as VariableExpressionAst).VariablePath.UserPath))
                            {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseTypeAtVariableAssignmentError, gpAst.Left.Extent.Text),
                                    gpAst.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
                            }
                        }
                    }
                    else
                    {
                        // Finds first AssignmentStatementAst from a given list.
                        AssignmentStatementAst asAst = varByName.ToList().OrderBy(
                            item => item.Extent.StartLineNumber).First() as AssignmentStatementAst;

                        if (asAst.Left is VariableExpressionAst && !Helper.Instance.HasSpecialVars((asAst.Left as VariableExpressionAst).VariablePath.UserPath))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseTypeAtVariableAssignmentError, ((AssignmentStatementAst)asAst).Left.Extent.Text),
                                asAst.Extent, GetName(), DiagnosticSeverity.Strict, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a variable in within If/Swtich statement
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private bool IsInFlowControlStatement(Ast ast)
        {
            Ast parentAst = ast;
            while (null != parentAst)
            {
                if (parentAst is IfStatementAst ||
                    parentAst is SwitchStatementAst)
                    break;
                parentAst = parentAst.Parent;
            }

            // If parentAst is a IfStatementAst or SwitchStatementAst, returns true. 
            // Otherwise, returns false.
            if (null != parentAst)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Return the scope name of a variable. It could be either the nmae of the funciton or the Script.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private string GetScopeName(Ast ast)
        {
            Ast parentAst = ast;
            while (null != parentAst)
            {
                if (parentAst is FunctionDefinitionAst)
                    break;
                parentAst = parentAst.Parent;
            }

            // If parentAst is a FunctionDefinitionAst, returns the name of that function. 
            // Otherwise, returns Script.
            if (null != parentAst)
                return ((FunctionDefinitionAst)parentAst).Name;
            else
                return "Script";
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseTypeAtVariableAssignmentName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseTypeAtVariableAssignmentCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseTypeAtVariableAssignmentDescription);
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
