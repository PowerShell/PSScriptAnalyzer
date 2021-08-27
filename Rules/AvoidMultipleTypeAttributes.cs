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
    /// AvoidMultipleTypeAttributes: Check that parameter does not be assigned to multiple types.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public sealed class AvoidMultipleTypeAttributes : IScriptRule
    {
        /// <summary>
        /// AvoidMultipleTypeAttributes: Check that parameter does not be assigned to multiple types.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast is null) 
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            // Finds all ParamAsts.
            IEnumerable<Ast> paramAsts = ast.FindAll(testAst => testAst is ParameterAst, searchNestedScriptBlocks: true);

            // Iterates all ParamAsts and check the number of its types.
            foreach (ParameterAst paramAst in paramAsts)
            {
                if (paramAst.Attributes.Where(typeAst => typeAst is TypeConstraintAst).Count() > 1)
                {
                    yield return new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, Strings.AvoidMultipleTypeAttributesError, paramAst.Name),
                        paramAst.Name.Extent,
                        GetName(), 
                        DiagnosticSeverity.Warning, 
                        fileName);
                }
            }
        }
    
        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidMultipleTypeAttributesName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidMultipleTypeAttributesCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidMultipleTypeAttributesDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning or information.
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
