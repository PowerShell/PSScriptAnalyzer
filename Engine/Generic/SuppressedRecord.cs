// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

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
        public IList<RuleSuppression> Suppressions
        {
            get
            {
                if (suppressions == null) suppressions = new List<RuleSuppression>();

                return suppressions;
            }
            set
            {
                suppressions = value;
            }
        }
        private IList<RuleSuppression> suppressions;

        /// <summary>
        /// Creates a suppressed record based on a diagnostic record and the rule suppression
        /// </summary>
        /// <param name="record"></param>
        /// <param name="Suppression"></param>
        public SuppressedRecord(DiagnosticRecord record, IList<RuleSuppression> suppressions)
        {
            Suppressions = suppressions;
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
