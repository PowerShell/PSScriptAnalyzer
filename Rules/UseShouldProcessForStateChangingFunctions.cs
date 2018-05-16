// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Management.Automation.Language;
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseShouldProcessForStateChangingFunctions: Analyzes the ast to check if ShouldProcess is included in Advanced functions if the Verb of the function could change system state.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class UseShouldProcessForStateChangingFunctions : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Analyzes the ast to check if ShouldProcess is included in Advanced functions if the Verb of the function could change system state.
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
            IEnumerable<Ast> funcDefWithNoShouldProcessAttrAsts = ast.FindAll(IsStateChangingFunctionWithNoShouldProcessAttribute, true);            
            foreach (FunctionDefinitionAst funcDefAst in funcDefWithNoShouldProcessAttrAsts)
            {
                yield return new DiagnosticRecord(
                    string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsError, funcDefAst.Name), 
                    Helper.Instance.GetScriptExtentForFunctionName(funcDefAst),                    
                    this.GetName(), 
                    DiagnosticSeverity.Warning, 
                    fileName);
            }
                            
        }
        /// <summary>
        /// Checks if the ast defines a state changing function
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>Returns true or false</returns>
        private bool IsStateChangingFunctionWithNoShouldProcessAttribute(Ast ast)
        {
            var funcDefAst = ast as FunctionDefinitionAst;
            // SupportsShouldProcess is not supported in workflows
            if (funcDefAst == null || funcDefAst.IsWorkflow)
            {
                return false;
            }
            return Helper.Instance.IsStateChangingFunctionName(funcDefAst.Name) 
                    && (funcDefAst.Body.ParamBlock == null
                        || funcDefAst.Body.ParamBlock.Attributes == null
                        || !HasShouldProcessTrue(funcDefAst.Body.ParamBlock.Attributes));
        }

        /// <summary>
        /// Checks if an attribute has SupportShouldProcess set to $true
        /// </summary>
        /// <param name="attributeAsts"></param>
        /// <returns>Returns true or false</returns>
        private bool HasShouldProcessTrue(IEnumerable<AttributeAst> attributeAsts)
        {
            var shouldProcessAttributeAst = Helper.Instance.GetShouldProcessAttributeAst(attributeAsts);
            if (shouldProcessAttributeAst == null)
            {
                return false;
            }
            return Helper.Instance.GetNamedArgumentAttributeValue(shouldProcessAttributeAst);
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, this.GetSourceName(), Strings.UseShouldProcessForStateChangingFunctionsName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseShouldProcessForStateChangingFunctionsDescrption);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: built-in, managed or module.
        /// </summary>
        /// <returns>Source type {PS, PSDSC}</returns>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns>Rule severity {Information, Warning, Error}</returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        /// <returns>Source name</returns>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }
}




