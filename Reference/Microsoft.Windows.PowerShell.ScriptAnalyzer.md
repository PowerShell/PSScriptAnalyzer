# PowerShell Script Analyzer - API Reference for HostedAnalyzer

## Contents

- [AnalysisType](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType')
  - [Ast](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-Ast 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType.Ast')
  - [File](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-File 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType.File')
  - [Script](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-Script 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType.Script')
- [AnalyzerResult](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult')
  - [#ctor()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-#ctor 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.#ctor')
  - [#ctor()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType,System-Collections-Generic-IEnumerable{Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord},Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.#ctor(Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType,System.Collections.Generic.IEnumerable{Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord},Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer)')
  - [Debug](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Debug 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Debug')
  - [Errors](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Errors 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Errors')
  - [Result](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Result 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Result')
  - [TerminatingErrors](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-TerminatingErrors 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.TerminatingErrors')
  - [Type](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Type 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Type')
  - [Verbose](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Verbose 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Verbose')
  - [Warning](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Warning 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult.Warning')
- [DiagnosticRecord](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord')
  - [#ctor()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-#ctor 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.#ctor')
  - [#ctor(message,extent,ruleName,severity,scriptPath,suggestedCorrections)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-#ctor-System-String,System-Management-Automation-Language-IScriptExtent,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity,System-String,System-String,System-Collections-Generic-IEnumerable{Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-CorrectionExtent}- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.#ctor(System.String,System.Management.Automation.Language.IScriptExtent,System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity,System.String,System.String,System.Collections.Generic.IEnumerable{Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent})')
  - [Extent](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Extent 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.Extent')
  - [Message](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Message 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.Message')
  - [RuleName](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-RuleName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.RuleName')
  - [RuleSuppressionID](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-RuleSuppressionID 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.RuleSuppressionID')
  - [ScriptName](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-ScriptName 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.ScriptName')
  - [ScriptPath](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-ScriptPath 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.ScriptPath')
  - [Severity](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Severity 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.Severity')
  - [SuggestedCorrections](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-SuggestedCorrections 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord.SuggestedCorrections')
- [DiagnosticSeverity](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity')
  - [Error](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Error 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity.Error')
  - [Information](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Information 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity.Information')
  - [ParseError](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-ParseError 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity.ParseError')
  - [Warning](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Warning 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity.Warning')
  - [VisitUnaryExpression(unaryExpressionAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-FlowGraph-VisitUnaryExpression-System-Management-Automation-Language-UnaryExpressionAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.FlowGraph.VisitUnaryExpression(System.Management.Automation.Language.UnaryExpressionAst)')
  - [VisitUsingExpression(usingExpressionAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-FlowGraph-VisitUsingExpression-System-Management-Automation-Language-UsingExpressionAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.FlowGraph.VisitUsingExpression(System.Management.Automation.Language.UsingExpressionAst)')
  - [VisitVariableExpression(variableExpressionAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-FlowGraph-VisitVariableExpression-System-Management-Automation-Language-VariableExpressionAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.FlowGraph.VisitVariableExpression(System.Management.Automation.Language.VariableExpressionAst)')
  - [VisitWhileStatement(whileStatementAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-FlowGraph-VisitWhileStatement-System-Management-Automation-Language-WhileStatementAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.FlowGraph.VisitWhileStatement(System.Management.Automation.Language.WhileStatementAst)')
- [HostedAnalyzer](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer')
  - [#ctor()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-#ctor 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.#ctor')
  - [Analyze(scriptast,tokens,settings,filename)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings,System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.Management.Automation.Language.ScriptBlockAst,System.Management.Automation.Language.Token[],Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings,System.String)')
  - [Analyze(scriptast,tokens,filename)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.Management.Automation.Language.ScriptBlockAst,System.Management.Automation.Language.Token[],System.String)')
  - [Analyze(ScriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [Analyze(ScriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String,System-Collections-Hashtable- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.String,System.Collections.Hashtable)')
  - [Analyze(ScriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.String)')
  - [Analyze(File,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-IO-FileInfo,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.IO.FileInfo,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [Analyze(File)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-IO-FileInfo- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Analyze(System.IO.FileInfo)')
  - [AnalyzeAsync(scriptast,tokens,settings,filename)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings,System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.Management.Automation.Language.ScriptBlockAst,System.Management.Automation.Language.Token[],Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings,System.String)')
  - [AnalyzeAsync(scriptast,tokens,filename)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.Management.Automation.Language.ScriptBlockAst,System.Management.Automation.Language.Token[],System.String)')
  - [AnalyzeAsync(ScriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [AnalyzeAsync(ScriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String,System-Collections-Hashtable- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.String,System.Collections.Hashtable)')
  - [AnalyzeAsync(ScriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.String)')
  - [AnalyzeAsync(File,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-IO-FileInfo,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.IO.FileInfo,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [AnalyzeAsync(File)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-IO-FileInfo- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.AnalyzeAsync(System.IO.FileInfo)')
  - [CreateSettings()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-String[]- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.CreateSettings(System.String[])')
  - [CreateSettings()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.CreateSettings(System.String)')
  - [CreateSettings()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.CreateSettings')
  - [CreateSettings()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-Collections-Hashtable- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.CreateSettings(System.Collections.Hashtable)')
  - [CreateSettingsFromFile()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettingsFromFile-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.CreateSettingsFromFile(System.String)')
  - [Dispose()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Dispose 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Dispose')
  - [Dispose()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Dispose-System-Boolean- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Dispose(System.Boolean)')
  - [Fix(scriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Fix(System.String)')
  - [Fix(scriptDefinition,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Fix(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [Fix(scriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Fix(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [Fix(scriptDefinition,range,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Fix(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [FixAsync(scriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FixAsync(System.String)')
  - [FixAsync(scriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FixAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [FixAsync(scriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FixAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [FixAsync(scriptDefinition,range,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FixAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [Format(scriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Format(System.String)')
  - [Format(scriptDefinition,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Format(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [Format(scriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Format(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [Format(scriptDefinition,settings,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Format(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [FormatAsync(scriptDefinition)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FormatAsync(System.String)')
  - [FormatAsync(scriptDefinition,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FormatAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [FormatAsync(scriptDefinition,settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FormatAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [FormatAsync(scriptDefinition,settings,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.FormatAsync(System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range,Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings)')
  - [GetBuiltinRules(ruleNames)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-GetBuiltinRules-System-String[]- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.GetBuiltinRules(System.String[])')
  - [Reset()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Reset 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.Reset')
  - [ToString()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-ToString 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer.ToString')
- [Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position')
  - [#ctor(line,column)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-#ctor-System-Int32,System-Int32- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.#ctor(System.Int32,System.Int32)')
  - [#ctor(position)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.#ctor(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [Column](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Column 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.Column')
  - [Line](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Line 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.Line')
  - [Equals()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Equals-System-Object- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.Equals(System.Object)')
  - [GetHashCode()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-GetHashCode 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.GetHashCode')
  - [Normalize(refPos,pos)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Normalize-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.Normalize(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [Shift(lineDelta,columnDelta)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Shift-System-Int32,System-Int32- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.Shift(System.Int32,System.Int32)')
  - [op_Equality()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_Equality-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_Equality(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [op_GreaterThan()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_GreaterThan-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_GreaterThan(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [op_GreaterThanOrEqual()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_GreaterThanOrEqual-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_GreaterThanOrEqual(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [op_Inequality()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_Inequality-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_Inequality(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [op_LessThan()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_LessThan-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_LessThan(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [op_LessThanOrEqual()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_LessThanOrEqual-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position.op_LessThanOrEqual(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
- [Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range')
  - [#ctor(start,end)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.#ctor(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Position)')
  - [#ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-System-Int32,System-Int32,System-Int32,System-Int32- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.#ctor(System.Int32,System.Int32,System.Int32,System.Int32)')
  - [#ctor(range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.#ctor(Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [End](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-End 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.End')
  - [Start](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Start 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.Start')
  - [Normalize(refPosition,range)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Normalize-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.Normalize(Microsoft.Windows.PowerShell.ScriptAnalyzer.Position,Microsoft.Windows.PowerShell.ScriptAnalyzer.Range)')
  - [Shift()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Shift-System-Int32,System-Int32- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range.Shift(System.Int32,System.Int32)')
- [RuleSeverity](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity')
  - [Error](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Error 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity.Error')
  - [Information](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Information 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity.Information')
  - [ParseError](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-ParseError 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity.ParseError')
  - [Warning](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Warning 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity.Warning')
- [Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings')
  - [#ctor(settings,presetResolver)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-#ctor-System-Object,System-Func{System-String,System-String}- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.#ctor(System.Object,System.Func{System.String,System.String})')
  - [#ctor(settings)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-#ctor-System-Object- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.#ctor(System.Object)')
  - [AddRuleArgument()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-AddRuleArgument-System-String,System-Collections-Generic-Dictionary{System-String,System-Object}- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.AddRuleArgument(System.String,System.Collections.Generic.Dictionary{System.String,System.Object})')
  - [AddRuleArgument()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-AddRuleArgument-System-String,System-String,System-Object- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.AddRuleArgument(System.String,System.String,System.Object)')
  - [ConvertToRuleArgumentType(ruleArgs)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-ConvertToRuleArgumentType-System-Object- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.ConvertToRuleArgumentType(System.Object)')
  - [Create(settingsObj,cwd,outputWriter,getResolvedProviderPathFromPSPathDelegate)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-Create-System-Object,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-IOutputWriter,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-PathResolver-GetResolvedProviderPathFromPSPath{System-String,System-Management-Automation-ProviderInfo,System-Collections-ObjectModel-Collection{System-String}}- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.Create(System.Object,System.String,Microsoft.Windows.PowerShell.ScriptAnalyzer.IOutputWriter,Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.PathResolver.GetResolvedProviderPathFromPSPath{System.String,System.Management.Automation.ProviderInfo,System.Collections.ObjectModel.Collection{System.String}})')
  - [GetDictionaryFromHashtable(hashtable)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetDictionaryFromHashtable-System-Collections-Hashtable- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetDictionaryFromHashtable(System.Collections.Hashtable)')
  - [GetSafeValueFromExpressionAst(exprAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValueFromExpressionAst-System-Management-Automation-Language-ExpressionAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetSafeValueFromExpressionAst(System.Management.Automation.Language.ExpressionAst)')
  - [GetSafeValueFromHashtableAst(hashtableAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValueFromHashtableAst-System-Management-Automation-Language-HashtableAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetSafeValueFromHashtableAst(System.Management.Automation.Language.HashtableAst)')
  - [GetSafeValuesFromArrayAst(arrLiteralAst)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValuesFromArrayAst-System-Management-Automation-Language-ArrayLiteralAst- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetSafeValuesFromArrayAst(System.Management.Automation.Language.ArrayLiteralAst)')
  - [GetSettingPresetFilePath()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSettingPresetFilePath-System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetSettingPresetFilePath(System.String)')
  - [GetSettingPresets()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSettingPresets 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetSettingPresets')
  - [GetShippedSettingsDirectory()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetShippedSettingsDirectory 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.GetShippedSettingsDirectory')
  - [SetRuleArgument()](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-SetRuleArgument-System-String,System-String,System-Object- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings.SetRuleArgument(System.String,System.String,System.Object)')
- [SourceType](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType')
  - [Builtin](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Builtin 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType.Builtin')
  - [Managed](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Managed 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType.Managed')
  - [Module](#F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Module 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType.Module')
- [TextEdit](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit')
  - [#ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber,newText)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-#ctor-System-Int32,System-Int32,System-Int32,System-Int32,System-String- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.#ctor(System.Int32,System.Int32,System.Int32,System.Int32,System.String)')
  - [#ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber,lines)](#M-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-#ctor-System-Int32,System-Int32,System-Int32,System-Int32,System-Collections-Generic-IEnumerable{System-String}- 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.#ctor(System.Int32,System.Int32,System.Int32,System.Int32,System.Collections.Generic.IEnumerable{System.String})')
  - [EndColumnNumber](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-EndColumnNumber 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.EndColumnNumber')
  - [EndLineNumber](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-EndLineNumber 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.EndLineNumber')
  - [StartColumnNumber](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-StartColumnNumber 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.StartColumnNumber')
  - [StartLineNumber](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-StartLineNumber 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.StartLineNumber')
  - [Text](#P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-Text 'Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit.Text')

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType'></a>
## AnalysisType `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting

##### Summary

Type of entity that was passed to the analyzer

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-Ast'></a>
### Ast `constants`

##### Summary

An Ast was passed to the analyzer.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-File'></a>
### File `constants`

##### Summary

An FileInfo was passed to the analyzer.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType-Script'></a>
### Script `constants`

##### Summary

An script (as a string) was passed to the analyzer.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult'></a>
## AnalyzerResult `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting

##### Summary

The encapsulated results of the analyzer

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-#ctor'></a>
### #ctor() `constructor`

##### Summary

initialize storage

##### Parameters

This constructor has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalysisType,System-Collections-Generic-IEnumerable{Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord},Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-'></a>
### #ctor() `constructor`

##### Summary

Create results from an invocation of the analyzer

##### Parameters

This constructor has no parameters.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Debug'></a>
### Debug `property`

##### Summary

The debug messages delivered during analysis.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Errors'></a>
### Errors `property`

##### Summary

The non-terminating errors which occurred during analysis.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Result'></a>
### Result `property`

##### Summary

The diagnostic records found during analysis.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-TerminatingErrors'></a>
### TerminatingErrors `property`

##### Summary

The terminating errors which occurred during analysis.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Type'></a>
### Type `property`

##### Summary

The type of entity which was analyzed.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Verbose'></a>
### Verbose `property`

##### Summary

The verbose messages delivered during analysis.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-AnalyzerResult-Warning'></a>
### Warning `property`

##### Summary

The warning messages delivered during analysis.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord'></a>
## DiagnosticRecord `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic

##### Summary

Represents a result from a PSScriptAnalyzer rule.
It contains a message, extent, rule name, and severity.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-#ctor'></a>
### #ctor() `constructor`

##### Summary

DiagnosticRecord: The constructor for DiagnosticRecord class.

##### Parameters

This constructor has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-#ctor-System-String,System-Management-Automation-Language-IScriptExtent,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity,System-String,System-String,System-Collections-Generic-IEnumerable{Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-CorrectionExtent}-'></a>
### #ctor(message,extent,ruleName,severity,scriptPath,suggestedCorrections) `constructor`

##### Summary

DiagnosticRecord: The constructor for DiagnosticRecord class that takes in suggestedCorrection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | A string about why this diagnostic was created |
| extent | [System.Management.Automation.Language.IScriptExtent](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.IScriptExtent 'System.Management.Automation.Language.IScriptExtent') | The place in the script this diagnostic refers to |
| ruleName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the rule that created this diagnostic |
| severity | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity') | The severity of this diagnostic |
| scriptPath | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The full path of the script file being analyzed |
| suggestedCorrections | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The correction suggested by the rule to replace the extent text |

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Extent'></a>
### Extent `property`

##### Summary

Represents a span of text in a script.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Message'></a>
### Message `property`

##### Summary

Represents a string from the rule about why this diagnostic was created.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-RuleName'></a>
### RuleName `property`

##### Summary

Represents the name of a script analyzer rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-RuleSuppressionID'></a>
### RuleSuppressionID `property`

##### Summary

Returns the rule id for this record

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-ScriptName'></a>
### ScriptName `property`

##### Summary

Represents the name of the script file that is under analysis

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-ScriptPath'></a>
### ScriptPath `property`

##### Summary

Returns the path of the script.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-Severity'></a>
### Severity `property`

##### Summary

Represents a severity level of an issue.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticRecord-SuggestedCorrections'></a>
### SuggestedCorrections `property`

##### Summary

Returns suggested correction
return value can be null

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity'></a>
## DiagnosticSeverity `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic

##### Summary

Represents a severity level of an issue.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Error'></a>
### Error `constants`

##### Summary

ERROR: This diagnostic is likely to cause a problem or does not follow PowerShell's required guidelines.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Information'></a>
### Information `constants`

##### Summary

Information: This diagnostic is trivial, but may be useful.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-ParseError'></a>
### ParseError `constants`

##### Summary

ERROR: This diagnostic is caused by an actual parsing error, and is generated only by the engine.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-DiagnosticSeverity-Warning'></a>
### Warning `constants`

##### Summary

WARNING: This diagnostic may cause a problem or does not follow PowerShell's recommended guidelines.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer'></a>
## HostedAnalyzer `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting

##### Summary

This is a helper class which provides a consistent, easy to use set of interfaces for Script Analyzer
This class will enable you to:
 - Analyze a script or file with any configuration which is supported by Invoke-ScriptAnalyzer
 - Reformat a script as is done via Invoke-Formatter

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-#ctor'></a>
### #ctor() `constructor`

##### Summary

Create an instance of the hosted analyzer

##### Parameters

This constructor has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings,System-String-'></a>
### Analyze(scriptast,tokens,settings,filename) `method`

##### Summary

Analyze a script in the form of an AST

##### Returns

An AnalyzerResult which encapsulates the analysis of the ast.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptast | [System.Management.Automation.Language.ScriptBlockAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ScriptBlockAst 'System.Management.Automation.Language.ScriptBlockAst') | A scriptblockast which represents the script to analyze. |
| tokens | [System.Management.Automation.Language.Token[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.Token[] 'System.Management.Automation.Language.Token[]') | The tokens in the ast. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | A settings object which defines which rules to run. |
| filename | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the file which held the script, if there was one. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],System-String-'></a>
### Analyze(scriptast,tokens,filename) `method`

##### Summary

Analyze a script in the form of an AST

##### Returns

An AnalyzerResult which encapsulates the analysis of the ast.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptast | [System.Management.Automation.Language.ScriptBlockAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ScriptBlockAst 'System.Management.Automation.Language.ScriptBlockAst') | A scriptblockast which represents the script to analyze. |
| tokens | [System.Management.Automation.Language.Token[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.Token[] 'System.Management.Automation.Language.Token[]') | The tokens in the ast. |
| filename | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the file which held the script, if there was one. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Analyze(ScriptDefinition,settings) `method`

##### Summary

Analyze a script in the form of a string with additional Settings

##### Returns

An AnalyzerResult which encapsulates the analysis of the script definition.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script as a string. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | A hastable which includes the settings. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String,System-Collections-Hashtable-'></a>
### Analyze(ScriptDefinition,settings) `method`

##### Summary

Analyze a script in the form of a string with additional Settings

##### Returns

An AnalyzerResult which encapsulates the analysis of the script definition.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script as a string. |
| settings | [System.Collections.Hashtable](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Hashtable 'System.Collections.Hashtable') | A hastable which includes the settings. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-String-'></a>
### Analyze(ScriptDefinition) `method`

##### Summary

Analyze a script asynchronously in the form of a string, based on default settings

##### Returns

An AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script (as a string) to analyze. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-IO-FileInfo,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Analyze(File,settings) `method`

##### Summary

Analyze a script in the form of a file.

##### Returns

An AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| File | [System.IO.FileInfo](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.FileInfo 'System.IO.FileInfo') | The file as a FileInfo object to analyze. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | A settings object which defines which rules to run. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Analyze-System-IO-FileInfo-'></a>
### Analyze(File) `method`

##### Summary

Analyze a script in the form of a file.

##### Returns

An AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| File | [System.IO.FileInfo](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.FileInfo 'System.IO.FileInfo') | The file as a FileInfo object to analyze. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings,System-String-'></a>
### AnalyzeAsync(scriptast,tokens,settings,filename) `method`

##### Summary

Analyze a script in the form of an AST

##### Returns

An AnalyzerResult which encapsulates the analysis of the ast.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptast | [System.Management.Automation.Language.ScriptBlockAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ScriptBlockAst 'System.Management.Automation.Language.ScriptBlockAst') | A scriptblockast which represents the script to analyze. |
| tokens | [System.Management.Automation.Language.Token[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.Token[] 'System.Management.Automation.Language.Token[]') | The tokens in the ast. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | A settings object which defines which rules to run. |
| filename | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the file which held the script, if there was one. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-Management-Automation-Language-ScriptBlockAst,System-Management-Automation-Language-Token[],System-String-'></a>
### AnalyzeAsync(scriptast,tokens,filename) `method`

##### Summary

Analyze a script in the form of an AST

##### Returns

An AnalyzerResult which encapsulates the analysis of the ast.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptast | [System.Management.Automation.Language.ScriptBlockAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ScriptBlockAst 'System.Management.Automation.Language.ScriptBlockAst') | A scriptblockast which represents the script to analyze. |
| tokens | [System.Management.Automation.Language.Token[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.Token[] 'System.Management.Automation.Language.Token[]') | The tokens in the ast. |
| filename | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the file which held the script, if there was one. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### AnalyzeAsync(ScriptDefinition,settings) `method`

##### Summary

Analyze a script in the form of a string with additional Settings

##### Returns

An AnalyzerResult which encapsulates the analysis of the script definition.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script as a string. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | A hastable which includes the settings. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String,System-Collections-Hashtable-'></a>
### AnalyzeAsync(ScriptDefinition,settings) `method`

##### Summary

Analyze a script in the form of a string with additional Settings

##### Returns

An AnalyzerResult which encapsulates the analysis of the script definition.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script as a string. |
| settings | [System.Collections.Hashtable](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Hashtable 'System.Collections.Hashtable') | A hastable which includes the settings. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-String-'></a>
### AnalyzeAsync(ScriptDefinition) `method`

##### Summary

Analyze a script asynchronously in the form of a string, based on default

##### Returns

A Task which encapsulates an AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ScriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script (as a string) to analyze. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-IO-FileInfo,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### AnalyzeAsync(File,settings) `method`

##### Summary

Analyze a script asynchronously in the form of a string, based on default

##### Returns

A Task which encapsulates an AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| File | [System.IO.FileInfo](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.FileInfo 'System.IO.FileInfo') | The file that contains the script. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when analyzing the script. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-AnalyzeAsync-System-IO-FileInfo-'></a>
### AnalyzeAsync(File) `method`

##### Summary

Analyze a script in the form of a file.

##### Returns

An AnalyzerResult.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| File | [System.IO.FileInfo](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.FileInfo 'System.IO.FileInfo') | The file as a FileInfo object to analyze. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-String[]-'></a>
### CreateSettings() `method`

##### Summary

Create a standard settings object for Script Analyzer
This is the object used by analyzer internally
It is more functional than the AnalyzerSettings object because
it contains the Rule Arguments which are not passable to the Initialize method

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-String-'></a>
### CreateSettings() `method`

##### Summary

Create a standard settings object for Script Analyzer
This is the object used by analyzer internally
It is more functional than the AnalyzerSettings object because
it contains the Rule Arguments which are not passable to the Initialize method

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings'></a>
### CreateSettings() `method`

##### Summary

Create a default settings object

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettings-System-Collections-Hashtable-'></a>
### CreateSettings() `method`

##### Summary

Create a standard settings object for Script Analyzer
This is the object used by analyzer internally
It is more functional than the AnalyzerSettings object because
it contains the Rule Arguments which are not passable to the Initialize method

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-CreateSettingsFromFile-System-String-'></a>
### CreateSettingsFromFile() `method`

##### Summary

Create a standard settings object for Script Analyzer from an existing .psd1 file

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Dispose'></a>
### Dispose() `method`

##### Summary

Dispose the Hosted Analyzer resources.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Dispose-System-Boolean-'></a>
### Dispose() `method`

##### Summary

Dispose the Helper, runspace, and Powershell instance

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String-'></a>
### Fix(scriptDefinition) `method`

##### Summary

Uses the default rules to fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### Fix(scriptDefinition,range) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The range of the script to fix. If not supplied, the entire script will be fixed.false |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Fix(scriptDefinition,settings) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when fixing. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Fix-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Fix(scriptDefinition,range,settings) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The range of the script to fix. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when fixing. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String-'></a>
### FixAsync(scriptDefinition) `method`

##### Summary

Uses the default rules to fix a script.

##### Returns

The task to fix a script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### FixAsync(scriptDefinition) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### FixAsync(scriptDefinition,settings) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when fixing. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FixAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### FixAsync(scriptDefinition,range,settings) `method`

##### Summary

Fix a script.

##### Returns

The fixed script as a string.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to fix. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The range of the script to use when fixing. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when fixing. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String-'></a>
### Format(scriptDefinition) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### Format(scriptDefinition,range) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The range of the script to format |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Format(scriptDefinition,settings) `method`

##### Summary

Format a script based on settings

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when formatting. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Format-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### Format(scriptDefinition,settings,range) `method`

##### Summary

Format a script based on settings

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The settings to use when formatting. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The range over which to apply the formatting. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String-'></a>
### FormatAsync(scriptDefinition) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### FormatAsync(scriptDefinition,range) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The range of the script to format |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### FormatAsync(scriptDefinition,settings) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing
and the union of the actual settings which are passed to it.

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The settings to use when formatting. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-FormatAsync-System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range,Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-'></a>
### FormatAsync(scriptDefinition,settings,range) `method`

##### Summary

Format a script according to the formatting rules
    PSPlaceCloseBrace
    PSPlaceOpenBrace
    PSUseConsistentWhitespace
    PSUseConsistentIndentation
    PSAlignAssignmentStatement
    PSUseCorrectCasing
and the union of the actual settings which are passed to it.

##### Returns

The formatted script.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| scriptDefinition | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The script to format. |
| settings | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | The settings to use when formatting. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings') | The range over which to apply the formatting. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-GetBuiltinRules-System-String[]-'></a>
### GetBuiltinRules(ruleNames) `method`

##### Summary

Get the available builtin rules.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ruleNames | [System.String[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String[] 'System.String[]') | A collection of strings which contain the wildcard pattern for the rule. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-Reset'></a>
### Reset() `method`

##### Summary

Reset the the analyzer and associated state.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Hosting-HostedAnalyzer-ToString'></a>
### ToString() `method`

##### Summary

A simple ToString, knowing the runspace can help with debugging.

##### Parameters

This method has no parameters.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position'></a>
## Position `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer

##### Summary

Class to represent position in text.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-#ctor-System-Int32,System-Int32-'></a>
### #ctor(line,column) `constructor`

##### Summary

Constructs a Position object.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| line | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number. |
| column | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based column number. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### #ctor(position) `constructor`

##### Summary

Copy constructor.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| position | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | Object to be copied. |

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Column'></a>
### Column `property`

##### Summary

Column number of the position.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Line'></a>
### Line `property`

##### Summary

Line number of the position.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Equals-System-Object-'></a>
### Equals() `method`

##### Summary

Checks of this object is equal the input object.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-GetHashCode'></a>
### GetHashCode() `method`

##### Summary

Returns the hash code of this object

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Normalize-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### Normalize(refPos,pos) `method`

##### Summary

Normalize position with respect to a reference position.

##### Returns

A Position object with normalized position.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| refPos | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | Reference position. |
| pos | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | Position to be normalized. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-Shift-System-Int32,System-Int32-'></a>
### Shift(lineDelta,columnDelta) `method`

##### Summary

Shift the position by given line and column deltas.

##### Returns

A new Position object with the shifted position.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| lineDelta | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Number of lines to shift the position. |
| columnDelta | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | Number of columns to shift the position. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_Equality-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_Equality() `method`

##### Summary

Checks if two position objects are equal.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_GreaterThan-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_GreaterThan() `method`

##### Summary

Checks if the left hand position comes after the right hand position.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_GreaterThanOrEqual-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_GreaterThanOrEqual() `method`

##### Summary

Checks if the left hand position comes after or is at the same position as that of the right hand position.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_Inequality-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_Inequality() `method`

##### Summary

Checks if the position objects are not equal.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_LessThan-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_LessThan() `method`

##### Summary

Checks if the left hand position comes before the right hand position.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-op_LessThanOrEqual-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### op_LessThanOrEqual() `method`

##### Summary

Checks if the left hand position comes before or is at the same position as that of the right hand position.

##### Parameters

This method has no parameters.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range'></a>
## Range `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer

##### Summary

Class to represent range in text. Range is represented as a pair of positions, [Start, End).

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Position-'></a>
### #ctor(start,end) `constructor`

##### Summary

Constructs a Range object to represent a range of text.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| start | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | The start position of the text. |
| end | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | The end position of the text, such that range is [start, end). |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-System-Int32,System-Int32,System-Int32,System-Int32-'></a>
### #ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber) `constructor`

##### Summary

Constructs a Range object to represent a range of text.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| startLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text starts. |
| startColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on start line at which the text starts. This includes the first character of the text. |
| endLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text ends. |
| endColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on end line at which the text ends. This offset value is 1 more than the offset of the last character of the text. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-#ctor-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### #ctor(range) `constructor`

##### Summary

Copy constructor

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | A Range object |

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-End'></a>
### End `property`

##### Summary

End position of the range.

 This position does not contain the last character of the range, but instead is the position
 right after the last character in the range.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Start'></a>
### Start `property`

##### Summary

Start position of the range.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Normalize-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position,Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-'></a>
### Normalize(refPosition,range) `method`

##### Summary

Normalize a range with respect to the input position.

##### Returns

Range object with normalized positions.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| refPosition | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Position](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Position 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Position') | Reference position. |
| range | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Range](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Range') | Range to be normalized. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Range-Shift-System-Int32,System-Int32-'></a>
### Shift() `method`

##### Summary

Returns a new range object with shifted positions.

##### Parameters

This method has no parameters.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo'></a>
## RuleInfo `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic

##### Summary

Represents an internal class to properly display the name and description of a rule.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-#ctor-System-String,System-String,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-'></a>
### #ctor(name,commonName,description,sourceType,sourceName) `constructor`

##### Summary

Constructor for a RuleInfo.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Name of the rule. |
| commonName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Common Name of the rule. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Description of the rule. |
| sourceType | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType') | Source type of the rule. |
| sourceName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Source name of the rule. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-#ctor-System-String,System-String,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity,System-Type-'></a>
### #ctor(name,commonName,description,sourceType,sourceName,implementingType) `constructor`

##### Summary

Constructor for a RuleInfo.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Name of the rule. |
| commonName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Common Name of the rule. |
| description | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Description of the rule. |
| sourceType | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType') | Source type of the rule. |
| sourceName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Source name of the rule. |
| implementingType | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity') | The dotnet type of the rule. |

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-CommonName'></a>
### CommonName `property`

##### Summary

Name: The common name of the rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-Description'></a>
### Description `property`

##### Summary

Description: The description of the rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-ImplementingType'></a>
### ImplementingType `property`

##### Summary

ImplementingType : The type which implements the rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-RuleName'></a>
### RuleName `property`

##### Summary

Name: The name of the rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-Severity'></a>
### Severity `property`

##### Summary

Severity : The severity of the rule violation.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-SourceName'></a>
### SourceName `property`

##### Summary

SourceName : The source name of the rule.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleInfo-SourceType'></a>
### SourceType `property`

##### Summary

SourceType: The source type of the rule.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity'></a>
## RuleSeverity `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic

##### Summary

Represents the severity of a PSScriptAnalyzer rule

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Error'></a>
### Error `constants`

##### Summary

ERROR: This warning is likely to cause a problem or does not follow PowerShell's required guidelines.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Information'></a>
### Information `constants`

##### Summary

Information: This warning is trivial, but may be useful. They are recommended by PowerShell best practice.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-ParseError'></a>
### ParseError `constants`

##### Summary

ERROR: This diagnostic is caused by an actual parsing error, and is generated only by the engine.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-RuleSeverity-Warning'></a>
### Warning `constants`

##### Summary

WARNING: This warning may cause a problem or does not follow PowerShell's recommended guidelines.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings'></a>
## Settings `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer

##### Summary

A class to represent the settings provided to ScriptAnalyzer class.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-#ctor-System-Object,System-Func{System-String,System-String}-'></a>
### #ctor(settings,presetResolver) `constructor`

##### Summary

Create a settings object from the input object.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| settings | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | An input object of type Hashtable or string. |
| presetResolver | [System.Func{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.String}') | A function that takes in a preset and resolves it to a path. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-#ctor-System-Object-'></a>
### #ctor(settings) `constructor`

##### Summary

Create a Settings object from the input object.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| settings | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | An input object of type Hashtable or string. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-AddRuleArgument-System-String,System-Collections-Generic-Dictionary{System-String,System-Object}-'></a>
### AddRuleArgument() `method`

##### Summary

Add a configurable rule to the rule arguments

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-AddRuleArgument-System-String,System-String,System-Object-'></a>
### AddRuleArgument() `method`

##### Summary

Add a configuration element to a rule

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-ConvertToRuleArgumentType-System-Object-'></a>
### ConvertToRuleArgumentType(ruleArgs) `method`

##### Summary

Sets the arguments for consumption by rules

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ruleArgs | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | A hashtable with rule names as keys |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-Create-System-Object,System-String,Microsoft-Windows-PowerShell-ScriptAnalyzer-IOutputWriter,Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-PathResolver-GetResolvedProviderPathFromPSPath{System-String,System-Management-Automation-ProviderInfo,System-Collections-ObjectModel-Collection{System-String}}-'></a>
### Create(settingsObj,cwd,outputWriter,getResolvedProviderPathFromPSPathDelegate) `method`

##### Summary

Create a settings object from an input object.

##### Returns

An object of Settings type.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| settingsObj | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | An input object of type Hashtable or string. |
| cwd | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The path in which to search for a settings file. |
| outputWriter | [Microsoft.Windows.PowerShell.ScriptAnalyzer.IOutputWriter](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-IOutputWriter 'Microsoft.Windows.PowerShell.ScriptAnalyzer.IOutputWriter') | An output writer. |
| getResolvedProviderPathFromPSPathDelegate | [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.PathResolver.GetResolvedProviderPathFromPSPath{System.String,System.Management.Automation.ProviderInfo,System.Collections.ObjectModel.Collection{System.String}}](#T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-PathResolver-GetResolvedProviderPathFromPSPath{System-String,System-Management-Automation-ProviderInfo,System-Collections-ObjectModel-Collection{System-String}} 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.PathResolver.GetResolvedProviderPathFromPSPath{System.String,System.Management.Automation.ProviderInfo,System.Collections.ObjectModel.Collection{System.String}}') | The GetResolvedProviderPathFromPSPath method from PSCmdlet to resolve relative path including wildcard support. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetDictionaryFromHashtable-System-Collections-Hashtable-'></a>
### GetDictionaryFromHashtable(hashtable) `method`

##### Summary

Recursively convert hashtable to dictionary

##### Returns

Dictionary that maps string to object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| hashtable | [System.Collections.Hashtable](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Hashtable 'System.Collections.Hashtable') |  |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValueFromExpressionAst-System-Management-Automation-Language-ExpressionAst-'></a>
### GetSafeValueFromExpressionAst(exprAst) `method`

##### Summary

Evaluates all statically evaluable, side-effect-free expressions under an
expression AST to return a value.
Throws if an expression cannot be safely evaluated.
Attempts to replicate the GetSafeValue() method on PowerShell AST methods from PSv5.

##### Returns

The .NET value represented by the PowerShell expression.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| exprAst | [System.Management.Automation.Language.ExpressionAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ExpressionAst 'System.Management.Automation.Language.ExpressionAst') | The expression AST to try to evaluate. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValueFromHashtableAst-System-Management-Automation-Language-HashtableAst-'></a>
### GetSafeValueFromHashtableAst(hashtableAst) `method`

##### Summary

Create a hashtable value from a PowerShell AST representing one,
provided that the PowerShell expression is statically evaluable and safe.

##### Returns

The Hashtable as a hydrated .NET value.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| hashtableAst | [System.Management.Automation.Language.HashtableAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.HashtableAst 'System.Management.Automation.Language.HashtableAst') | The PowerShell representation of the hashtable value. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSafeValuesFromArrayAst-System-Management-Automation-Language-ArrayLiteralAst-'></a>
### GetSafeValuesFromArrayAst(arrLiteralAst) `method`

##### Summary

Process a PowerShell array literal with statically evaluable/safe contents
into a .NET value.

##### Returns

The .NET value represented by PowerShell syntax.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| arrLiteralAst | [System.Management.Automation.Language.ArrayLiteralAst](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Management.Automation.Language.ArrayLiteralAst 'System.Management.Automation.Language.ArrayLiteralAst') | The PowerShell array AST to turn into a value. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSettingPresetFilePath-System-String-'></a>
### GetSettingPresetFilePath() `method`

##### Summary

Gets the path to the settings file corresponding to the given preset.

 If the corresponding preset file is not found, the method returns null.

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetSettingPresets'></a>
### GetSettingPresets() `method`

##### Summary

Returns the builtin setting presets

 Looks for powershell data files (*.psd1) in the PSScriptAnalyzer module settings directory
 and returns the names of the files without extension

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-GetShippedSettingsDirectory'></a>
### GetShippedSettingsDirectory() `method`

##### Summary

Retrieves the Settings directory from the Module directory structure

##### Parameters

This method has no parameters.

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-Settings-SetRuleArgument-System-String,System-String,System-Object-'></a>
### SetRuleArgument() `method`

##### Summary

Allow for changing setting of an existing value

##### Parameters

This method has no parameters.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType'></a>
## SourceType `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic

##### Summary

Represents a source name of a script analyzer rule.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Builtin'></a>
### Builtin `constants`

##### Summary

BUILTIN: Indicates the script analyzer rule is contributed as a built-in rule.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Managed'></a>
### Managed `constants`

##### Summary

MANAGED: Indicates the script analyzer rule is contributed as a managed rule.

<a name='F-Microsoft-Windows-PowerShell-ScriptAnalyzer-Generic-SourceType-Module'></a>
### Module `constants`

##### Summary

MODULE: Indicates the script analyzer rule is contributed as a Windows PowerShell module rule.

<a name='T-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit'></a>
## TextEdit `type`

##### Namespace

Microsoft.Windows.PowerShell.ScriptAnalyzer

##### Summary

Class to provide information about an edit

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-#ctor-System-Int32,System-Int32,System-Int32,System-Int32,System-String-'></a>
### #ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber,newText) `constructor`

##### Summary

Constructs a TextEdit object.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| startLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text, which needs to be replaced, starts. |
| startColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on start line at which the text, which needs to be replaced, starts. This includes the first character of the text. |
| endLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text, which needs to be replace, ends. |
| endColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on end line at which the text, which needs to be replaced, ends. This offset value is 1 more than the offset of the last character of the text. |
| newText | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The text that will replace the text bounded by the Line/Column number properties. |

<a name='M-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-#ctor-System-Int32,System-Int32,System-Int32,System-Int32,System-Collections-Generic-IEnumerable{System-String}-'></a>
### #ctor(startLineNumber,startColumnNumber,endLineNumber,endColumnNumber,lines) `constructor`

##### Summary

Constructs a TextEdit object.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| startLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text, which needs to be replaced, starts. |
| startColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on start line at which the text, which needs to be replaced, starts. This includes the first character of the text. |
| endLineNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based line number on which the text, which needs to be replace, ends. |
| endColumnNumber | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | 1-based offset on end line at which the text, which needs to be replaced, ends. This offset value is 1 more than the offset of the last character of the text. |
| lines | [System.Collections.Generic.IEnumerable{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.String}') | The contiguous lines that will replace the text bounded by the Line/Column number properties. |

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-EndColumnNumber'></a>
### EndColumnNumber `property`

##### Summary

1-based offset on end line at which the text, which needs to be replaced, ends.
This offset value is 1 more than the offset of the last character of the text.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-EndLineNumber'></a>
### EndLineNumber `property`

##### Summary

1-based line number on which the text, which needs to be replace, ends.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-StartColumnNumber'></a>
### StartColumnNumber `property`

##### Summary

1-based offset on start line at which the text, which needs to be replaced, starts.
This includes the first character of the text.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-StartLineNumber'></a>
### StartLineNumber `property`

##### Summary

1-based line number on which the text, which needs to be replaced, starts.

<a name='P-Microsoft-Windows-PowerShell-ScriptAnalyzer-TextEdit-Text'></a>
### Text `property`

##### Summary

The text that will replace the text bounded by the Line/Column number properties.
