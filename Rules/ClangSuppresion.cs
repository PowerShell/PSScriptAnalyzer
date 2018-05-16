// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// The idea behind clang suppresion style is to wrap a statement in extra parenthesis to implicitly suppress warnings of its content to signal that the offending operation was deliberate.
    /// </summary>
    internal static class ClangSuppresion
    {
        /// <summary>
        /// The community requested an implicit suppression mechanism that follows the clang style where warnings are not issued if the expression is wrapped in extra parenthesis.
        /// See here for details: https://github.com/Microsoft/clang/blob/349091162fcf2211a2e55cf81db934978e1c4f0c/test/SemaCXX/warn-assignment-condition.cpp#L15-L18
        /// </summary>
        /// <param name="scriptExtent"></param>
        /// <returns></returns>
        internal static bool ScriptExtendIsWrappedInParenthesis(IScriptExtent scriptExtent)
        {
            return scriptExtent.Text.StartsWith("(") && scriptExtent.Text.EndsWith(")");
        }
    }
}
