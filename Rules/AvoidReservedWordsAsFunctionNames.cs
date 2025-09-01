// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation.Language;
using System.Linq;

#if !CORECLR
using System.ComponentModel.Composition;
#endif

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    /// <summary>
    /// Rule that warns when reserved words are used as function names
    /// </summary>
    public class AvoidReservedWordsAsFunctionNames : IScriptRule
    {

        // The list of PowerShell reserved words.
        // https://learn.microsoft.com/en-gb/powershell/module/microsoft.powershell.core/about/about_reserved_words
        static readonly HashSet<string> reservedWords = new HashSet<string>(
            new[] {
                "assembly", "base", "begin", "break",
                "catch", "class", "command", "configuration",
                "continue", "data", "define", "do",
                "dynamicparam", "else", "elseif", "end",
                "enum", "exit", "filter", "finally",
                "for", "foreach", "from", "function",
                "hidden", "if", "in", "inlinescript",
                "interface", "module", "namespace", "parallel",
                "param", "private", "process", "public",
                "return", "sequence", "static", "switch",
                "throw", "trap", "try", "type",
                "until", "using","var", "while", "workflow"
            },
            StringComparer.OrdinalIgnoreCase
        );

        /// <summary>
        /// Analyzes the PowerShell AST for uses of reserved words as function names.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(Strings.NullAstErrorMessage);
            }

            // Find all FunctionDefinitionAst in the Ast
            var functionDefinitions = ast.FindAll(
                astNode => astNode is FunctionDefinitionAst,
                true
            ).Cast<FunctionDefinitionAst>();

            foreach (var function in functionDefinitions)
            {
                if (reservedWords.Contains(function.Name))
                {
                    yield return new DiagnosticRecord(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.AvoidReservedWordsAsFunctionNamesError,
                            function.Name),
                        Helper.Instance.GetScriptExtentForFunctionName(function) ?? function.Extent,
                        GetName(),
                        DiagnosticSeverity.Warning,
                        fileName
                    );
                }
            }
        }

        public string GetCommonName() => Strings.AvoidReservedWordsAsFunctionNamesCommonName;

        public string GetDescription() => Strings.AvoidReservedWordsAsFunctionNamesDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidReservedWordsAsFunctionNamesName);

        public RuleSeverity GetSeverity() => RuleSeverity.Warning;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}