using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
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
                ScriptName = record.ScriptName;
                RuleSuppressionID = record.RuleSuppressionID;
            }
        }
    }
}
