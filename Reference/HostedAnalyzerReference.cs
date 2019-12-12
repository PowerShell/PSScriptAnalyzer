namespace Microsoft.Windows.PowerShell.ScriptAnalyzer {
  public class Position  {
    public Position(int line, int column) { }
    public Position(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position position) { }

    public int Column { get { return default(int); } }
    public int Line { get { return default(int); } }
    public override bool Equals ( object obj ) { return default(bool); }
    public override int GetHashCode (  ) { return default(int); }
    public static Microsoft.Windows.PowerShell.ScriptAnalyzer.Position Normalize ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position refPos, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position pos ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position); }
    public static bool operator == ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public static bool operator > ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public static bool operator >= ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public static bool operator != ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public static bool operator < ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public static bool operator <= ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position lhs, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position rhs ) { return default(bool); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Position Shift ( int lineDelta, int columnDelta ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position); }

  }

  public class Range  {
    internal Range() { }
    public Range(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position start, Microsoft.Windows.PowerShell.ScriptAnalyzer.Position end) { }
    public Range(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber) { }
    public Range(Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range) { }

    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Position End { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position); } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Position Start { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position); } }
    public static Microsoft.Windows.PowerShell.ScriptAnalyzer.Range Normalize ( Microsoft.Windows.PowerShell.ScriptAnalyzer.Position refPosition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Range); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Range Shift ( int lineDelta, int columnDelta ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Range); }

  }

  public class Settings  {
    public Settings(object settings, System.Func<string, string> presetResolver) { }
    public Settings(object settings) { }

    public System.Collections.Generic.List<string> CustomRulePath { get { return default(System.Collections.Generic.List<string>); } }
    public System.Collections.Generic.List<string> ExcludeRules { get { return default(System.Collections.Generic.List<string>); } }
    public string FilePath { get { return default(string); } }
    public bool IncludeDefaultRules { get { return default(bool); } set { } }
    public System.Collections.Generic.List<string> IncludeRules { get { return default(System.Collections.Generic.List<string>); } }
    public bool RecurseCustomRulePath { get { return default(bool); } set { } }
    public System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>> RuleArguments { get { return default(System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>); } }
    public System.Collections.Generic.List<string> Severities { get { return default(System.Collections.Generic.List<string>); } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings AddRuleArgument ( string name, System.Collections.Generic.Dictionary<string, object> setting ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings AddRuleArgument ( string rule, string setting, object value ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public static string GetSettingPresetFilePath ( string settingPreset ) { return default(string); }
    public static System.Collections.Generic.IEnumerable<string> GetSettingPresets (  ) { return default(System.Collections.Generic.IEnumerable<string>); }
    public static string GetShippedSettingsDirectory (  ) { return default(string); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings SetRuleArgument ( string rule, string setting, object value ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }

  }

  public class TextEdit : Microsoft.Windows.PowerShell.ScriptAnalyzer.Range {
    internal TextEdit() { }
    public TextEdit(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber, string newText) { }
    public TextEdit(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber, System.Collections.Generic.IEnumerable<string> lines) { }

    public int EndColumnNumber { get { return default(int); } }
    public int EndLineNumber { get { return default(int); } }
    public string[] Lines { get { return default(string[]); } }
    public int StartColumnNumber { get { return default(int); } }
    public int StartLineNumber { get { return default(int); } }
    public string Text { get { return default(string); } }
  }

}

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic {
  public class CorrectionExtent : Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit {
    public CorrectionExtent(int startLineNumber, int endLineNumber, int startColumnNumber, int endColumnNumber, string text, string file) { }
    public CorrectionExtent(int startLineNumber, int endLineNumber, int startColumnNumber, int endColumnNumber, System.Collections.Generic.IEnumerable<string> lines, string file, string description) { }
    public CorrectionExtent(int startLineNumber, int endLineNumber, int startColumnNumber, int endColumnNumber, string text, string file, string description) { }
    public CorrectionExtent(System.Management.Automation.Language.IScriptExtent violationExtent, string replacementText, string filePath, string description) { }
    public CorrectionExtent(System.Management.Automation.Language.IScriptExtent violationExtent, string replacementText, string filePath) { }

    public string Description { get { return default(string); } }
    public string File { get { return default(string); } }
  }

  public class DiagnosticRecord  {
    public DiagnosticRecord() { }
    public DiagnosticRecord(string message, System.Management.Automation.Language.IScriptExtent extent, string ruleName, Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity severity, string scriptPath, string ruleId, System.Collections.Generic.IEnumerable<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent> suggestedCorrections) { }

    public System.Management.Automation.Language.IScriptExtent Extent { get { return default(System.Management.Automation.Language.IScriptExtent); } protected  set { } }
    public string Message { get { return default(string); } protected  set { } }
    public string RuleName { get { return default(string); } protected  set { } }
    public string RuleSuppressionID { get { return default(string); } set { } }
    public string ScriptName { get { return default(string); } }
    public string ScriptPath { get { return default(string); } protected  set { } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity Severity { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity); } set { } }
    public System.Collections.Generic.IEnumerable<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent> SuggestedCorrections { get { return default(System.Collections.Generic.IEnumerable<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent>); } set { } }
  }

  public static class DiagnosticRecordHelper  {
    public static string FormatError ( string format, object[] args ) { return default(string); }

  }

  public enum DiagnosticSeverity : uint {
    Error = 2,
    Information = 0,
    ParseError = 3,
    Warning = 1,
  }

  public class RuleInfo  {
    public RuleInfo(string name, string commonName, string description, Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType sourceType, string sourceName, Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity severity) { }
    public RuleInfo(string name, string commonName, string description, Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType sourceType, string sourceName, Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity severity, System.Type implementingType) { }

    public string CommonName { get { return default(string); } private  set { } }
    public string Description { get { return default(string); } private  set { } }
    public System.Type ImplementingType { get { return default(System.Type); } private  set { } }
    public string RuleName { get { return default(string); } private  set { } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity Severity { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity); } private  set { } }
    public string SourceName { get { return default(string); } private  set { } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType SourceType { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType); } private  set { } }
    public override string ToString (  ) { return default(string); }

  }

  public enum RuleSeverity : uint {
    Error = 2,
    Information = 0,
    ParseError = 3,
    Warning = 1,
  }

  public enum SourceType : uint {
    Builtin = 0,
    Managed = 1,
    Module = 2,
  }

}
namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting {
  public enum AnalysisType {
    Ast = 0,
    File = 1,
    Script = 2,
  }

  public class AnalyzerResult  {
    public AnalyzerResult(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType type, System.Collections.Generic.IEnumerable<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord> records, Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer ha) { }

    public System.Collections.Generic.List<string> Debug { get { return default(System.Collections.Generic.List<string>); } set { } }
    public System.Collections.Generic.List<System.Management.Automation.ErrorRecord> Errors { get { return default(System.Collections.Generic.List<System.Management.Automation.ErrorRecord>); } set { } }
    public System.Collections.Generic.List<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord> Result { get { return default(System.Collections.Generic.List<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord>); } set { } }
    public System.Collections.Generic.List<System.Management.Automation.ErrorRecord> TerminatingErrors { get { return default(System.Collections.Generic.List<System.Management.Automation.ErrorRecord>); } set { } }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType Type { get { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType); } set { } }
    public System.Collections.Generic.List<string> Verbose { get { return default(System.Collections.Generic.List<string>); } set { } }
    public System.Collections.Generic.List<string> Warning { get { return default(System.Collections.Generic.List<string>); } set { } }
  }

  public class HostedAnalyzer : System.IDisposable {
    public HostedAnalyzer() { }

    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( System.Management.Automation.Language.ScriptBlockAst scriptast, System.Management.Automation.Language.Token[] tokens, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings, string filename ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( System.Management.Automation.Language.ScriptBlockAst scriptast, System.Management.Automation.Language.Token[] tokens, string filename ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( string ScriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( string ScriptDefinition, System.Collections.Hashtable settings ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( string ScriptDefinition ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( System.IO.FileInfo File, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult Analyze ( System.IO.FileInfo File ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult); }

    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( string ScriptDefinition ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( System.Management.Automation.Language.ScriptBlockAst scriptast, System.Management.Automation.Language.Token[] tokens, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings, string filename ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( System.Management.Automation.Language.ScriptBlockAst scriptast, System.Management.Automation.Language.Token[] tokens, string filename ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( string ScriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( string ScriptDefinition, System.Collections.Hashtable settings ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( System.IO.FileInfo File, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }
    public System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult> AnalyzeAsync ( System.IO.FileInfo File ) { return default(System.Threading.Tasks.Task<Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult>); }

    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings CreateSettings (  ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings CreateSettings ( string[] ruleNames ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings CreateSettings ( string SettingsName ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings CreateSettings ( System.Collections.Hashtable settings ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }
    public Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings CreateSettingsFromFile ( string settingsFile ) { return default(Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings); }

    public void Dispose (  ) { }

    public string Fix ( string scriptDefinition ) { return default(string); }
    public string Fix ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range ) { return default(string); }
    public string Fix ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(string); }
    public string Fix ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(string); }

    public System.Threading.Tasks.Task<string> FixAsync ( string scriptDefinition ) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FixAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range ) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FixAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FixAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(System.Threading.Tasks.Task<string>); }

    public string Format ( string scriptDefinition ) { return default(string); }
    public string Format ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range ) { return default(string); }
    public string Format ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings) { return default(string); }
    public string Format ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(string); }

    public System.Threading.Tasks.Task<string> FormatAsync ( string scriptDefinition ) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FormatAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range ) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FormatAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Range range, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings) { return default(System.Threading.Tasks.Task<string>); }
    public System.Threading.Tasks.Task<string> FormatAsync ( string scriptDefinition, Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings settings ) { return default(System.Threading.Tasks.Task<string>); }

    public System.Collections.Generic.List<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleInfo> GetBuiltinRules ( string[] ruleNames ) { return default(System.Collections.Generic.List<Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleInfo>); }
    public void Reset (  ) { }
    public override string ToString (  ) { return default(string); }

  }

}
