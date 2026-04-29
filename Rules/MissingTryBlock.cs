// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Rule that warns when catch or finally blocks are used without a corresponding try block
    /// </summary>
    public class MissingTryBlock : IScriptRule
    {
        /// <summary>
        /// Analyzes the PowerShell AST for catch - or finally blocks that misses the try block.
        /// </summary>
        /// <param name="ast">The PowerShell Abstract Syntax Tree to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed (for diagnostic reporting).</param>
        /// <returns>A collection of diagnostic records for each violation.</returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find all FunctionDefinitionAst in the Ast
            var missingTryAsts = ast.FindAll(testAst =>
                // Normally should be part of a TryStatementAst
                testAst is StringConstantExpressionAst stringAst &&
                // Catch of finally  are reserved keywords and should be bare words
                stringAst.StringConstantType == StringConstantType.BareWord &&
                (
                    String.Equals(stringAst.Value, "catch", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(stringAst.Value, "finally", StringComparison.OrdinalIgnoreCase)
                ) &&
                stringAst.Parent is CommandAst commandAst &&
                // Only violate if the catch or finally is the first command element
                commandAst.CommandElements[0] == stringAst,
                true
            );

            foreach (StringConstantExpressionAst missingTryAst in missingTryAsts)
            {
                yield return new DiagnosticRecord(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.MissingTryBlockError,
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(missingTryAst.Value)),
                    missingTryAst.Extent,
                    GetName(),
                    DiagnosticSeverity.Warning,
                    fileName,
                    missingTryAst.Value
                );
            }
        }

        public string GetCommonName() => Strings.MissingTryBlockCommonName;

        public string GetDescription() => Strings.MissingTryBlockDescription;

        public string GetName() => string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.MissingTryBlockName);

        public RuleSeverity GetSeverity() => RuleSeverity.Warning;

        public string GetSourceName() => Strings.SourceName;

        public SourceType GetSourceType() => SourceType.Builtin;
    }
}