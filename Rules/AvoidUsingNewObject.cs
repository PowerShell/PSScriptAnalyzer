// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// Flags New-Object usage except when creating COM objects via -ComObject parameter.
    /// Supports parameter abbreviation, splatting, variable resolution, and expandable strings.
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidUsingNewObject : IScriptRule
    {
        #region Constants

        private const string CmdletName = "New-Object";
        private const string ComObjectParameterName = "ComObject";
        private const string TypeNameParameterName = "TypeName";

        #endregion

        #region Fields

        /// <summary>
        /// Root AST for variable assignment tracking.
        /// </summary>
        private Ast _rootAst;

        /// <summary>
        /// Lazy-loaded cache mapping variable names to their assignment statements.
        /// Only initialized when splatted variables are encountered.
        /// </summary>
        private Lazy<Dictionary<string, List<AssignmentStatementAst>>> _assignmentCache;

        #endregion

        #region Public Methods

        /// <summary>
        /// Analyzes PowerShell AST for New-Object usage that should be replaced with type literals.
        /// </summary>
        /// <param name="ast">The root AST to analyze</param>
        /// <param name="fileName">Source file name for diagnostic reporting</param>
        /// <returns>Enumerable of diagnostic records for non-COM New-Object usage</returns>
        /// <exception cref="ArgumentNullException">Thrown when ast parameter is null</exception>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            _rootAst = ast;

            // Direct enumeration without intermediate collection
            foreach (var node in ast.FindAll(testAst => testAst is CommandAst cmdAst &&
                string.Equals(cmdAst.GetCommandName(), CmdletName, StringComparison.OrdinalIgnoreCase), true))
            {
                var cmdAst = (CommandAst)node;

                if (!IsComObject(cmdAst))
                {
                    yield return new DiagnosticRecord(
                        string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectError),
                        cmdAst.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName,
                        CmdletName);
                }
            }
        }

        #endregion

        #region Private Instance Methods - Core Logic

        /// <summary>
        /// Determines if a New-Object command creates a COM object.
        /// </summary>
        /// <param name="commandAst">The New-Object command AST to analyze</param>
        /// <returns>True if command creates COM object via -ComObject parameter; false otherwise</returns>
        private bool IsComObject(CommandAst commandAst)
        {
            // Quick check for positional TypeName (non-splat expressions after New-Object)
            if (commandAst.CommandElements.Count >= 2)
            {
                var firstArg = commandAst.CommandElements[1];

                // Non-splat variable provided for first positional parameter.
                if (firstArg is VariableExpressionAst varAst && !varAst.Splatted)
                {
                    return false;
                }

                // Non-splat using expression provided for first positional parameter (e.g., $using:var)
                if (firstArg is UsingExpressionAst usingAst && !(usingAst.SubExpression is VariableExpressionAst usingVar && usingVar.Splatted))
                {
                    return false;
                }
            }

            // Parse named parameters with early TypeName exit
            return HasComObjectParameter(commandAst);
        }

        /// <summary>
        /// Parses command parameters to detect COM object usage with minimal allocations.
        /// Implements early exit optimization for TypeName parameter detection.
        /// </summary>
        /// <param name="commandAst">The command AST to analyze for parameters</param>
        /// <returns>True if ComObject parameter is found; false if TypeName parameter is found or neither</returns>
        private bool HasComObjectParameter(CommandAst commandAst)
        {
            var waitingForParamValue = false;
            var elements = commandAst.CommandElements;
            var elementCount = elements.Count;

            // Fast path: insufficient elements
            if (elementCount <= 1) return false;

            for (int i = 1; i < elementCount; i++)
            {
                var element = elements[i];

                if (waitingForParamValue)
                {
                    waitingForParamValue = false;
                    continue;
                }

                switch (element)
                {
                    case CommandParameterAst paramAst:
                        var paramName = paramAst.ParameterName;

                        // Early exit: TypeName parameter means not COM
                        if (TypeNameParameterName.StartsWith(paramName, StringComparison.OrdinalIgnoreCase))
                            return false;

                        // Found ComObject parameter
                        if (ComObjectParameterName.StartsWith(paramName, StringComparison.OrdinalIgnoreCase))
                            return true;

                        waitingForParamValue = true;
                        break;

                    case ExpandableStringExpressionAst expandableAst:
                        if (ProcessExpandableString(expandableAst, ref waitingForParamValue, out bool result))
                            return result;
                        break;

                    case VariableExpressionAst varAst when varAst.Splatted:
                        if (IsSplattedVariableComObject(varAst))
                            return true;
                        break;

                    case UsingExpressionAst usingAst when usingAst.SubExpression is VariableExpressionAst usingVar && usingVar.Splatted:
                        if (IsSplattedVariableComObject((VariableExpressionAst)usingAst.SubExpression))
                            return true;
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes expandable strings with minimal allocations.
        /// </summary>
        private bool ProcessExpandableString(
            ExpandableStringExpressionAst expandableAst,
            ref bool waitingForParamValue,
            out bool foundResult)
        {
            foundResult = false;
            var expandedValues = Helper.Instance.GetStringsFromExpressionAst(expandableAst);

            if (expandedValues is IList<string> list && list.Count == 0 && expandableAst.NestedExpressions != null)
            {
                // Defer cache initialization until actually needed
                var resolvedText = TryResolveExpandableString(expandableAst);
                if (resolvedText != null)
                {
                    expandedValues = new[] { resolvedText };
                }
            }

            foreach (var expandedValue in expandedValues)
            {
                if (expandedValue.Length > 1 && expandedValue[0] == '-')
                {
                    // Avoid substring allocation by using span slicing
                    var paramName = GetParameterNameFromExpandedValue(expandedValue);

                    if (TypeNameParameterName.StartsWith(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundResult = true;
                        return true; // TypeName found, not COM
                    }

                    if (ComObjectParameterName.StartsWith(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundResult = true;
                        return true; // ComObject found
                    }

                    waitingForParamValue = true;
                }
            }

            return false;
        }

        #endregion

        #region Private Instance Methods - Variable Resolution

        /// <summary>
        /// Lazy resolution of expandable strings - only initialize cache when needed.
        /// </summary>
        private string TryResolveExpandableString(ExpandableStringExpressionAst expandableAst)
        {
            if (expandableAst.NestedExpressions?.Count != 1)
                return null;

            var nestedExpr = expandableAst.NestedExpressions[0];

            if (!(nestedExpr is VariableExpressionAst varAst))
                return null;

            // Now we actually need the cache
            EnsureAssignmentCacheInitialized();

            var varName = GetVariableNameWithoutScope(varAst);
            var varValues = ResolveVariableValues(varName);

            if (varValues is IList<string> list && list.Count > 0)
            {
                var resolvedText = expandableAst.Extent.Text;
                var userPath = varAst.VariablePath.UserPath;

                // Optimize string replacement
                var varPattern = string.Concat("${", userPath, "}");
                var index = resolvedText.IndexOf(varPattern, StringComparison.Ordinal);

                if (index >= 0)
                {
                    return string.Concat(
                        resolvedText.Substring(0, index),
                        list[0],
                        resolvedText.Substring(index + varPattern.Length)
                    );
                }

                // Try without braces
                varPattern = "$" + userPath;
                index = resolvedText.IndexOf(varPattern, StringComparison.Ordinal);

                if (index >= 0)
                {
                    return string.Concat(
                        resolvedText.Substring(0, index),
                        list[0],
                        resolvedText.Substring(index + varPattern.Length)
                    );
                }
            }

            return null;
        }

        /// <summary>
        /// Analyzes splatted variable for COM object parameters by examining hashtable assignments.
        /// Supports string literals, expandable strings, and variable key resolution.
        /// </summary>
        /// <param name="splattedVar">Splatted variable expression to analyze</param>
        /// <returns>True if hashtable contains ComObject key or abbreviation</returns>
        private bool IsSplattedVariableComObject(VariableExpressionAst splattedVar)
        {
            EnsureAssignmentCacheInitialized();

            var variableName = GetVariableNameWithoutScope(splattedVar);

            return IsSplattedVariableComObjectRecursive(variableName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private bool IsSplattedVariableComObjectRecursive(string variableName, HashSet<string> visited)
        {
            if (visited.Contains(variableName))
            {
                return false;
            }

            visited.Add(variableName);

            var assignments = FindHashtableAssignments(variableName);

            foreach (var assignment in assignments)
            {
                if (assignment.Right is CommandExpressionAst cmdExpr)
                {
                    if (cmdExpr.Expression is HashtableAst hashtableAst)
                    {
                        var hasComKey = HasComObjectKey(hashtableAst);

                        if (hasComKey)
                        {
                            return true;
                        }
                    }
                    else if (cmdExpr.Expression is VariableExpressionAst referencedVar)
                    {
                        // Follow variable chain: $params2 = $script:params
                        var referencedName = GetVariableNameWithoutScope(referencedVar);

                        if (IsSplattedVariableComObjectRecursive(referencedName, visited))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Instance Methods - Cache Management

        /// <summary>
        /// Ensures the assignment cache is initialized before use.
        /// </summary>
        private void EnsureAssignmentCacheInitialized()
        {
            if (_assignmentCache == null)
            {
                _assignmentCache = new Lazy<Dictionary<string, List<AssignmentStatementAst>>>(BuildAssignmentCache);
            }
        }

        /// <summary>
        /// Builds case-insensitive cache mapping variable names to their assignment statements.
        /// Single AST traversal shared across all variable lookups.
        /// </summary>
        /// <returns>Dictionary of variable assignments indexed by variable name</returns>
        private Dictionary<string, List<AssignmentStatementAst>> BuildAssignmentCache()
        {
            var cache = new Dictionary<string, List<AssignmentStatementAst>>(StringComparer.OrdinalIgnoreCase);

            foreach (var ast in _rootAst.FindAll(ast => ast is AssignmentStatementAst, true))
            {
                var assignment = (AssignmentStatementAst)ast;
                VariableExpressionAst leftVar = null;

                switch (assignment.Left)
                {
                    case VariableExpressionAst varAst:
                        leftVar = varAst;
                        break;
                    case ConvertExpressionAst convertExpr when convertExpr.Child is VariableExpressionAst convertedVar:
                        leftVar = convertedVar;
                        break;
                }

                if (leftVar != null)
                {
                    var variableName = GetVariableNameWithoutScope(leftVar);

                    if (!cache.TryGetValue(variableName, out var list))
                    {
                        list = new List<AssignmentStatementAst>(4);
                        cache[variableName] = list;
                    }

                    list.Add(assignment);
                }
            }

            return cache;
        }

        /// <summary>
        /// Retrieves cached assignment statements for the specified variable.
        /// </summary>
        /// <param name="variableName">Variable name to look up</param>
        /// <returns>List of assignment statements or empty list if none found</returns>
        private List<AssignmentStatementAst> FindHashtableAssignments(string variableName)
        {
            var cache = _assignmentCache.Value;
            return cache.TryGetValue(variableName, out var assignments)
                ? assignments
                : new List<AssignmentStatementAst>();
        }

        #endregion

        #region Private Instance Methods - Hashtable Analysis

        /// <summary>
        /// Checks hashtable for ComObject keys, supporting parameter abbreviation.
        /// Handles string literals, expandable strings, and variable keys.
        /// </summary>
        /// <param name="hashtableAst">Hashtable AST to examine</param>
        /// <returns>True if any key matches ComObject parameter name or abbreviation</returns>
        private bool HasComObjectKey(HashtableAst hashtableAst)
        {
            foreach (var keyValuePair in hashtableAst.KeyValuePairs)
            {
                var keyStrings = GetKeyStrings(keyValuePair.Item1);

                foreach (var keyString in keyStrings)
                {
                    // Require minimum 3 characters for COM parameter abbreviation to avoid false positives
                    if (keyString.Length >= 3)
                    {
                        if (ComObjectParameterName.StartsWith(keyString, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Extracts string values from hashtable key expressions.
        /// Supports literals, expandable strings, and variable resolution.
        /// </summary>
        /// <param name="keyExpression">Key expression from hashtable</param>
        /// <returns>List of resolved string values for the key</returns>
        private List<string> GetKeyStrings(ExpressionAst keyExpression)
        {
            switch (keyExpression)
            {
                case StringConstantExpressionAst stringAst:
                    return new List<string>(1) { stringAst.Value };

                case ExpandableStringExpressionAst expandableAst:
                    var expandedStrings = Helper.Instance.GetStringsFromExpressionAst(expandableAst);

                    if (expandedStrings is IList<string> list && list.Count > 0)
                        return new List<string>(list);

                    // Fallback for single variable case
                    if (expandableAst.NestedExpressions?.Count == 1 &&
                        expandableAst.NestedExpressions[0] is VariableExpressionAst varAst)
                    {
                        return ResolveVariableValues(varAst.VariablePath.UserPath);
                    }
                    break;

                case VariableExpressionAst variableAst:
                    return ResolveVariableValues(variableAst.VariablePath.UserPath);
            }

            return new List<string>(0);
        }

        /// <summary>
        /// Resolves variable values by analyzing string assignments using cached lookups.
        /// </summary>
        /// <param name="variableName">Variable name to resolve</param>
        /// <returns>List of possible string values assigned to the variable</returns>
        private List<string> ResolveVariableValues(string variableName)
        {
            var values = new List<string>();

            // Optimize scope removal
            var colonIndex = variableName.IndexOf(':');
            var normalizedName = colonIndex >= 0
                ? TrimQuotes(variableName.Substring(colonIndex + 1))
                : TrimQuotes(variableName);

            if (_assignmentCache.Value.TryGetValue(normalizedName, out var variableAssignments))
            {
                foreach (var assignment in variableAssignments)
                {
                    if (assignment.Right is CommandExpressionAst cmdExpr)
                    {
                        var extractedValues = Helper.Instance.GetStringsFromExpressionAst(cmdExpr.Expression);

                        // Avoid AddRange if possible
                        if (extractedValues is IList<string> list)
                        {
                            for (int i = 0; i < list.Count; i++)
                                values.Add(list[i]);
                        }
                        else
                        {
                            values.AddRange(extractedValues);
                        }
                    }
                }
            }

            return values;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Extracts parameter name without allocating substrings.
        /// </summary>
        private static string GetParameterNameFromExpandedValue(string expandedValue)
        {
            // Skip the '-' prefix
            var startIndex = 1;
            var length = expandedValue.Length - 1;

            // Check for quotes and adjust
            if (length >= 2)
            {
                var firstChar = expandedValue[startIndex];
                var lastChar = expandedValue[expandedValue.Length - 1];

                if ((firstChar == '"' || firstChar == '\'') && firstChar == lastChar)
                {
                    startIndex++;
                    length -= 2;
                }
            }

            // Only allocate if we need to trim
            return startIndex == 1 && length == expandedValue.Length - 1
                ? expandedValue.Substring(1)
                : expandedValue.Substring(startIndex, length);
        }

        /// <summary>
        /// Extracts the variable name without scope from a VariableExpressionAst.
        /// Additionally, trims quotes from the variable name (required for expandable strings).
        /// </summary>
        /// <param name="variableAst">The VariableExpressionAst to extract the variable name from</param>
        /// <returns>The variable name without scope</returns>
        private static string GetVariableNameWithoutScope(VariableExpressionAst variableAst)
        {
            var variableName = Helper.Instance.VariableNameWithoutScope(variableAst.VariablePath);
            return TrimQuotes(variableName);
        }

        /// <summary>
        /// ExpandableStringExpressionAst's with quotes will not resolve to a non-quoted string.
        /// It's necessary to trim the quotes from the input string in order to successfully lookup
        /// the variable value.
        /// </summary>
        /// <param name="input">The input string to trim</param>
        /// <returns>The trimmed string, or the original string if it doesn't contain quotes</returns>
        private static string TrimQuotes(string input)
        {
            if (input.Length < 2)
                return input;

            char first = input[0];
            char last = input[input.Length - 1];

            if (first != last)
                return input;

            if (first == '"' || first == '\'')
                return input.Substring(1, input.Length - 2);

            return input;
        }

        #endregion

        #region IScriptRule Implementation

        /// <summary>
        /// Gets the fully qualified name of this rule.
        /// </summary>
        /// <returns>Rule name in format "SourceName\RuleName"</returns>
        public string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidUsingNewObjectName
            );
        }

        /// <summary>
        /// Gets the user-friendly common name of this rule.
        /// </summary>
        /// <returns>Localized common name</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectCommonName);
        }

        /// <summary>
        /// Gets the detailed description of what this rule checks.
        /// </summary>
        /// <returns>Localized rule description</returns>
        public string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidUsingNewObjectDescription);
        }

        /// <summary>
        /// Gets the severity level of violations detected by this rule.
        /// </summary>
        /// <returns>Warning severity level</returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the source name for this rule.
        /// </summary>
        /// <returns>PSScriptAnalyzer source name</returns>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Gets the source type indicating this is a built-in rule.
        /// </summary>
        /// <returns>Builtin source type</returns>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        #endregion
    }
}