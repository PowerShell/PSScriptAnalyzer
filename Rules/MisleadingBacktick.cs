// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// MisleadingBacktick: Checks that lines don't end with a backtick followed by whitespace
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class MisleadingBacktick : IScriptRule
    {
        private static Regex TrailingEscapedWhitespaceRegex = new Regex(@"`\s+$");
        private static Regex NewlineRegex = new Regex("\r?\n");

        /// <summary>
        /// MisleadingBacktick: Checks that lines don't end with a backtick followed by a whitespace
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            string[] lines = NewlineRegex.Split(ast.Extent.Text);

            if((ast.Extent.EndLineNumber - ast.Extent.StartLineNumber + 1) != lines.Length)
            {
                // Did not match the number of lines that the extent indicated
                yield break;
            }

            foreach (int i in Enumerable.Range(0, lines.Length))
            {
                string line = lines[i];

                Match match = TrailingEscapedWhitespaceRegex.Match(line);

                if(match.Success)
                {
                    int lineNumber = ast.Extent.StartLineNumber + i;

                    var start = new ScriptPosition(fileName, lineNumber, match.Index + 1, line);
                    var end = new ScriptPosition(fileName, lineNumber, match.Index + match.Length + 1, line);
                    var extent = new ScriptExtent(start, end);
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.MisleadingBacktickError),
                            extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName,
                            suggestedCorrections: GetCorrectionExtent(extent));
                }
            }
        }

        /// <summary>
        /// Creates a list containing suggested correction
        /// </summary>
        /// <param name="cmdAst"></param>
        /// <returns>Returns a list of suggested corrections</returns>
        private List<CorrectionExtent> GetCorrectionExtent(IScriptExtent violationExtent)
        {
            var corrections = new List<CorrectionExtent>();
            string description = "Remove trailing whilespace";
            corrections.Add(new CorrectionExtent(
                violationExtent.StartLineNumber ,
                violationExtent.EndLineNumber,
                violationExtent.StartColumnNumber + 1,
                violationExtent.EndColumnNumber,
                String.Empty,
                violationExtent.File,
                description));
            return corrections;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.MisleadingBacktickName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.MisleadingBacktickCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.MisleadingBacktickDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
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
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




