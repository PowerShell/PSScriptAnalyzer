// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for an analyzer rule that analyzes the tokens of a script.
    /// </summary>
    public interface ITokenRule : IRule
    {
        /// <summary>
        /// AnalyzeTokens: Analyzes the tokens of the given script.
        /// </summary>
        /// <param name="tokens">The tokens to be analyzed</param>
        /// <param name="fileName">The name of the script file being analyzed</param>
        /// <returns>The results of the analysis</returns>
        IEnumerable<DiagnosticRecord> AnalyzeTokens(Token[] tokens, string fileName);
    }
}
