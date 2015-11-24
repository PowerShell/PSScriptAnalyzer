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

using System.Text.RegularExpressions;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.IO;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands
{
    /// <summary>
    /// InvokeScriptAnalyzerCommand: Cmdlet to statically check PowerShell scripts.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke,
        "ScriptAnalyzer",
        DefaultParameterSetName="File",
        HelpUri = "http://go.microsoft.com/fwlink/?LinkId=525914")]
    public class InvokeScriptAnalyzerCommand : PSCmdlet, IOutputWriter
    {
        #region Parameters
        /// <summary>
        /// Path: The path to the file or folder to invoke PSScriptAnalyzer on.
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = "File",
            Mandatory = true)]
        [ValidateNotNull]
        [Alias("PSPath")]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        private string path;

        /// <summary>
        /// ScriptDefinition: a script definition in the form of a string to run rules on.
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = "ScriptDefinition",
            Mandatory = true)]
        [ValidateNotNull]
        public string ScriptDefinition
        {
            get { return scriptDefinition; }
            set { scriptDefinition = value; }
        }
        private string scriptDefinition;

        /// <summary>
        /// CustomRulePath: The path to the file containing custom rules to run.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [Alias("CustomizedRulePath")]
        public string CustomRulePath
        {
            get { return customRulePath; }
            set { customRulePath = value; }
        }
        private string customRulePath;

        /// <summary>
        /// RecurseCustomRulePath: Find rules within subfolders under the path
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter RecurseCustomRulePath
        {
            get { return recurseCustomRulePath; }
            set { recurseCustomRulePath = value; }
        }
        private bool recurseCustomRulePath;

        /// <summary>
        /// ExcludeRule: Array of names of rules to be disabled.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] ExcludeRule
        {
            get { return excludeRule; }
            set { excludeRule = value; }
        }
        private string[] excludeRule;

        /// <summary>
        /// IncludeRule: Array of names of rules to be enabled.
        /// </summary>
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] IncludeRule
        {
            get { return includeRule; }
            set { includeRule = value; }
        }
        private string[] includeRule;

        /// <summary>
        /// IncludeRule: Array of the severity types to be enabled.
        /// </summary>
        [ValidateSet("Warning", "Error", "Information", IgnoreCase = true)]
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        private string[] severity;

        /// <summary>
        /// Recurse: Apply to all files within subfolders under the path
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter Recurse
        {
            get { return recurse; }
            set { recurse = value; }
        }
        private bool recurse;

        /// <summary>
        /// ShowSuppressed: Show the suppressed message
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter SuppressedOnly
        {
            get { return suppressedOnly; }
            set { suppressedOnly = value; }
        }
        private bool suppressedOnly;

        /// <summary>
        /// Returns path to the file that contains user profile for ScriptAnalyzer
        /// </summary>
        [Alias("Configuration")]
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        public string Profile
        {
            get { return profile; }
            set { profile = value; }
        }
        private string profile;

        #endregion Parameters

        #region Overrides

        /// <summary>
        /// Imports all known rules and loggers.
        /// </summary>
        protected override void BeginProcessing()
        {
            string[] rulePaths = Helper.ProcessCustomRulePaths(customRulePath,
                this.SessionState, recurseCustomRulePath);

            ScriptAnalyzer.Instance.Initialize(
                this,
                rulePaths,
                this.includeRule,
                this.excludeRule,
                this.severity,
                this.suppressedOnly,
                this.profile);
        }

        /// <summary>
        /// Analyzes the given script/directory.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (String.Equals(this.ParameterSetName, "File", StringComparison.OrdinalIgnoreCase))
            {
                // throws Item Not Found Exception                        
                Collection<PathInfo> paths = this.SessionState.Path.GetResolvedPSPathFromPSPath(path);
                foreach (PathInfo p in paths)
                {
                    ProcessPathOrScriptDefinition(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(p.Path));
                }
            }
            else if (String.Equals(this.ParameterSetName, "ScriptDefinition", StringComparison.OrdinalIgnoreCase))
            {
                ProcessPathOrScriptDefinition(scriptDefinition);
            }
        }

        #endregion

        #region Methods

        private void ProcessPathOrScriptDefinition(string pathOrScriptDefinition)
        {
            IEnumerable<DiagnosticRecord> diagnosticsList = Enumerable.Empty<DiagnosticRecord>();

            if (String.Equals(this.ParameterSetName, "File", StringComparison.OrdinalIgnoreCase))
            {
                diagnosticsList = ScriptAnalyzer.Instance.AnalyzePath(pathOrScriptDefinition);
            }
            else if (String.Equals(this.ParameterSetName, "ScriptDefinition", StringComparison.OrdinalIgnoreCase))
            {
                diagnosticsList = ScriptAnalyzer.Instance.AnalyzeScriptDefinition(pathOrScriptDefinition);
            }

            //Output through loggers
            foreach (ILogger logger in ScriptAnalyzer.Instance.Loggers)
            {
                foreach (DiagnosticRecord diagnostic in diagnosticsList)
                {
                    logger.LogObject(diagnostic, this);
                }
            }
        }

        #endregion
    }
}