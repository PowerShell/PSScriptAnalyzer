// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// An interface for an analyzer rule that analyzes the Ast.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// GetName: Retrieves the name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        string GetName();

        /// <summary>
        /// GetName: Retrieves the Common name of the rule.
        /// </summary>
        /// <returns>The name of the rule.</returns>
        string GetCommonName();

        /// <summary>
        /// GetDescription: Retrieves the description of the rule.
        /// </summary>
        /// <returns>The description of the rule.</returns>
        string GetDescription();

        /// <summary>
        /// GetSourceName: Retrieves the source name of the rule.
        /// </summary>
        /// <returns>The source name of the rule.</returns>
        string GetSourceName();

        /// <summary>
        /// GetSourceType: Retrieves the source type of the rule.
        /// </summary>
        /// <returns>The source type of the rule.</returns>
        SourceType GetSourceType();

        /// <summary>
        /// GetSeverity: Retrieves severity of the rule.
        /// </summary>
        /// <returns></returns>
        RuleSeverity GetSeverity();

    }
}
