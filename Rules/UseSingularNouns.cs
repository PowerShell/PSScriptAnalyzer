﻿//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if CORECLR
using Pluralize.NET;
#else
using System.ComponentModel.Composition;
using System.Data.Entity.Design.PluralizationServices;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// CmdletSingularNoun: Analyzes scripts to check that all defined cmdlets use singular nouns.
    /// 
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class CmdletSingularNoun : ConfigurableRule 
    {
        [ConfigurableRuleProperty(defaultValue: new string[] { "Data", "Windows" })]
        public string[] NounAllowList { get; set; }

        public CmdletSingularNoun()
        {
            Enable = true;
        }

        /// <summary>
        /// Checks that all defined cmdlet use singular noun
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullCommandInfoError);

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            var pluralizer = new PluralizerProxy();

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                if (funcAst.Name == null || !funcAst.Name.Contains('-'))
                {
                    continue;
                }

                string noun = GetLastWordInCmdlet(funcAst.Name);

                if (noun is null)
                {
                    continue;
                }

                if (pluralizer.CanOnlyBePlural(noun))
                {
                    if (NounAllowList.Contains(noun, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    IScriptExtent extent = Helper.Instance.GetScriptExtentForFunctionName(funcAst);

                    if (extent is null)
                    {
                        extent = funcAst.Extent;
                    }

                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsError, funcAst.Name),
                        extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        funcAst.Name,
                        suggestedCorrections: new CorrectionExtent[] { GetCorrection(pluralizer, extent, funcAst.Name, noun) });
                }
            }

        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseSingularNounsName);
        }

        /// <summary>
        /// GetName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseSingularNounsDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        private CorrectionExtent GetCorrection(PluralizerProxy pluralizer, IScriptExtent extent, string commandName, string noun)
        {
            string singularNoun = pluralizer.Singularize(noun);
            string newCommandName = commandName.Substring(0, commandName.Length - noun.Length) + singularNoun;
            return new CorrectionExtent(extent, newCommandName, extent.File, $"Singularized correction of '{extent.Text}'");
        }

        /// <summary>
        /// Gets the last word in a standard syntax, CamelCase cmdlet.
        /// If the cmdlet name is non-standard, returns null.
        /// </summary>
        private string GetLastWordInCmdlet(string cmdletName)
        {
            if (string.IsNullOrEmpty(cmdletName))
            {
                return null;
            }

            // Cmdlet doesn't use CamelCase, so assume it's something like an initialism that shouldn't be singularized
            if (!char.IsLower(cmdletName[cmdletName.Length - 1]))
            {
                return null;
            }

            for (int i = cmdletName.Length - 1; i >= 0; i--)
            {
                if (cmdletName[i] == '-')
                {
                    // We got to the dash without seeing a CamelCase word, so nothing to singularize
                    return null;
                }

                // We just changed from lower case to upper, so we have the end word
                if (char.IsUpper(cmdletName[i]))
                {
                    return cmdletName.Substring(i);
                }
            }

            // We shouldn't ever get here since we should always eventually hit a '-'
            // But if we do, assume this isn't supported cmdlet name
            return null;
        }

#if CORECLR
        private class PluralizerProxy
        {
            private readonly Pluralizer _pluralizer;

            public PluralizerProxy()
            {
                _pluralizer = new Pluralizer();
            }

            public bool CanOnlyBePlural(string noun) =>
                !_pluralizer.IsSingular(noun) && _pluralizer.IsPlural(noun);

            public string Singularize(string noun) => _pluralizer.Singularize(noun);
        }
#else
        private class PluralizerProxy
        {
            private static readonly PluralizationService s_pluralizationService = PluralizationService.CreateService(
                CultureInfo.GetCultureInfo("en-us"));

            public bool CanOnlyBePlural(string noun) =>
                !s_pluralizationService.IsSingular(noun) && s_pluralizationService.IsPlural(noun);

            public string Singularize(string noun) => s_pluralizationService.Singularize(noun);
        }
#endif
    }

}
