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
    /// AvoidOneCharName: Analyzes ast to check that cmdlets and parameters have more than one character.
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class AvoidOneCharName : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check that cmdlets and parameters have more than one character.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> funcAsts = ast.FindAll(testAst => testAst is FunctionDefinitionAst, true);
            IEnumerable<Ast> scriptBlockAsts = ast.FindAll(testAst => testAst is ScriptBlockAst, true);
            IEnumerable<Ast> paramAsts;

            if (funcAsts != null) {
                foreach (FunctionDefinitionAst funcAst in funcAsts) {
                    if (funcAst.Name.Length < 2) {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.OneCharErrorCmdlet, funcAst.Name), funcAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }

                    paramAsts = funcAst.FindAll(testAst => testAst is ParameterAst, false);

                    if (paramAsts != null) {
                        foreach (ParameterAst paramAst in paramAsts) {
                            if (paramAst.Name.VariablePath.UserPath.Length < 2) {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.OneCharErrorParameter, funcAst.Name, paramAst.Name.VariablePath.UserPath), funcAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
                    }
                }
            }

            if (scriptBlockAsts != null) {
                foreach (ScriptBlockAst scriptBlockAst in scriptBlockAsts) {
                    paramAsts = scriptBlockAst.FindAll(testAst => testAst is ParameterAst, false);

                    if (paramAsts != null) {
                        foreach (ParameterAst paramAst in paramAsts) {
                            if (paramAst.Name.VariablePath.UserPath.Length < 2) {
                                yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.OneCharErrorParameterSB, paramAst.Name.VariablePath.UserPath), scriptBlockAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName() {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.OneCharName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.OneCharCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.OneCharDescription);
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
