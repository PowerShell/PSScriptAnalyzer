// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidDefaultValueForMandatoryParameter: Check if a mandatory parameter does not have a default value.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidDefaultValueForMandatoryParameter : IScriptRule
    {
        /// <summary>
        /// AnalyzeScript: Check if a mandatory parameter has a default value.
        /// </summary>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all ParameterAst which are children of a ParamBlockAst. This
            // doesn't pick up where they appear as children of a
            // FunctionDefinitionAst. i.e.
            //
            // function foo ($a,$b){} -> $a and $b are `ParameterAst`
            //
            // Include only parameters which have a default value (as without
            // one this rule would never alert)
            // Include only parameters where ALL parameter attributes have the
            // mandatory named argument set to true (implicitly or explicitly)

            var mandatoryParametersWithDefaultValues =
                ast.FindAll(testAst => testAst is ParamBlockAst, true)
                    .Cast<ParamBlockAst>()
                    .Where(pb => pb.Parameters?.Count > 0)
                    .SelectMany(pb => pb.Parameters)
                    .Where(paramAst =>
                        paramAst.DefaultValue != null &&
                        HasMandatoryInAllParameterAttributes(
                            paramAst,
                            StringComparer.OrdinalIgnoreCase
                        )
                    );

            // Report diagnostics for each parameter that violates the rule
            foreach (var parameter in mandatoryParametersWithDefaultValues)
            {
                yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.AvoidDefaultValueForMandatoryParameterError,
                            parameter.Name.VariablePath.UserPath
                        ),
                        parameter.Name.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        parameter.Name.VariablePath.UserPath
                    );
            }
        }

        /// <summary>
        /// Determines if a parameter is mandatory in all of its Parameter attributes.
        /// A parameter may have multiple [Parameter] attributes for different parameter sets.
        /// This method returns true only if ALL [Parameter] attributes have Mandatory=true.
        /// </summary>
        /// <param name="paramAst">The parameter AST to examine</param>
        /// <param name="comparer">String comparer for case-insensitive attribute name matching</param>
        /// <returns>
        /// True if the parameter has at least one [Parameter] attribute and ALL of them 
        /// have the Mandatory named argument set to true (explicitly or implicitly).
        /// False if the parameter has no [Parameter] attributes or if any [Parameter] 
        /// attribute does not have Mandatory=true.
        /// </returns>
        private static bool HasMandatoryInAllParameterAttributes(ParameterAst paramAst, StringComparer comparer)
        {
            var parameterAttributes = paramAst.Attributes.OfType<AttributeAst>()
                .Where(attr => IsParameterAttribute(attr.TypeName?.Name, comparer))
                .ToList();

            return parameterAttributes.Count > 0 &&
                   parameterAttributes.All(attr => HasMandatoryArgument(attr, comparer));
        }

        /// <summary>
        /// Determines if an attribute type name represents a PowerShell Parameter attribute.
        /// Checks for both the short form "Parameter" and full form "ParameterAttribute".
        /// </summary>
        /// <param name="typeName">The attribute type name to check</param>
        /// <param name="comparer">String comparer for case-insensitive matching</param>
        /// <returns>
        /// True if the type name is "Parameter" or "ParameterAttribute" (case-insensitive).
        /// False otherwise.
        /// </returns>
        private static bool IsParameterAttribute(string typeName, StringComparer comparer)
        {
            return comparer.Equals(typeName, "parameter");
        }

        /// <summary>
        /// Determines if a Parameter attribute has the Mandatory named argument set to true.
        /// Handles both explicit (Mandatory=$true) and implicit (Mandatory) cases.
        /// Uses the Helper.Instance.GetNamedArgumentAttributeValue method to evaluate
        /// the mandatory argument value.
        /// </summary>
        /// <param name="attr">The Parameter attribute AST to examine</param>
        /// <param name="comparer">String comparer for case-insensitive argument name matching</param>
        /// <returns>
        /// True if the attribute has a "Mandatory" named argument that evaluates to true.
        /// False if there is no "Mandatory" argument or if it evaluates to false.
        /// </returns>
        private static bool HasMandatoryArgument(AttributeAst attr, StringComparer comparer)
        {
            return attr.NamedArguments?.OfType<NamedAttributeArgumentAst>()
                .Any(namedArg =>
                    comparer.Equals(namedArg?.ArgumentName, "mandatory") &&
                    Helper.Instance.GetNamedArgumentAttributeValue(namedArg)
                ) == true;
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.AvoidDefaultValueForMandatoryParameterName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueForMandatoryParameterCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidDefaultValueForMandatoryParameterDescription);
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

