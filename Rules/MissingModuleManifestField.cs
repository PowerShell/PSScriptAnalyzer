// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// MissingModuleManifestField: Run Test Module Manifest to check that none of the required fields are missing.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class MissingModuleManifestField : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Run Test Module Manifest to check that none of the required fields are missing. From the ILintScriptRule interface.
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The script's file name</param>
        /// <returns>A List of diagnostic results of this rule</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }
            if (fileName == null)
            {
                yield break;
            }
            if (Helper.IsModuleManifest(fileName))
            {
                IEnumerable<ErrorRecord> errorRecords;
                var psModuleInfo = Helper.Instance.GetModuleManifest(fileName, out errorRecords);
                if (errorRecords != null)
                {
                    foreach (var errorRecord in errorRecords)
                    {
                        if (Helper.IsMissingManifestMemberException(errorRecord))
                        {
                            System.Diagnostics.Debug.Assert(
                                errorRecord.Exception != null && !String.IsNullOrWhiteSpace(errorRecord.Exception.Message), 
                                Strings.NullErrorMessage);
                            var hashTableAst = ast.Find(x => x is HashtableAst, false);
                            if (hashTableAst == null)
                            {
                                yield break;
                            }
                            yield return new DiagnosticRecord(
                                errorRecord.Exception.Message, 
                                hashTableAst.Extent, 
                                GetName(), 
                                DiagnosticSeverity.Warning, 
                                fileName,
                                suggestedCorrections:GetCorrectionExtent(hashTableAst as HashtableAst));
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Gets the correction extent
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>A List of CorrectionExtent</returns>
        private List<CorrectionExtent> GetCorrectionExtent(HashtableAst ast)
        {
            int startLineNumber;
            int startColumnNumber;

            // for empty hashtable insert after after "@{"
            if (ast.KeyValuePairs.Count == 0)
            {
                // check if ast starts with "@{"
                if (ast.Extent.Text.IndexOf("@{") != 0)
                {
                    return null;
                }
                startLineNumber = ast.Extent.StartLineNumber;
                startColumnNumber = ast.Extent.StartColumnNumber + 2; // 2 for "@{",
            }
            else // for non-empty hashtable insert after the last element
            {
                int maxLine = 0;
                int lastCol = 0;
                foreach (var keyVal in ast.KeyValuePairs)
                {
                    if (keyVal.Item2.Extent.EndLineNumber > maxLine)
                    {
                        maxLine = keyVal.Item2.Extent.EndLineNumber;
                        lastCol = keyVal.Item2.Extent.EndColumnNumber;
                    }
                }
                startLineNumber = maxLine;
                startColumnNumber = lastCol;
            }

            var correctionExtents = new List<CorrectionExtent>();
            string fieldName = "ModuleVersion";
            string fieldValue = "1.0.0.0";
            string description = string.Format(
                CultureInfo.CurrentCulture,
                Strings.MissingModuleManifestFieldCorrectionDescription,
                fieldName,
                fieldValue);
            var correctionTextTemplate = string.Concat(Environment.NewLine,
                "# Version number of this module.", Environment.NewLine,
                "{0} = '{1}'", Environment.NewLine);
            var correctionText = string.Format(
                correctionTextTemplate,
                fieldName,
                fieldValue);
            var correctionExtent = new CorrectionExtent(
                startLineNumber,
                startLineNumber,
                startColumnNumber,
                startColumnNumber,
                correctionText,
                ast.Extent.File,
                description);
            correctionExtents.Add(correctionExtent);
            return correctionExtents;
        }
        
        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.MissingModuleManifestFieldName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.MissingModuleManifestFieldCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.MissingModuleManifestFieldDescription);
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
