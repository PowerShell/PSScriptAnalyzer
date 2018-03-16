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

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseIdenticalParametersDSC: Check that the Test-TargetResource and
    /// Set-TargetResource have identical parameters.
    /// </summary>
#if !CORECLR
[Export(typeof(IDSCResourceRule))]
#endif
    public class UseIdenticalParametersDSC : IDSCResourceRule
    {
        /// <summary>
        /// AnalyzeDSCResource: Analyzes given DSC Resource
        /// </summary>
        /// <param name="ast">The script's ast</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCResource(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // parameters
            Dictionary<string, ParameterAst> paramNames = new Dictionary<string, ParameterAst>(StringComparer.OrdinalIgnoreCase);

            IEnumerable<Ast> functionDefinitionAsts = Helper.Instance.DscResourceFunctions(ast);

            if (functionDefinitionAsts.Count() == 2)
            {
                var firstFunc = functionDefinitionAsts.First();
                var secondFunc = functionDefinitionAsts.Last();

                IEnumerable<Ast> funcParamAsts = firstFunc.FindAll(item => item is ParameterAst, true);
                IEnumerable<Ast> funcParamAsts2 = secondFunc.FindAll(item => item is ParameterAst, true);

                if (funcParamAsts.Count() != funcParamAsts2.Count())
                {
                    yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalParametersDSCError),
                        firstFunc.Extent, GetName(), DiagnosticSeverity.Error, fileName);
                }

                foreach (ParameterAst paramAst in funcParamAsts)
                {
                    paramNames[paramAst.Name.VariablePath.UserPath] = paramAst;
                }

                foreach (ParameterAst paramAst in funcParamAsts2)
                {
                    if (!paramNames.ContainsKey(paramAst.Name.VariablePath.UserPath)
                        || !CompareParamAsts(paramAst, paramNames[paramAst.Name.VariablePath.UserPath]))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalParametersDSCError),
                            paramAst.Extent, GetName(), DiagnosticSeverity.Error, fileName);   
                    }
                }
            }
        }

        /// <summary>
        /// AnalyzeDSCClass: This function returns nothing in the case of dsc class.
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeDSCClass(Ast ast, string fileName)
        {
            return Enumerable.Empty<DiagnosticRecord>();
        }

        // We assume they have the same name
        private bool CompareParamAsts(ParameterAst paramAst1, ParameterAst paramAst2)
        {
            if (paramAst1.StaticType != paramAst2.StaticType)
            {
                return false;
            }

            if ((paramAst1.Attributes == null && paramAst2.Attributes != null)
                || (paramAst1.Attributes != null && paramAst2.Attributes == null))
            {
                return false;
            }

            if (paramAst1.Attributes != null && paramAst2.Attributes != null)
            {
                if (paramAst1.Attributes.Count() != paramAst2.Attributes.Count())
                {
                    return false;
                }

                HashSet<string> attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var attribute in paramAst1.Attributes)
                {
                    attributes.Add(attribute.TypeName.FullName);
                }

                foreach (var attribute in paramAst2.Attributes)
                {
                    if (!attributes.Contains(attribute.TypeName.FullName))
                    {
                        return false;
                    }
                }

            }

            return true;
        }
        

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {            
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.UseIdenticalParametersDSCName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the Common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalParametersDSCCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.UseIdenticalParametersDSCDescription);
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
            return RuleSeverity.Error;
        }

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.DSCSourceName);
        }
    }

}



