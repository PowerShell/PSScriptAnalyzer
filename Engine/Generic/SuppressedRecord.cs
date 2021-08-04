// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents a suppressed diagnostic record
    /// </summary>
    public class SuppressedRecord : DiagnosticRecord
    {
        /// <summary>
        /// The rule suppressions applied to this record.
        /// </summary>
        public IReadOnlyList<RuleSuppression> Suppression { get; set; }

        /// <summary>
        /// Creates a suppressed record based on a diagnostic record and the rule suppression
        /// </summary>
        /// <param name="record"></param>
        /// <param name="Suppression"></param>
        public SuppressedRecord(DiagnosticRecord record, IReadOnlyList<RuleSuppression> suppressions)
        {
            Suppression = new ReadOnlyCollection<RuleSuppression>(new List<RuleSuppression>(suppressions));
            IsSuppressed = true;
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
