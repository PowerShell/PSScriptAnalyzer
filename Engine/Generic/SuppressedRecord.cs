// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents a suppressed diagnostic record
    /// </summary>
    public class SuppressedRecord : DiagnosticRecord
    {
        /// <summary>
        /// The rule suppression of this record
        /// </summary>
        public RuleSuppression Suppression
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a suppressed record based on a diagnostic record and the rule suppression
        /// </summary>
        /// <param name="record"></param>
        /// <param name="Suppression"></param>
        public SuppressedRecord(DiagnosticRecord record, RuleSuppression suppression)
        {
            Suppression = suppression;
            if (record != null)
            {
                RuleName = record.RuleName;
                Message = record.Message;
                Extent = record.Extent;
                Severity = record.Severity;
                ScriptPath = record.ScriptPath;
                RuleSuppressionID = record.RuleSuppressionID;
            }
        }
    }
}
