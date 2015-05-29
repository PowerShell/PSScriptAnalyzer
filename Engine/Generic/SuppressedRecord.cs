//
// Copyright (c) Microsoft Corporation.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

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
                ScriptName = record.ScriptName;
                RuleSuppressionID = record.RuleSuppressionID;
            }
        }
    }
}
