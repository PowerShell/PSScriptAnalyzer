using System;
using SMA = System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Management.Automation.Language;
using System.Threading;
using System.IO;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting
{
    /// <summary>
    /// This is a helper class which provides a consistent, easy to use set of interfaces for Script Analyzer
    /// This class will enable you to:
    ///  - Analyze a script or file with any configuration which is supported by Invoke-ScriptAnalyzer
    ///  - Reformat a script as is done via Invoke-Formatter
    /// </summary>
    public class HostedAnalyzer : IDisposable
    {
        private SMA.PowerShell ps;
        internal hostedWriter writer;
        private ScriptAnalyzer analyzer;
        private bool disposed = false;

        object hostedAnalyzerLock = new object();
        bool lockWasTaken = false;

        /// <summary>
        /// Create an instance of the hosted analyzer
        /// </summary>
        public HostedAnalyzer()
        {
            InitialSessionState iss = InitialSessionState.CreateDefault2();
            ps = SMA.PowerShell.Create(iss);
            writer = new hostedWriter();
            ScriptAnalyzer.Instance.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            analyzer = ScriptAnalyzer.Instance;
        }

        /// <summary>Reset the the analyzer and associated state</summary>
        public void Reset()
        {

            lock (hostedAnalyzerLock) {
                analyzer.CleanUp();
                Helper.Instance = new Helper(ps.Runspace.SessionStateProxy.InvokeCommand, writer);
                Helper.Instance.Initialize();
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            }
        }

        /// <summary>
        /// Analyze a script in the form of an AST
        /// <param name="scriptast">A scriptblockast which represents the script to analyze</param>
        /// <param name="tokens">The tokens in the ast</param>
        /// <param name="filename">The name of the file which held the script, if there was one</param>
        /// </summary>
        public AnalyzerResult Analyze(ScriptBlockAst scriptast, Token[] tokens, string filename = null)
        {
            try
            {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                writer.ClearWriter();
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
                var result = analyzer.AnalyzeSyntaxTree(scriptast, tokens, filename);
                return new AnalyzerResult(AnalysisType.Ast, result, this);
            }
            finally
            {
                analyzer.CleanUp();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>
        /// Analyze a script in the form of a string with additional Settings
        /// <param name="ScriptDefinition">The script as a string</param>
        /// <param name="settings">A hastable which includes the settings</param>
        /// </summary>
        public AnalyzerResult Analyze(string ScriptDefinition, Settings settings)
        {
            try {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                writer.ClearWriter();
                analyzer.UpdateSettings(settings);
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null, false);
                var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
                return new AnalyzerResult(AnalysisType.Script, result, this);
            }
            finally {
                analyzer.CleanUp();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>
        /// Analyze a script in the form of a string with additional Settings
        /// <param name="ScriptDefinition">The script as a string</param>
        /// <param name="settings">A hastable which includes the settings</param>
        /// </summary>
        public AnalyzerResult Analyze(string ScriptDefinition, Hashtable settings)
        {
            try
            {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                writer.ClearWriter();
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
                analyzer.UpdateSettings(CreateSettings(settings));
                var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
                return new AnalyzerResult(AnalysisType.Script, result, this);
            }
            finally
            {
                analyzer.CleanUp();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>Analyze a script in the form of a string, based on default</summary>
        public AnalyzerResult Analyze(string ScriptDefinition)
        {
            try
            {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                writer.ClearWriter();
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
                var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
                return new AnalyzerResult(AnalysisType.Script, result, this);
            }
            finally
            {
                analyzer.CleanUp();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>Analyze a script in the form of a file</summary>
        public AnalyzerResult Analyze(FileInfo File)
        {
            try {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                writer.ClearWriter();
                analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
                var result = analyzer.AnalyzePath(File.FullName, (x, y) => { return true; }, false);
                return new AnalyzerResult(AnalysisType.File, result, this);
            }
            finally
            {
                analyzer.CleanUp();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>Fix a script</summary>
        public string Fix(string scriptDefinition)
        {
            try
            {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                bool fixesApplied;
                return analyzer.Fix(scriptDefinition, out fixesApplied);
            }
            finally
            {
                analyzer.CleanUp();
                Reset();
                Monitor.Exit(hostedAnalyzerLock);
            }
        }

        /// <summary>
        /// Create a standard settings object for Script Analyzer
        /// This is the object used by analyzer internally
        /// It is more functional than the AnalyzerSettings object because
        /// it contains the Rule Arguments which are not passable to the Initialize method
        /// </summary>
        public Settings CreateSettings(string SettingsName)
        {
            return Settings.Create(SettingsName, "", writer, ps.Runspace.SessionStateProxy.Path.GetResolvedProviderPathFromPSPath);
        }

        /// <summary>
        /// Create a standard settings object for Script Analyzer from an existing .psd1 file
        /// </summary>
        public Settings CreateSettingsFromFile(string settingsFile)
        {
            return new Settings(settingsFile);
        }

        /// <summary>
        /// Create a default settings object
        /// </summary>
        public Settings CreateSettings()
        {
            Settings s = Settings.Create(new Hashtable(), "", writer, ps.Runspace.SessionStateProxy.Path.GetResolvedProviderPathFromPSPath);
            s.IncludeDefaultRules = true;
            return s;
        }

        /// <summary>
        /// Create a standard settings object for Script Analyzer
        /// This is the object used by analyzer internally
        /// It is more functional than the AnalyzerSettings object because
        /// it contains the Rule Arguments which are not passable to the Initialize method
        /// </summary>
        public Settings CreateSettings(Hashtable settings)
        {
            return Settings.Create(settings, "", writer, ps.Runspace.SessionStateProxy.Path.GetResolvedProviderPathFromPSPath);
        }

        /// <summary>
        /// Format a script according to the formatting rules
        ///     PSPlaceCloseBrace
        ///     PSPlaceOpenBrace
        ///     PSUseConsistentWhitespace
        ///     PSUseConsistentIndentation
        ///     PSAlignAssignmentStatement
        ///     PSUseCorrectCasing
        /// and the union of the actual settings which are passed to it.
        /// </summary>
        public string Format(string scriptDefinition, Settings settings)
        {
            if ( settings == null ) {
                throw new ArgumentException("settings may not be null");
            }
            string s;
            try {
                Monitor.Enter(hostedAnalyzerLock, ref lockWasTaken);
                s = Formatter.Format(scriptDefinition, settings, null, ps.Runspace, writer);
            }
            finally {
                analyzer.CleanUp();
                // Reset is required because formatting leaves a number of settings behind which
                // should be cleared.
                Reset();
                Monitor.Exit(hostedAnalyzerLock);
            }
            return s;
        }

        /// <summary>
        /// Analyzer usually requires a cmdlet to manage the output to the user.
        /// This class is provided to collect the non-diagnostic record output
        /// when invoking the methods in the HostedAnalyzer.
        /// </summary>
        internal class hostedWriter : IOutputWriter
        {
            /// <summary>The terminating errors emitted during the invocation of the analyzer</summary>
            public IList<SMA.ErrorRecord> TerminatingErrors;
            /// <summary>The non-terminating errors emitted during the invocation of the analyzer</summary>
            public IList<SMA.ErrorRecord> Errors;
            /// <summary>The verbose messages emitted during the invocation of the analyzer</summary>
            public IList<string> Verbose;
            /// <summary>The debug messages emitted during the invocation of the analyzer</summary>
            public IList<string> Debug;
            /// <summary>The warning messages emitted during the invocation of the analyzer</summary>
            public IList<string> Warning;
            /// <summary>Add a terminating error the ccollection</summary>
            public void ThrowTerminatingError(SMA.ErrorRecord er) { TerminatingErrors.Add(er); }
            /// <summary>Add a non-terminating error the ccollection</summary>
            public void WriteError(SMA.ErrorRecord er) { Errors.Add(er); }
            /// <summary>Add a verbose message to the verbose collection</summary>
            public void WriteVerbose(string m) { Verbose.Add(m); }
            /// <summary>Add a debug message to the debug collection</summary>
            public void WriteDebug(string m) { Debug.Add(m); }
            /// <summary>Add a warning message to the warning collection</summary>
            public void WriteWarning(string m) { Warning.Add(m); }

            /// <summary>
            /// Clear the writer collections to avoid getting output from
            /// multiple invocations
            /// </summary>
            public void ClearWriter()
            {
                TerminatingErrors.Clear();
                Errors.Clear();
                Verbose.Clear();
                Debug.Clear();
                Warning.Clear();
            }

            /// <summary>
            /// Initialize all the output colections
            /// </summary>
            public hostedWriter()
            {
                TerminatingErrors = new List<SMA.ErrorRecord>();
                Errors = new List<SMA.ErrorRecord>();
                Verbose = new List<string>();
                Debug = new List<string>();
                Warning = new List<string>();
            }
        }

        /// <summary>Get the available builtin rules</summary>
        /// <param name="ruleNames">A collection of strings which contain the wildcard pattern for the rule</param>
        public List<RuleInfo> GetBuiltinRules(string[] ruleNames = null)
        {
            List<RuleInfo> builtinRules = new List<RuleInfo>();
            IEnumerable<IRule> rules = ScriptAnalyzer.Instance.GetRule(null, ruleNames);
            foreach ( IRule rule in rules )
            {
                builtinRules.Add(
                    new RuleInfo(
                        name: rule.GetName(),
                        commonName: rule.GetCommonName(),
                        description: rule.GetDescription(),
                        sourceType: rule.GetSourceType(),
                        sourceName: rule.GetSourceName(),
                        severity: rule.GetSeverity(),
                        implementingType: rule.GetType()
                        )
                    );
            }
            return builtinRules;
        }

        /// <summary>Dispose the PowerShell instance</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if ( disposed )
            {
                return;
            }

            if ( disposing )
            {
                Helper.Instance.Dispose();
                ps.Runspace.Dispose();
                ps.Dispose();
            }

            disposed = true;
        }
    }

    /// <summary>
    /// Type of entity that was passed to the analyzer
    /// </summary>
    public enum AnalysisType {
        /// <summary>An Ast was passed to the analyzer</summary>
        Ast,
        /// <summary>An FileInfo was passed to the analyzer</summary>
        File,
        /// <summary>An script (as a string) was passed to the analyzer</summary>
        Script
    }

    /// <summary>
    /// The encapsulated results of the analyzer
    /// </summary>
    public class AnalyzerResult
    {
        /// <summary>The type of entity which was analyzed</summary>
        public AnalysisType Type;
        /// <summary>The diagnostic records found during analysis</summary>
        public List<DiagnosticRecord> Result;
        /// <summary>The terminating errors which occurred during analysis</summary>
        public List<SMA.ErrorRecord> TerminatingErrors;
        /// <summary>The non-terminating errors which occurred during analysis</summary>
        public List<SMA.ErrorRecord> Errors;
        /// <summary>The verbose messages delivered during analysis</summary>
        public List<string> Verbose;
        /// <summary>The warning messages delivered during analysis</summary>
        public List<string> Warning;
        /// <summary>The debug messages delivered during analysis</summary>
        public List<string> Debug;

        /// <summary>
        /// initialize storage
        /// </summary>
        private AnalyzerResult()
        {
            Type = AnalysisType.Script;
            Result = new List<DiagnosticRecord>();
            TerminatingErrors = new List<SMA.ErrorRecord>();
            Errors = new List<SMA.ErrorRecord>();
            Verbose = new List<string>();
            Warning = new List<string>();
            Debug = new List<string>();
        }

        /// <summary>
        /// Create results from an invocation of the analyzer
        /// </summary>
        public AnalyzerResult(AnalysisType type, IEnumerable<DiagnosticRecord>records, HostedAnalyzer ha) : this()
        {
            Type = type;
            Result.AddRange(records);
            TerminatingErrors.AddRange(ha.writer.TerminatingErrors);
            Errors.AddRange(ha.writer.Errors);
            Verbose.AddRange(ha.writer.Verbose);
            Warning.AddRange(ha.writer.Warning);
            Debug.AddRange(ha.writer.Debug);
        }
    }

}
