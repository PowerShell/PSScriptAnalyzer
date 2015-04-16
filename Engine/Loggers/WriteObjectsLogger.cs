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
using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using Microsoft.Windows.Powershell.ScriptAnalyzer.Commands;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Loggers
{
    /// <summary>
    /// WriteObjectsLogger: Logs Diagnostics though WriteObject.
    /// </summary>
    [Export(typeof(ILogger))]
    public class WriteObjectsLogger : ILogger
    {
        #region Private members

        private CultureInfo cul = Thread.CurrentThread.CurrentCulture;
        private ResourceManager rm = new ResourceManager("Microsoft.Windows.Powershell.ScriptAnalyzer.Strings",
                                                                  Assembly.GetExecutingAssembly());

        #endregion

        #region Methods

        /// <summary>
        /// LogMessage: Logs the given diagnostic though WriteObject.
        /// </summary>
        /// <param name="diagnostic">The diagnostic to be logged</param>
        /// <param name="command">The Invoke-PSLint command this logger is running through</param>
        public void LogMessage(DiagnosticRecord diagnostic, InvokeScriptAnalyzerCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (diagnostic == null)
            {
                throw new ArgumentNullException("diagnostic");
            }
            command.WriteObject(diagnostic);
        }

        /// <summary>
        /// GetName: Retrieves the name of this logger.
        /// </summary>
        /// <returns>The name of this logger</returns>
        public string GetName()
        {
            return rm.GetString("DefaultLoggerName", cul);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this logger.
        /// </summary>
        /// <returns>The description of this logger</returns>
        public string GetDescription()
        {
            return rm.GetString("DefaultLoggerDescription", cul);
        }

        #endregion
    }
}
