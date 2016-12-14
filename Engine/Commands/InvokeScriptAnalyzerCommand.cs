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
using System.Management.Automation.Runspaces;
using System.Collections;

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
        #region Private variables
        List<string> processedPaths;
        #endregion // Private variables

        #region Parameters
        /// <summary>
        /// Path: The path to the file or folder to invoke PSScriptAnalyzer on.
        /// </summary>
        [Parameter(Position = 0,
            ParameterSetName = "File",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
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
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
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
        public string[] CustomRulePath
        {
            get { return customRulePath; }
            set { customRulePath = value; }
        }
        private string[] customRulePath;

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
        /// IncludeDefaultRules: Invoke default rules along with Custom rules
        /// </summary>
        [Parameter(Mandatory = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public SwitchParameter IncludeDefaultRules
        {
            get { return includeDefaultRules; }
            set { includeDefaultRules = value; }
        }
        private bool includeDefaultRules;

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
        /// Returns path to the file that contains user profile or hash table for ScriptAnalyzer
        /// </summary>
        [Alias("Profile")]
        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        public object Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        private object settings;

        private bool stopProcessing;

#if !PSV3
        /// <summary>
        /// Resolve DSC resource dependency
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter SaveDscDependency
        {
            get { return saveDscDependency; }
            set { saveDscDependency = value; }
        }
        private bool saveDscDependency;
#endif // !PSV3
#endregion Parameters

#region Overrides

        /// <summary>
        /// Imports all known rules and loggers.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize helper
            Helper.Instance = new Helper(
                SessionState.InvokeCommand,
                this);
            Helper.Instance.Initialize();

            var psVersionTable = this.SessionState.PSVariable.GetValue("PSVersionTable") as Hashtable;
            if (psVersionTable != null)
            {
                Helper.Instance.SetPSVersionTable(psVersionTable);
            }

            string[] rulePaths = Helper.ProcessCustomRulePaths(customRulePath,
                this.SessionState, recurseCustomRulePath);
            if (IsFileParameterSet())
            {
                ProcessPath();
            }

            var settingFileHasErrors = false;
            if (settings == null
                && processedPaths != null
                && processedPaths.Count == 1)
            {
                // add a directory separator character because if there is no trailing separator character, it will return the parent
                var directory = processedPaths[0].TrimEnd(System.IO.Path.DirectorySeparatorChar);
                if (File.Exists(directory))
                {
                    // if given path is a file, get its directory
                    directory = System.IO.Path.GetDirectoryName(directory);
                }

                this.WriteVerbose(
                    String.Format(
                        "Settings not provided. Will look for settings file in the given path {0}.",
                        path));
                var settingsFileAutoDiscovered = false;
                if (Directory.Exists(directory))
                {
                    // if settings are not provided explicitly, look for it in the given path
                    // check if pssasettings.psd1 exists
                    var settingsFilename = "PSScriptAnalyzerSettings.psd1";
                    var settingsFilepath = System.IO.Path.Combine(directory, settingsFilename);
                    if (File.Exists(settingsFilepath))
                    {
                        settingsFileAutoDiscovered = true;
                        this.WriteVerbose(
                            String.Format(
                                "Found {0} in {1}. Will use it to provide settings for this invocation.",
                                settingsFilename,
                                directory));
                        settingFileHasErrors = !ScriptAnalyzer.Instance.ParseProfile(settingsFilepath, this.SessionState.Path, this);
                    }
                }

                if (!settingsFileAutoDiscovered)
                {
                    this.WriteVerbose(
                        String.Format(
                            "Cannot find a settings file in the given path {0}.",
                            path));
                }
            }
            else if (IsBuiltinSettingPreset(settings))
            {
                settingFileHasErrors = !ScriptAnalyzer.Instance.ParseProfile(
                    Helper.GetSettingPresetFilePath(settings as string),
                    this.SessionState.Path,
                    this);
            }
            else
            {
                settingFileHasErrors = !ScriptAnalyzer.Instance.ParseProfile(this.settings, this.SessionState.Path, this);
            }

            if (settingFileHasErrors)
            {
                this.WriteWarning("Cannot parse settings. Will abort the invocation.");
                stopProcessing = true;
                return;
            }

            ScriptAnalyzer.Instance.Initialize(
                this,
                rulePaths,
                this.includeRule,
                this.excludeRule,
                this.severity,
                null == rulePaths ? true : this.includeDefaultRules,
                this.suppressedOnly);
        }

        /// <summary>
        /// Analyzes the given script/directory.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (stopProcessing)
            {
                stopProcessing = false;
                return;
            }

#if !PSV3
            // TODO Support dependency resolution for analyzing script definitions
            if (saveDscDependency)
            {
                using (var rsp = RunspaceFactory.CreateRunspace())
                {
                    rsp.Open();
                    using (var moduleHandler = new ModuleDependencyHandler(rsp))
                    {
                        ScriptAnalyzer.Instance.ModuleHandler = moduleHandler;
                        this.WriteVerbose(
                            String.Format(
                                "Temporary module location: {0}",
                                moduleHandler.TempModulePath));
                        ProcessInput();
                    }
                }
                return;
            }
#endif
            ProcessInput();
        }

        protected override void EndProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.EndProcessing();
        }

        protected override void StopProcessing()
        {
            ScriptAnalyzer.Instance.CleanUp();
            base.StopProcessing();
        }

#endregion

#region Private Methods

        private static bool IsBuiltinSettingPreset(object settingPreset)
        {
            var preset = settingPreset as string;
            if (preset != null)
            {
                return Helper.GetBuiltinSettingPresets().Contains(preset, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }

        private void ProcessInput()
        {
            IEnumerable<DiagnosticRecord> diagnosticsList = Enumerable.Empty<DiagnosticRecord>();
            if (IsFileParameterSet())
            {
                foreach (var p in processedPaths)
                {
                    diagnosticsList = ScriptAnalyzer.Instance.AnalyzePath(p, this.recurse);
                    WriteToOutput(diagnosticsList);
                }
            }
            else if (String.Equals(this.ParameterSetName, "ScriptDefinition", StringComparison.OrdinalIgnoreCase))
            {
                diagnosticsList = ScriptAnalyzer.Instance.AnalyzeScriptDefinition(scriptDefinition);
                WriteToOutput(diagnosticsList);
            }
        }

        private void WriteToOutput(IEnumerable<DiagnosticRecord> diagnosticRecords)
        {
            foreach (ILogger logger in ScriptAnalyzer.Instance.Loggers)
            {
                foreach (DiagnosticRecord diagnostic in diagnosticRecords)
                {
                    logger.LogObject(diagnostic, this);
                }
            }
        }

        private void ProcessPath()
        {
            Collection<PathInfo> paths = this.SessionState.Path.GetResolvedPSPathFromPSPath(path);
            processedPaths = new List<string>();
            foreach (PathInfo p in paths)
            {
                processedPaths.Add(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(p.Path));
            }
        }

        private bool IsFileParameterSet()
        {
            return String.Equals(this.ParameterSetName, "File", StringComparison.OrdinalIgnoreCase);
        }
#endregion // Private Methods
    }
}