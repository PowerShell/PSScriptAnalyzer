using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidReservedParams: Analyzes the ast to check for reserved parameters in function definitions.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidReservedParams : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check for reserved parameters in function definitions.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            string paramName;

            List<string> commonParamNames = typeof(CommonParameters).GetProperties().Select(param => param.Name).ToList();

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                IEnumerable<ParameterAst> parameters = null;
                if (funcAst.Parameters != null)
                {
                    parameters = funcAst.Parameters;
                }
                // Check param block
                else
                {
                    if (funcAst.Body != null && funcAst.Body.ParamBlock != null && funcAst.Body.ParamBlock.Parameters != null)
                    {
                        parameters = funcAst.Body.ParamBlock.Parameters;
                    }
                }

                if (parameters != null)
                {
                    foreach (ParameterAst paramAst in parameters)
                    {
                        paramName = paramAst.Name.VariablePath.UserPath;

                        if (commonParamNames.Contains(paramName, StringComparer.OrdinalIgnoreCase))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReservedParamsError, funcAst.Name, paramName),
                                paramAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ReservedParamsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReservedParamsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReservedParamsDescription);
        }

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
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
