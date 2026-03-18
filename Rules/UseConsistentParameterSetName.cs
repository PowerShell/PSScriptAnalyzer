// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// UseConsistentParameterSetName: Check for case-sensitive parameter set
    /// name mismatches, missing default parameter set names, and parameter set
    /// names containing new lines.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class UseConsistentParameterSetName : ConfigurableRule
    {

        private const string AllParameterSetsName = "__AllParameterSets";

        /// <summary>
        /// AnalyzeScript: Check for parameter set name issues.
        /// </summary>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            var allParameterBlocks = ast
                .FindAll(testAst => testAst is ParamBlockAst, true)
                .Cast<ParamBlockAst>()
                .Where(pb => pb.Parameters?.Count > 0);

            foreach (var paramBlock in allParameterBlocks)
            {
                // If the paramblock has no parameters, skip it
                if (paramBlock.Parameters.Count == 0)
                {
                    continue;
                }

                // Get the CmdletBinding attribute and default parameter set name
                // Or null if not present
                var cmdletBindingAttr = Helper.Instance.GetCmdletBindingAttributeAst(paramBlock.Attributes);
                var defaultParamSetName = GetNamedArgumentValue(cmdletBindingAttr, "DefaultParameterSetName");

                // For each parameter block, build up a list of all the parameters
                // and the parameter sets in which they appear.
                List<ParameterSetInfo> paramBlockInfo = new List<ParameterSetInfo>();

                foreach (var parameter in paramBlock.Parameters)
                {
                    // If the parameter has no attributes, it is part of all
                    // parameter sets. We can ignore it for these checks.
                    if (parameter.Attributes.Count == 0)
                    {
                        continue;
                    }

                    // For each parameter attribute a parameter has, extract
                    // the parameter set and add it to our knowledge of the
                    // param block.
                    foreach (var attribute in parameter.Attributes.Where(attr => attr is AttributeAst).Cast<AttributeAst>())
                    {
                        if (string.Equals(attribute.TypeName?.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
                        {
                            var parameterSetName = GetNamedArgumentValue(attribute, "ParameterSetName", AllParameterSetsName);
                            paramBlockInfo.Add(new ParameterSetInfo(parameter.Name.VariablePath.UserPath, parameterSetName, attribute));
                        }
                    }
                }

                // We now have a picture of the parameters and parameterset
                // usage of this paramblock. We can make each check.

                // Check 1: Default parameter set name
                // -------------------------------------------------------------
                // If we have parameter sets in use and the CmdletBinding
                // attribute, but no default specified, warn about this.
                if (string.IsNullOrEmpty(defaultParamSetName) &&
                    cmdletBindingAttr != null &&
                    paramBlockInfo.Any(p => p.ParameterSetName != AllParameterSetsName)
                )
                {
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.UseConsistentParameterSetNameMissingDefaultError),
                        cmdletBindingAttr?.Extent ?? paramBlock.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName);
                }

                // Check 2: Parameter Declared Multiple Times in Same Set
                // -------------------------------------------------------------
                // If any parameter has more than one parameter attribute for
                // the same parameterset, warn about each instance.
                // Parameters cannot be declared multiple times in the same set.
                // Calling a function that has a parameter declared multiple
                // times in the same parameterset is a runtime exception -
                // specifically a [System.Management.Automation.MetadataException]
                // It'd be better to know before runtime.
                // We use the same message text as the MetadataException for
                // consistency
                var duplicateAttributes = paramBlockInfo
                    .GroupBy(p => new { p.ParameterName, p.ParameterSetName })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                foreach (var duplicate in duplicateAttributes)
                {
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.UseConsistentParameterSetNameMultipleDeclarationsError,
                            duplicate.ParameterName,
                            duplicate.ParameterSetName),
                        duplicate.ParameterAttributeAst.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName);
                }

                // Check 3: Validate Default Parameter Set
                // -------------------------------------------------------------
                // If a default parameter set is specified and matches one of
                // the used parameter set names ignoring case, but not otherwise
                // then we should warn about this
                if (!string.IsNullOrEmpty(defaultParamSetName))
                {
                    // Look for an exact (case-sensitive) match
                    var exactMatch = paramBlockInfo
                        .FirstOrDefault(p =>
                            string.Equals(
                                p.ParameterSetName,
                                defaultParamSetName,
                                StringComparison.Ordinal
                            )
                        );

                    if (exactMatch == null)
                    {
                        // No exact match, look for a case-insensitive match
                        var caseInsensitiveMatch = paramBlockInfo
                            .FirstOrDefault(p =>
                                string.Equals(
                                    p.ParameterSetName,
                                    defaultParamSetName,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            );

                        if (caseInsensitiveMatch != null)
                        {
                            var defaultParameterSetNameExtents = GetDefaultParameterSetNameValueExtent(cmdletBindingAttr);

                            // Emit a diagnostic for the first case-insensitive match
                            yield return new DiagnosticRecord(
                                string.Format(
                                    CultureInfo.CurrentCulture,
                                    Strings.UseConsistentParameterSetNameCaseMismatchDefaultError,
                                    defaultParamSetName,
                                    caseInsensitiveMatch.ParameterSetName),
                                defaultParameterSetNameExtents ?? cmdletBindingAttr?.Extent ?? paramBlock.Extent,
                                GetName(),
                                DiagnosticSeverity.Warning,
                                fileName);
                        }
                    }
                }

                // Check 4: Parameter Set Name Consistency
                // -------------------------------------------------------------
                // If a parameter set name is used in multiple places, it must
                // be consistently used across all usages. This means the casing
                // must match exactly. We should warn about any inconsistencies
                // found.
                var paramSetGroups = paramBlockInfo
                    .GroupBy(p => p.ParameterSetName, StringComparer.OrdinalIgnoreCase)
                    .Where(g =>
                        g.Select(p => p.ParameterSetName)
                         .Distinct(StringComparer.Ordinal)
                         .Skip(1).Any()
                        );

                foreach (var group in paramSetGroups)
                {
                    // Take the first instance as the canonical casing
                    var canonical = group.First();
                    foreach (var entry in group.Skip(1))
                    {
                        if (!string.Equals(
                            entry.ParameterSetName,
                            canonical.ParameterSetName,
                            StringComparison.Ordinal
                            )
                        )
                        {
                            var parameterSetNameExtents = GetParameterSetNameValueExtent(entry.ParameterAttributeAst);

                            if (parameterSetNameExtents != null)
                            {
                                var correction = new CorrectionExtent(
                                    parameterSetNameExtents.StartLineNumber,
                                    parameterSetNameExtents.EndLineNumber,
                                    parameterSetNameExtents.StartColumnNumber,
                                    parameterSetNameExtents.EndColumnNumber,
                                    $"'{canonical.ParameterSetName}'",
                                    fileName,
                                    string.Format(
                                        CultureInfo.CurrentCulture,
                                        Strings.UseConsistentParameterSetNameCaseMismatchSuggestedCorrectionDescription,
                                        entry.ParameterSetName,
                                        canonical.ParameterSetName
                                    )
                                );
                                yield return new DiagnosticRecord(
                                    string.Format(
                                        CultureInfo.CurrentCulture,
                                        Strings.UseConsistentParameterSetNameCaseMismatchParameterError,
                                        entry.ParameterSetName,
                                        canonical.ParameterSetName),
                                    parameterSetNameExtents,
                                    GetName(),
                                    DiagnosticSeverity.Warning,
                                    fileName,
                                    null,
                                    new List<CorrectionExtent> { correction });
                            }
                            else
                            {
                                // If we couldn't find the parameter set name extents, we can't create a correction
                                yield return new DiagnosticRecord(
                                    string.Format(
                                        CultureInfo.CurrentCulture,
                                        Strings.UseConsistentParameterSetNameCaseMismatchParameterError,
                                        entry.ParameterSetName,
                                        canonical.ParameterSetName),
                                    entry.ParameterAttributeAst.Extent,
                                    GetName(),
                                    DiagnosticSeverity.Warning,
                                    fileName);
                            }
                        }
                    }
                }

                // Check 5: Parameter Set Names should not contain New Lines
                // -------------------------------------------------------------
                // There is no practical purpose for parameterset names to
                // contain a newline
                foreach (var entry in paramBlockInfo)
                {
                    if (entry.ParameterSetName.Contains('\n') || entry.ParameterSetName.Contains('\r'))
                    {
                        var parameterSetNameExtents = GetParameterSetNameValueExtent(entry.ParameterAttributeAst);
                        yield return new DiagnosticRecord(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Strings.UseConsistentParameterSetNameNewLineError),
                            parameterSetNameExtents ?? entry.ParameterAttributeAst.Extent,
                            GetName(),
                            DiagnosticSeverity.Warning,
                            fileName);
                    }
                }
                if (defaultParamSetName != null &&
                    (defaultParamSetName.Contains('\n') || defaultParamSetName.Contains('\r')))
                {
                    // If the default parameter set name contains new lines, warn about it
                    var defaultParameterSetNameExtents = GetDefaultParameterSetNameValueExtent(cmdletBindingAttr);
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.UseConsistentParameterSetNameNewLineError,
                            defaultParamSetName),
                        defaultParameterSetNameExtents ?? cmdletBindingAttr?.Extent ?? paramBlock.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName);
                }

            }
        }

        /// <summary>
        /// Retrieves the value of a named argument from an AttributeAst's NamedArguments collection.
        /// If the named argument is not found, returns the provided default value.
        /// If the argument value is a constant, returns its string representation; otherwise, returns the argument's text.
        /// </summary>
        /// <param name="attributeAst">The AttributeAst to search for the named argument.</param>
        /// <param name="argumentName">The name of the argument to look for (case-insensitive).</param>
        /// <param name="defaultValue">The value to return if the named argument is not found. Defaults to null.</param>
        /// <returns>
        /// The value of the named argument as a string if found; otherwise, the default value.
        /// </returns>
        private static string GetNamedArgumentValue(AttributeAst attributeAst, string argumentName, string defaultValue = null)
        {
            if (attributeAst == null || attributeAst.NamedArguments == null)
            {
                return defaultValue;
            }

            foreach (var namedArg in attributeAst.NamedArguments)
            {
                if (namedArg?.ArgumentName == null) continue;

                if (string.Equals(namedArg.ArgumentName, argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    // Try to evaluate the argument value as a constant string
                    if (namedArg.Argument is ConstantExpressionAst constAst)
                    {
                        return constAst.Value?.ToString();
                    }
                    // If not a constant, try to get the string representation
                    return namedArg.Argument.Extent.Text;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Finds the IScriptExtent of the value assigned to the ParameterSetName argument
        /// in the given AttributeAst (if it is a [Parameter()] attribute).
        /// Returns null if not found.
        /// </summary>
        /// <param name="attributeAst">The AttributeAst to search.</param>
        /// <returns>The IScriptExtent of the ParameterSetName value, or null if not found.</returns>
        private static IScriptExtent GetParameterSetNameValueExtent(AttributeAst attributeAst)
        {
            return GetAttributeNamedArgumentValueExtent(attributeAst, "ParameterSetName", "Parameter");
        }

        /// <summary>
        /// Finds the IScriptExtent of the value assigned to the DefaultParameterSetName argument
        /// in the given AttributeAst (if it is a [CmdletBinding()] attribute).
        /// Returns null if not found.
        /// </summary>
        /// <param name="attributeAst">The AttributeAst to search.</param>
        /// <returns>The IScriptExtent of the DefaultParameterSetName value, or null if not found.</returns>
        private static IScriptExtent GetDefaultParameterSetNameValueExtent(AttributeAst attributeAst)
        {
            return GetAttributeNamedArgumentValueExtent(attributeAst, "DefaultParameterSetName", "CmdletBinding");
        }

        /// <summary>
        /// Finds the IScriptExtent of the value of a named argument in the given AttributeAst.
        /// Returns null if not found.
        /// </summary>
        /// <param name="attributeAst">The AttributeAst to search.</param>
        /// <param name="argumentName">The name of the argument to find.</param>
        /// <param name="expectedAttributeName">The expected type name of the attribute. i.e. <c>Parameter</c> (optional).</param>
        /// <returns>The IScriptExtent of the named argument value, or null if not found.</returns>
        private static IScriptExtent GetAttributeNamedArgumentValueExtent(AttributeAst attributeAst, string argumentName, string expectedAttributeName = null)
        {
            if (attributeAst == null || attributeAst.NamedArguments == null)
                return null;

            if (!string.IsNullOrEmpty(expectedAttributeName) &&
                !string.Equals(
                    attributeAst.TypeName?.Name,
                    expectedAttributeName,
                    StringComparison.OrdinalIgnoreCase)
                )
                return null;

            foreach (var namedArg in attributeAst.NamedArguments)
            {
                if (string.Equals(namedArg.ArgumentName, argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return namedArg.Argument?.Extent;
                }
            }
            return null;
        }

        /// <summary>
        /// Represents information about a parameter and its parameter set.
        /// </summary>
        private class ParameterSetInfo
        {
            public string ParameterName { get; }
            public string ParameterSetName { get; }
            public AttributeAst ParameterAttributeAst { get; }

            public ParameterSetInfo(string parameterName, string parameterSetName, AttributeAst parameterAttributeAst)
            {
                ParameterName = parameterName;
                ParameterSetName = parameterSetName;
                ParameterAttributeAst = parameterAttributeAst;
            }
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public override string GetName() => string.Format(
            CultureInfo.CurrentCulture,
            Strings.NameSpaceFormat,
            GetSourceName(),
            Strings.UseConsistentParameterSetNameName
            );

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public override string GetCommonName() => string.Format(
            CultureInfo.CurrentCulture,
            Strings.UseConsistentParameterSetNameCommonName
            );

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public override string GetDescription() => string.Format(
            CultureInfo.CurrentCulture,
            Strings.UseConsistentParameterSetNameDescription
            );

        /// <summary>
        /// Method: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public override SourceType GetSourceType() => SourceType.Builtin;

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: error, warning of information.
        /// </summary>
        /// <returns></returns>
        public override RuleSeverity GetSeverity() => RuleSeverity.Warning;

        /// <summary>
        /// Method: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public override string GetSourceName() => string.Format(
            CultureInfo.CurrentCulture, Strings.SourceName
            );
    }
}
