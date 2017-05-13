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

using Microsoft.Windows.Powershell.ScriptAnalyzer.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Resources;
using System.Threading;
using System.Reflection;

namespace Microsoft.Windows.Powershell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// GetScriptAnalyzerLoggerCommand: Cmdlet that lists the PSScriptAnalyzer logger names and descriptions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ScriptAnalyzerLogger", HelpUri = "http://go.microsoft.com/fwlink/?LinkId=525912")]
    public class GetScriptAnalyzerLoggerCommand : PSCmdlet
    {
        #region Parameters

        /// <summary>
        /// Path: Path to custom logger folder or assembly files.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Path
        {
            get { return path; }
            set { path = value; }
        }
        private string[] path;        

        /// <summary>
        /// Name: The name of a specific logger to list.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Name
        {
            get { return name; }
            set { name = value; }
        }
        private string[] name;

        #endregion Parameters

        #region Private Members

        Dictionary<string, List<string>> validationResults = new Dictionary<string, List<string>>();
        private const string baseName                      = "Microsoft.Windows.Powershell.ScriptAnalyzer.Strings";
        private CultureInfo cul                            = Thread.CurrentThread.CurrentCulture;
        private ResourceManager rm                         = new ResourceManager(baseName, Assembly.GetExecutingAssembly());

        #endregion

        #region Overrides

        /// <summary>
        /// BeginProcessing : TBD
        /// </summary>
        protected override void BeginProcessing()
        {
            #region Set PSCmdlet property of Helper

            Helper.Instance.MyCmdlet = this;

            #endregion

            // Verifies paths
            if (path != null)
            {
                validationResults = ScriptAnalyzer.Instance.CheckPath(path, this);
                foreach (string invalidPath in validationResults["InvalidPaths"])
                {
                    WriteWarning(string.Format(cul, rm.GetString("InvalidPath", cul), invalidPath));
                }
            }
            else
            {
                validationResults.Add("InvalidPaths", new List<string>());
                validationResults.Add("ValidPaths",   new List<string>());             
            }

            try
            {
                if (validationResults["ValidPaths"].Count == 0)
                {
                    ScriptAnalyzer.Instance.Initialize();
                }
                else
                {
                    ScriptAnalyzer.Instance.Initilaize(validationResults);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, ex.HResult.ToString("X", cul),
                    ErrorCategory.NotSpecified, this));
            }
        }

        /// <summary>
        /// ProcessRecord : TBD
        /// </summary>
        protected override void ProcessRecord()
        {
            IEnumerable<ILogger> loggers = ScriptAnalyzer.Instance.Loggers;
            if (loggers != null)
            {
                if (name != null)
                {
                    foreach (ILogger logger in loggers)
                    {
                        if (name.Contains(logger.GetName(), StringComparer.OrdinalIgnoreCase))
                        {
                            WriteObject(new LoggerInfo(logger.GetName(), logger.GetDescription()));
                        }
                    }
                }
                else
                {
                    foreach (ILogger logger in loggers)
                    {
                        WriteObject(new LoggerInfo(logger.GetName(), logger.GetDescription()));
                    }                
                }
            }
            else
            {
                WriteObject(rm.GetString("LoggersNotFound", cul));
            }
        }

        #endregion
    }
}
