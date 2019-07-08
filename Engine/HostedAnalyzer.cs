using System;
using SMA = System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
using System.Management.Automation.Language;
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
    public class HostedAnalyzer
    {
        private SMA.PowerShell ps;
        internal hostedWriter writer;
        private ScriptAnalyzer analyzer;
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
            analyzer.CleanUp();
            Helper.Instance = new Helper(
                    ps.Runspace.SessionStateProxy.InvokeCommand,
                    writer);
            Helper.Instance.Initialize();
            analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
        }

        /// <summary>
        /// Analyze a script in the form of an AST
        /// <param name="scriptast">A scriptblockast which represents the script to analyze</param>
        /// <param name="tokens">The tokens in the ast</param>
        /// <param name="filename">The name of the file which held the script, if there was one</param>
        /// </summary>
        public AnalyzerResult Analyze(ScriptBlockAst scriptast, Token[] tokens, string filename = null)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            var result = analyzer.AnalyzeSyntaxTree(scriptast, tokens, filename);
            return new AnalyzerResult(AnalysisType.Ast, result, this);
        }

        /// <summary>
        /// Analyze a script in the form of a string with additional Settings
        /// <param name="ScriptDefinition">The script as a string</param>
        /// <param name="settings">A hastable which includes the settings</param>
        /// </summary>
        public AnalyzerResult Analyze(string ScriptDefinition, Settings settings)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, settings.CustomRulePath == null ? null : settings.CustomRulePath.ToArray(),
                settings.IncludeRules == null ? null : settings.IncludeRules.ToArray(),
                settings.ExcludeRules == null ? null : settings.ExcludeRules.ToArray(),
                settings.Severities == null ? null : settings.Severities.ToArray(),
                settings.IncludeDefaultRules,
                false,
                null);
            var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
            return new AnalyzerResult(AnalysisType.Script, result, this);
        }

        /// <summary>
        /// Analyze a script in the form of a string with additional Settings
        /// <param name="ScriptDefinition">The script as a string</param>
        /// <param name="settings">A hastable which includes the settings</param>
        /// </summary>
        public AnalyzerResult Analyze(string ScriptDefinition, Hashtable settings)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            // var set = CreateSettings(settings);
            // analyzer.UpdateSettings(set);
            var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
            return new AnalyzerResult(AnalysisType.Script, result, this);
        }

        /// <summary>Analyze a script in the form of a string, based on default</summary>
        public AnalyzerResult Analyze(string ScriptDefinition)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
            return new AnalyzerResult(AnalysisType.Script, result, this);
        }

        /// <summary>Analyze a script based on passed settings</summary>
        public AnalyzerResult Analyze(string ScriptDefinition, AnalyzerConfiguration Settings)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, Settings.RulePath, Settings.IncludeRuleNames, Settings.ExcludeRuleNames, Settings.Severity, Settings.IncludeDefaultRules, Settings.SuppressedOnly); 
            var result = analyzer.AnalyzeScriptDefinition(ScriptDefinition);
            return new AnalyzerResult(AnalysisType.Script,  result, this);
        }

        /// <summary>Analyze a file based on passed settings</summary>
        public AnalyzerResult Analyze(FileInfo File, AnalyzerConfiguration Settings)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, Settings.RulePath, Settings.IncludeRuleNames, Settings.ExcludeRuleNames, Settings.Severity, Settings.IncludeDefaultRules, Settings.SuppressedOnly); 
            var result = analyzer.AnalyzePath(File.FullName, (x, y) => { return true; }, false);
            return new AnalyzerResult(AnalysisType.File, result, this);
        }

        /// <summary>Analyze a script in the form of a file</summary>
        public AnalyzerResult Analyze(FileInfo File)
        {
            writer.ClearWriter();
            analyzer.Initialize(ps.Runspace, writer, null, null, null, null, true, false, null);
            var result = analyzer.AnalyzePath(File.FullName, (x, y) => { return true; }, false);
            return new AnalyzerResult(AnalysisType.File, result, this);
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
        /// Create a default settings object
        /// </summary>
        public Settings CreateSettings()
        {
            Settings s = Settings.Create(null, 
                Directory.GetParent(Directory.GetParent(typeof(ScriptAnalyzer).Assembly.Location).FullName).FullName,
                writer,  ps.Runspace.SessionStateProxy.Path.GetResolvedProviderPathFromPSPath);
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
            string s = Formatter.Format(scriptDefinition, settings, null, ps.Runspace, writer);
            analyzer.CleanUp();
            return s;
        }

        /// <summary>
        /// An encapsulation of the arguments passed to Analyzer.Initialize
        /// they roughly equate to some of the parameters on the Invoke-ScriptAnalyzer
        /// cmdlet, but encapsulated to improve the experience.
        /// </summary>
        public class AnalyzerConfiguration
        {
            public string[] RulePath;
            public string[] IncludeRuleNames;
            public string[] ExcludeRuleNames;
            public string[] Severity;
            public bool IncludeDefaultRules = true;
            public bool SuppressedOnly = false;
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
    }
        
    /// <summary>
    /// The encapsulated rules of fixing a script
    /// </summary>
    public class FormattedScriptResult
    {
        /// <summary>The original script that was fixed</summary>
        public string OriginalScript;
        /// <summary>The script which has all the fixes</summary>
        public string FormattedScript;
        /// <summary>
        /// The analysis results.
        /// This includes all the output streams as well as the diagnostic records
        /// </summary>
        public AnalyzerResult Analysis;
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

    /// <summary>A public settings object</summary>
    public class PSSASettings
    {
        /// <summary>thing</summary>
        public bool RecurseCustomRulePath { get; set;} = false;
        /// <summary>thing</summary>
        public bool IncludeDefaultRules { get; set; } = false;
        /// <summary>thing</summary>
        public string FilePath { get; set; }
        /// <summary>thing</summary>
        public List<RuleSeverity> Severities  { get; set; }
        /// <summary>thing</summary>
        public List<string> CustomRulePath { get; set; }
        /// <summary>The rules which encapsulate an analyzer setting</summary>
        public List<PSSARule>Rules;

        /// <summary>Convert to hashtable so the analyzer method can use it</summary>
        public Hashtable ConvertToHashtable()
        {
            Hashtable ht = new Hashtable();
            return ht;
        }
    }

    /// <summary>Whether the rule should be included or excluded</summary>
    public enum RuleStatus {
        /// <summary>Include the rule</summary>
        Include,
        /// <summary>Exclude the rule</summary>
        Exclude
    }

    /// <summary>The encapsulation of a rule</summary>
    public class PSSARule
    {
        /// <summary>the name for a rule</summary>
        public string Name;
        /// <summary>Is the rule included or excluded</summary>
        public RuleStatus RuleAction;
        /// <summary>the settings for a rule</summary>
        public Dictionary<string, string>RuleSettings;

        /// <summary>Create a new rule, the default status is to include it</summary>
        public PSSARule(string name, RuleStatus status = RuleStatus.Include) {
            Name = name;
            RuleAction = status;
            RuleSettings = new Dictionary<string,string>();
        }

        /// <summary>
        /// Create a new rule, the default status is to include it
        /// <param name="name" />
        /// <param name="status" />
        /// <param name="ruleSettings" />
        /// </summary>
        public PSSARule(string name, RuleStatus status, Dictionary<string, string>ruleSettings) {
            Name = name;
            RuleAction = status;
            RuleSettings = ruleSettings;
        }

    }
}