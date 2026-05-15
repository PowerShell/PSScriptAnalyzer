// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif

    /// <summary>
    /// Rule that warns when catch or finally blocks are used without a corresponding try block
    /// </summary>

    public class MissingTryBlock : ConfigurableRule
    {

        /// <summary>
        /// Construct an object of MissingTryBlock type.
        /// </summary>
        public MissingTryBlock() {
            Enable = false;
        }

        /// <summary>
        /// Find bare word "catch" or "finally" tokens that are not part of a TryStatementAst
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            // Find the bare word 'catch' or 'finally' StringConstantExpressionAst nodes used as commands
            var missingTryAsts = ast.FindAll(testAst =>
                // Normally should be part of a TryStatementAst
                testAst is StringConstantExpressionAst stringAst &&
                // Check whether "catch" or "finally" are bare words
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

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.MissingTryBlockCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.MissingTryBlockDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.MissingTryBlockName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}

