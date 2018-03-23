// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.IO;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidAlias: Check if help file uses utf8 encoding
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseUTF8EncodingForHelpFile : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: check if the help file uses something other than utf8
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            // we are given a script definition, do not analyze
            // this rule is not applicable for that
            if (String.IsNullOrWhiteSpace(fileName))
            {
                yield break;
            }

            if (!String.IsNullOrWhiteSpace(fileName) && Helper.Instance.IsHelpFile(fileName))
            {
                using (var fileStream = File.Open(fileName, FileMode.Open))
                using (var reader = new System.IO.StreamReader(fileStream, true))
                {
                    reader.ReadToEnd();
                    if (reader.CurrentEncoding != System.Text.Encoding.UTF8)
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseUTF8EncodingForHelpFileError, System.IO.Path.GetFileName(fileName), reader.CurrentEncoding),
                            null, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseUTF8EncodingForHelpFileName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseUTF8EncodingForHelpFileCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseUTF8EncodingForHelpFileDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
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
        /// GetSourceName: Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




