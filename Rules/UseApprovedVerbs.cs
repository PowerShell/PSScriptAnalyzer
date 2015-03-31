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
    /// UseApprovedVerbs: Analyzes CommandInfos to check that all defined cmdlets use approved verbs.
    /// </summary>
    [Export(typeof(ICommandRule))]
    public class UseApprovedVerbs : ICommandRule {
        /// <summary>
        /// AnalyzeCommand: Analyzes command infos to check that all defined cmdlets use approved verbs.
        /// </summary>
        /// <param name="commandInfo">The current command info from the script</param>
        /// <param name="extent">The current position in the script</param>
        /// <param name="fileName">The name of the script</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeCommand(CommandInfo commandInfo, IScriptExtent extent, string fileName) {
            if (commandInfo == null) throw new ArgumentNullException(Strings.NullCommandInfoError);

            List<string> approvedVerbs = typeof(VerbsCommon).GetFields().Concat<FieldInfo>(
                typeof(VerbsCommunications).GetFields()).Concat<FieldInfo>(
                typeof(VerbsData).GetFields()).Concat<FieldInfo>(
                typeof(VerbsDiagnostic).GetFields()).Concat<FieldInfo>(
                typeof(VerbsLifecycle).GetFields()).Concat<FieldInfo>(
                typeof(VerbsSecurity).GetFields()).Concat<FieldInfo>(
                typeof(VerbsOther).GetFields()).Select<FieldInfo, String>(
                item => item.Name).ToList();

            string funcName;
            char[] funcSeperator = { '-' };
            string[] funcNamePieces = new string[2];
            string verb;

            funcName = commandInfo.Name;

            if (funcName != null && funcName.Contains('-')) {
                funcNamePieces = funcName.Split(funcSeperator);
                verb = funcNamePieces[0];

                if (!approvedVerbs.Contains(verb, StringComparer.OrdinalIgnoreCase)) {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsError, funcName), extent, GetName(), DiagnosticSeverity.Warning, fileName);
                }
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseApprovedVerbsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseApprovedVerbsDescription);
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
