// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents the severity of a PSScriptAnalyzer rule
    /// </summary>
    public enum RuleSeverity : uint
    {
        /// <summary>
        /// Information: This warning is trivial, but may be useful. They are recommended by PowerShell best practice.
        /// </summary>
        Information = 0,

        /// <summary>
        /// WARNING: This warning may cause a problem or does not follow PowerShell's recommended guidelines.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// ERROR: This warning is likely to cause a problem or does not follow PowerShell's required guidelines.
        /// </summary>
        Error = 2,

        /// <summary>
        /// ERROR: This diagnostic is caused by an actual parsing error, and is generated only by the engine.
        /// </summary>
        ParseError    = 3,
    };
}
