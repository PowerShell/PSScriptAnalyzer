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
    /// AvoidDefaultTrueValueSwitchParameter: Check that switch parameter does not default to true.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidDefaultTrueValueSwitchParameter : IScriptRule
    {
        /// <summary>
        /// AvoidUsingPlainTextForPassword: Check that switch parameter does not default to true.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Finds all ParamAsts.
            IEnumerable<Ast> paramAsts = ast.FindAll(testAst => testAst is ParameterAst, true);

            // Iterates all ParamAsts and check if any are switch.
            foreach (ParameterAst paramAst in paramAsts)
            {
                if (paramAst.Attributes.Any(attr => attr.TypeName.GetReflectionType() == typeof(System.Management.Automation.SwitchParameter))
                    && paramAst.DefaultValue != null && String.Equals(paramAst.DefaultValue.Extent.Text, "$true", StringComparison.OrdinalIgnoreCase))
                {
                    if (String.IsNullOrWhiteSpace(fileName))
                    {
                        yield return new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueSwitchParameterErrorScriptDefinition),
                            paramAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
                    }
                    else
                    {
                        yield return new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueSwitchParameterError, System.IO.Path.GetFileName(fileName)),
                            paramAst.Extent, GetName(), DiagnosticSeverity.Warning, fileName);
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
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidDefaultValueSwitchParameterName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueSwitchParameterCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueSwitchParameterDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule, builtin, managed or module.
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




