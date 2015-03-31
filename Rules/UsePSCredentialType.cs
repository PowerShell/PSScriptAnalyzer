using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Language;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System.ComponentModel.Composition;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.BuiltinRules
{

    /// <summary>
    /// UsePSCredentialType: Analyzes the ast to check that cmdlets that have a Credential parameter accept PSCredential.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class UsePSCredentialType : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that cmdlets that have a Credential parameter accept PSCredential.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> funcDefAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);

            string funcName;

            foreach (FunctionDefinitionAst funcDefAst in funcDefAsts)
            {
                funcName = funcDefAst.Name;

                if (funcDefAst.Parameters != null)
                {
                    foreach (ParameterAst parameter in funcDefAst.Parameters)
                    {
                        if (parameter.Name.VariablePath.UserPath.Equals("Credential", StringComparison.OrdinalIgnoreCase) && parameter.StaticType != typeof(PSCredential))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeError, funcName), funcDefAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                    }
                }

                if (funcDefAst.Body.ParamBlock != null)
                {
                    foreach (ParameterAst parameter in funcDefAst.Body.ParamBlock.Parameters)
                    {
                        if (parameter.Name.VariablePath.UserPath.Equals("Credential", StringComparison.OrdinalIgnoreCase) && parameter.StaticType != typeof(PSCredential))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeError, funcName), funcDefAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                        }
                    }
                }
            }

            foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts)
            {
                if (scriptBlockAst.ParamBlock != null && scriptBlockAst.ParamBlock.Parameters != null)
                {
                    foreach (ParameterAst parameter in scriptBlockAst.ParamBlock.Parameters)
                    {
                        if (parameter.Name.VariablePath.UserPath.Equals("Credential", StringComparison.OrdinalIgnoreCase) && parameter.StaticType != typeof(PSCredential))
                        {
                            yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeErrorSB), scriptBlockAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UsePSCredentialTypeName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UsePSCredentialTypeDescription);
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
