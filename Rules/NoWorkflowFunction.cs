using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;
using NanoServerCompliance;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// NoWorkflowFunction: Check if workflow block is used. Workflow is not supported on NanoServer.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class NoWorkflowFunction : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if workflow block is used.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);
            IEnumerable<Ast> funcs = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            foreach (FunctionDefinitionAst func in funcs)
            {
                if(func.IsWorkflow)
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.WorkFlowFunctionNotSupportedOnNanoError, func.Name),
    func.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }        

        }


        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NoWorkflowFunctionsOnNanoName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.CmdletNotAvailableOnNanoCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NoWorkflowFunctionsOnNanoDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
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
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}
