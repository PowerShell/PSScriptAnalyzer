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

using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Provides an interface for writing output to a PowerShell session.
    /// </summary>
    public interface IOutputWriter
    {
        /// <summary>
        /// Writes an error to the session.
        /// </summary>
        /// <param name="error">The ErrorRecord to write.</param>
        void WriteError(ErrorRecord error);

        /// <summary>
        /// Writes a warning to the session.
        /// </summary>
        /// <param name="message">The warning string to write.</param>
        void WriteWarning(string message);

        /// <summary>
        /// Writes a verbose message to the session.
        /// </summary>
        /// <param name="message">The verbose message to write.</param>
        void WriteVerbose(string message);

        /// <summary>
        /// Writes a debug message to the session.
        /// </summary>
        /// <param name="message">The debug message to write.</param>
        void WriteDebug(string message);

        /// <summary>
        /// Throws a terminating error in the session.
        /// </summary>
        /// <param name="record">The ErrorRecord which describes the failure.</param>
        void ThrowTerminatingError(ErrorRecord record);
    }
}
