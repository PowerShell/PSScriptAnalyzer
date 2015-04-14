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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Commands;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// ILogger: An interface for a PSScriptAnalyzer logger to output the results of PSScriptAnalyzer rules.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// LogMessage: Logs the given diagnostic, using the command for Write methods if needed.
        /// </summary>
        /// <param name="diagnostic">The DiagnosticRecord to be logged.</param>
        /// <param name="command">The InvokePSScriptAnalyzerCommand that this logger is running off of.</param>
        void LogMessage(DiagnosticRecord diagnostic, InvokeScriptAnalyzerCommand command);

        /// <summary>
        /// GetName: Retrieves the name of the logger.
        /// </summary>
        /// <returns>The name of the logger</returns>
        string GetName();

        /// <summary>
        /// GetDescription: Retrives the description of the logger.
        /// </summary>
        /// <returns>The description of the logger</returns>
        string GetDescription();
    }
}
