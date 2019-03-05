// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents an interface for an external analyzer rule.
    /// </summary>
    internal interface IExternalRule : IRule
    {
        /// <summary>
        /// Adds module name (source name) to handle ducplicate function names in different modules.
        /// </summary>
        /// <returns></returns>
        string GetFullName();

        /// <summary>
        /// GetParameter: Retrieves AstType parameter
        /// </summary>
        /// <returns>string</returns>
        string GetParameter();

        /// <summary>
        /// GetFullModulePath: Retrieves full module path.
        /// </summary>
        /// <returns>string</returns>
        string GetFullModulePath();
    }
}
