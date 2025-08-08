// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    
    /// <summary>
    /// Rule that identifies parameter blocks with multiple parameters in
    /// the same parameter set that are marked as ValueFromPipeline=true, which
    /// can cause undefined behavior.
    /// </summary>
    public class UseSingleValueFromPipelineParameter : IScriptRule
    {
        private const string AllParameterSetsName = "__AllParameterSets";

        /// <summary>
        /// Analyzes the PowerShell AST for parameter sets with multiple ValueFromPipeline parameters.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violating parameter.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                yield break;
            }
            // Find all param blocks that have a Parameter attribute with
            // ValueFromPipeline set to true.
            var paramBlocks = ast.FindAll(testAst => testAst is ParamBlockAst, true)
                .Where(paramBlock => paramBlock.FindAll(
                    attributeAst => attributeAst is AttributeAst attr &&
                    ParameterAttributeAstHasValueFromPipeline(attr),
                    true
                ).Any());

            foreach (var paramBlock in paramBlocks)
            {
                // Find all parameter declarations in the current param block
                // Convert the generic ast objects into ParameterAst Objects
                // For each ParameterAst, find all it's attributes that have
                // ValueFromPipeline set to true (either explicitly or
                // implicitly). Flatten the results into a single collection of
                // Annonymous objects relating the parameter with it's attribute
                // and then group them by parameter set name.
                // 
                // 
                // https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_parameter_sets?#reserved-parameter-set-name
                // 
                // The default parameter set name is '__AllParameterSets'.
                // Not specifying a parameter set name and using the parameter
                // set name '__AllParameterSets' are equivalent, so we shouldn't
                // treat them like they're different just because one is an
                // empty string and the other is not.
                // 
                // Filter the list to only keep parameter sets that have more
                // than one ValueFromPipeline parameter.
                var parameterSetGroups = paramBlock.FindAll(n => n is ParameterAst, true)
                    .Cast<ParameterAst>()
                    .SelectMany(parameter => parameter.FindAll(
                        a => a is AttributeAst attr && ParameterAttributeAstHasValueFromPipeline(attr),
                        true
                    ).Cast<AttributeAst>().Select(attr => new { Parameter = parameter, Attribute = attr }))
                    .GroupBy(item => GetParameterSetForAttribute(item.Attribute) ?? AllParameterSetsName)
                    .Where(group => group.Count() > 1);


                foreach (var group in parameterSetGroups)
                {
                    // __AllParameterSets being the default name is...obscure.
                    // Instead we'll show the user "default". It's more than
                    // likely the user has not specified a parameter set name,
                    // so default will make sense. If they have used 'default'
                    // as their parameter set name, then we're still correct.
                    var parameterSetName = group.Key == AllParameterSetsName ? "default" : group.Key;

                    // Create a concatenated string of parameter names that
                    // conflict in this parameter set
                    var parameterNames = string.Join(", ", group.Select(item => item.Parameter.Name.VariablePath.UserPath));

                    // We emit a diagnostic record for each offending parameter
                    // attribute in the parameter set so it's obvious where all the
                    // occurrences are.
                    foreach (var item in group)
                    {
                        var message = string.Format(CultureInfo.CurrentCulture,
                            Strings.UseSingleValueFromPipelineParameterError,
                            parameterNames,
                            parameterSetName);

                        yield return new DiagnosticRecord(
                            message,
                            item.Attribute.Extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName,
                            parameterSetName);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether the specified AttributeAst represents a Parameter attribute
        /// that has the ValueFromPipeline named argument set to true (either explicitly or
        /// implicitly).
        /// </summary>
        /// <param name="attributeAst">The Parameter attribute to examine.</param>
        /// <returns>Whether the attribute has the ValueFromPipeline named argument set to true.</returns>
        private static bool ParameterAttributeAstHasValueFromPipeline(AttributeAst attributeAst)
        {
            // Exit quickly if the attribute is null, has no named arguments, or
            // is not a parameter attribute.
            if (attributeAst?.NamedArguments == null ||
                !string.Equals(attributeAst.TypeName?.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return attributeAst.NamedArguments
                .OfType<NamedAttributeArgumentAst>()
                .Any(namedArg => string.Equals(
                    namedArg?.ArgumentName,
                    "ValueFromPipeline",
                    StringComparison.OrdinalIgnoreCase
                // Helper.Instance.GetNamedArgumentAttributeValue handles both explicit ($true)
                // and implicit (no value specified) ValueFromPipeline declarations
                ) && Helper.Instance.GetNamedArgumentAttributeValue(namedArg));
        }

        /// <summary>
        /// Gets the ParameterSetName value from a Parameter attribute.
        /// </summary>
        /// <param name="attributeAst">The Parameter attribute to examine.</param>
        /// <returns>The parameter set name, or null if not found or empty.</returns>
        private static string GetParameterSetForAttribute(AttributeAst attributeAst)
        {
            // Exit quickly if the attribute is null, has no named arguments, or
            // is not a parameter attribute.
            if (attributeAst?.NamedArguments == null ||
                !string.Equals(attributeAst.TypeName.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return attributeAst.NamedArguments
                .OfType<NamedAttributeArgumentAst>()
                .Where(namedArg => string.Equals(
                    namedArg?.ArgumentName,
                    "ParameterSetName",
                    StringComparison.OrdinalIgnoreCase
                ))
                .Select(namedArg => namedArg?.Argument)
                .OfType<StringConstantExpressionAst>()
                .Select(stringConstAst => stringConstAst?.Value)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        public string GetCommonName() => Strings.UseSingleValueFromPipelineParameterCommonName;

        public string GetDescription() => Strings.UseSingleValueFromPipelineParameterDescription;

        public string GetName() => Strings.UseSingleValueFromPipelineParameterName;

        public RuleSeverity GetSeverity() => RuleSeverity.Warning;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}