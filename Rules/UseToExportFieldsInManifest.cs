// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Diagnostics;
using System.Text;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseToExportFieldsInManifest: Checks if AliasToExport, CmdletsToExport, FunctionsToExport and VariablesToExport 
    /// fields do not use wildcards and $null in their entries. 
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseToExportFieldsInManifest : IScriptRule
    {

        private const string functionsToExport = "FunctionsToExport";
        private const string cmdletsToExport = "CmdletsToExport";
        private const string aliasesToExport = "AliasesToExport";
                   
        /// <summary>
        /// AnalyzeScript: Analyzes the AST to check if AliasToExport, CmdletsToExport, FunctionsToExport 
        /// and VariablesToExport fields do not use wildcards and $null in their entries. 
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
            
            if (fileName == null || !Helper.IsModuleManifest(fileName))
            {
                yield break;
            }

            // check if valid module manifest
            IEnumerable<ErrorRecord> errorRecord = null;
            PSModuleInfo psModuleInfo = Helper.Instance.GetModuleManifest(fileName, out errorRecord);            
            if ((errorRecord != null && errorRecord.Count() > 0) || psModuleInfo == null)
            {                
                yield break;
            }
            
            var hashtableAst = ast.Find(x => x is HashtableAst, false) as HashtableAst;
            
            if (hashtableAst == null)
            {                                
                yield break;
            }

            string[] manifestFields = { functionsToExport, cmdletsToExport, aliasesToExport };

            foreach(string field in manifestFields)
            {
                IScriptExtent extent;
                if (!HasAcceptableExportField(field, hashtableAst, ast.Extent.Text, out extent) && extent != null)
                {
                    yield return new DiagnosticRecord(
                        GetError(field), 
                        extent, 
                        GetName(), 
                        DiagnosticSeverity.Warning, 
                        fileName,
                        suggestedCorrections: GetCorrectionExtent(field, extent, psModuleInfo));
                }
                else
                {

                }
            }                               
                        
        }

        private string GetListLiteral<T>(Dictionary<string, T> exportedItemsDict)
        {
            const int lineWidth = 64;
            var exportedItems = new SortedDictionary<string, T>(exportedItemsDict);
            if (exportedItems == null || exportedItems.Keys == null)
            {
                return null;
            }
            var sbuilder = new StringBuilder();
            sbuilder.Append("@(");
            var sbuilderInner = new StringBuilder();
            int charLadder = lineWidth;
            int keyCount = exportedItems.Keys.Count;
            foreach (var key in exportedItems.Keys)
            {
                sbuilderInner.Append("'");
                sbuilderInner.Append(key);
                sbuilderInner.Append("'");
                if (--keyCount > 0)
                {
                    sbuilderInner.Append(", ");
                    if (sbuilderInner.Length > charLadder)
                    {
                        charLadder += lineWidth;
                        sbuilderInner.AppendLine();
                        sbuilderInner.Append('\t', 2);
                    }
                }
            }
            sbuilder.Append(sbuilderInner);
            sbuilder.Append(")");
            return sbuilder.ToString();
        }


        private List<CorrectionExtent> GetCorrectionExtent(string field, IScriptExtent extent, PSModuleInfo psModuleInfo)
        {
            Debug.Assert(field != null);            
            Debug.Assert(psModuleInfo != null);
            Debug.Assert(extent != null);
            var corrections = new List<CorrectionExtent>();
            string correctionText = null;
            switch (field)
            {
                case functionsToExport:
                    correctionText = GetListLiteral(psModuleInfo.ExportedFunctions);
                    break;
                case cmdletsToExport:
                    correctionText = GetListLiteral(psModuleInfo.ExportedCmdlets);
                    break;
                case aliasesToExport:
                    correctionText = GetListLiteral(psModuleInfo.ExportedAliases);
                    break;
                default:
                    throw new NotImplementedException(string.Format("{0} not implemented", field));
            }
            string description = string.Format(
                Strings.UseToExportFieldsInManifestCorrectionDescription,
                extent.Text,
                correctionText);
            corrections.Add(new CorrectionExtent(
                extent.StartLineNumber,
                extent.EndLineNumber,
                extent.StartColumnNumber,
                extent.EndColumnNumber,
                correctionText,
                extent.File,
                description));
            return corrections;
        }

        ///// <summary>
        ///// Checks if the manifest file is valid. 
        ///// </summary>
        ///// <param name="ast"></param>
        ///// <param name="fileName"></param>
        ///// <returns>A boolean value indicating the validity of the manifest file.</returns>
        //private bool IsValidManifest(Ast ast, string fileName)
        //{
        //    var missingManifestRule = new MissingModuleManifestField();
        //    return !missingManifestRule.AnalyzeScript(ast, fileName).GetEnumerator().MoveNext();

        //}

        /// <summary>
        /// Checks if the ast contains wildcard character.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private bool HasWildcardInExpression(Ast ast)
        {
            var strConstExprAst = ast as StringConstantExpressionAst;
            return strConstExprAst != null && WildcardPattern.ContainsWildcardCharacters(strConstExprAst.Value);
        }

        /// <summary>
        /// Checks if the ast contains null expression.
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private bool HasNullInExpression(Ast ast)
        {
            var varExprAst = ast as VariableExpressionAst;
            return varExprAst != null
                    && varExprAst.VariablePath.IsUnqualified
                    && varExprAst.VariablePath.UserPath.Equals("null", StringComparison.OrdinalIgnoreCase);
        }
                
        /// <summary>
        /// Checks if the *ToExport fields are explicitly set to arrays, eg. @(...), and the array entries do not contain any wildcard.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hast"></param>
        /// <param name="scriptText"></param>
        /// <param name="extent"></param>
        /// <returns>A boolean value indicating if the the ToExport fields are explicitly set to arrays or not.</returns>
        private bool HasAcceptableExportField(string key, HashtableAst hast, string scriptText, out IScriptExtent extent)
        {
            extent = null;
            foreach (var pair in hast.KeyValuePairs)
            {
                var keyStrConstAst = pair.Item1 as StringConstantExpressionAst;
                if (keyStrConstAst != null && keyStrConstAst.Value.Equals(key, StringComparison.OrdinalIgnoreCase))                    
                {
                    // Checks for wildcard character in the entry.
                    var astWithWildcard = pair.Item2.Find(HasWildcardInExpression, false);
                    if (astWithWildcard != null)
                    {
                        extent = astWithWildcard.Extent;
                        return false;
                    }
                    else
                    {
                        // Checks for $null in the entry.                           
                        var astWithNull = pair.Item2.Find(HasNullInExpression, false);
                        if (astWithNull != null)
                        {
                            extent = astWithNull.Extent;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }       

        
        /// <summary>
        /// Gets the error string of the rule.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public string GetError(string field)
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestError, field);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseToExportFieldsInManifestName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return String.Format(CultureInfo.CurrentCulture, Strings.UseToExportFieldsInManifestDescription);
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
