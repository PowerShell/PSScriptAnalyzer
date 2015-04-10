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
    /// CmdletSingularNoun: Analyzes scripts to check that all defined cmdlets use singular nouns.
    /// 
    /// </summary>
    [Export(typeof(IScriptRule))]
    public class CmdletSingularNoun : IScriptRule {
        /// <summary>
        /// Checks that all defined cmdlet use singular noun
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName) {
            if (ast == null) throw new ArgumentNullException(Strings.NullCommandInfoError);

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            char[] funcSeperator = { '-' };
            string[] funcNamePieces = new string[2];

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                if (funcAst.Name != null && funcAst.Name.Contains('-'))
                {
                    funcNamePieces = funcAst.Name.Split(funcSeperator);
                    String noun = funcNamePieces[1];
                    var ps = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

                    if (ps.IsPlural(noun))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsError, funcAst.Name),
                            funcAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseSingularNounsName);
        }

        /// <summary>
        /// GetName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsDescription);
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
