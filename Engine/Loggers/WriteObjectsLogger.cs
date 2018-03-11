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
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands;
#if !CORECLR
using System.ComponentModel.Composition;
#endif // CORECLR
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Loggers
{
    /// <summary>
    /// WriteObjectsLogger: Logs Diagnostics though WriteObject.
    /// </summary>
#if !CORECLR
    [Export(typeof(ILogger))]
#endif
    public class WriteObjectsLogger : ILogger
    {
        #region Private members

#if CORECLR
        private CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
        private ResourceManager rm = new ResourceManager(
            "Microsoft.Windows.PowerShell.ScriptAnalyzer.Strings",
            typeof(WriteObjectsLogger).GetTypeInfo().Assembly);
#else
        private CultureInfo cul = Thread.CurrentThread.CurrentCulture;
        private ResourceManager rm = new ResourceManager("Microsoft.Windows.PowerShell.ScriptAnalyzer.Strings",
                                                                  Assembly.GetExecutingAssembly());
#endif

        #endregion

        #region Methods

        /// <summary>
        /// LogObject: Logs the given object though WriteObject.
        /// </summary>
        /// <param name="obj">The object to be logged</param>
        /// <param name="command">The Invoke-PSLint command this logger is running through</param>
        public void LogObject(Object obj, InvokeScriptAnalyzerCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("diagnostic");
            }
            command.WriteObject(obj);
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
