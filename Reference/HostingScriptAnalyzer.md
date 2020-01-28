# Hosting PowerShell Script Analyzer

With the release of PowerShell Script Analyzer 1.18.4 a new mechanism for accessing script analyzer functionality is available.
A new type, `Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer`, is available to improve the experience of using PSScriptAnalyzer from c-sharp code.
Additionally a reference library is available in the PowerShell.ScriptAnalyzer.nuget package which may be used during development.
The PSScriptAnalyzer module is still required for this new type to be used.
The senario for use is that the hosting code may be used in a module or assembly which is then imported or loaded into an existing PowerShell session which has already imported the PSScriptAnalyzer module.

## Hosting Types

The following types are available via the reference package

```cs
Microsoft.Windows.PowerShell.ScriptAnalyzer.Position
Microsoft.Windows.PowerShell.ScriptAnalyzer.Range
Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings
Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit
Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalysisType
Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult
Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecordHelper
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleInfo
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.RuleSeverity
Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.SourceType
```

The main type for hosted analysis is `Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer`.
This is type provides the foundation for your code in accessing script analysis functionality.

## HostedAnalyzer APIs

The methods of `HostedAnalyzer` are divided into 4 areas of functionality:

### Analysis

The following synchronous and asynchronous APIs are available

```cs
AnalyzerResult Analyze(ScriptBlockAst scriptBlock, Token[] tokens, string fileName)
AnalyzerResult Analyze(ScriptBlockAst scriptBlock, Token[] tokens, Settings settings, string fileName)
AnalyzerResult Analyze(string scriptDefinition)
AnalyzerResult Analyze(string scriptDefinition, Hashtable settings)
AnalyzerResult Analyze(string scriptDefinition, Settings settings)
AnalyzerResult Analyze(FileInfo fileInfo)
AnalyzerResult Analyze(FileInfo fileInfo, Settings settings)

Task<AnalyzerResult> AnalyzeAsync(ScriptBlockAst scriptBlock, Token[] tokens, string fileName)
Task<AnalyzerResult> AnalyzeAsync(ScriptBlockAst scriptBlock, Token[] tokens, Settings settings, string fileName)
Task<AnalyzerResult> AnalyzeAsync(string scriptDefinition)
Task<AnalyzerResult> AnalyzeAsync(string scriptDefinition, Hashtable settings)
Task<AnalyzerResult> AnalyzeAsync(string scriptDefinition, Settings settings)
Task<AnalyzerResult> AnalyzeAsync(FileInfo fileInfo)
Task<AnalyzerResult> AnalyzeAsync(FileInfo fileInfo, Settings settings)

```

### Formatting and Fixing

The following synchronous and asynchronous APIs allow you to apply script fixes or formatting

```cs
string Fix(string scriptDefinition, Range range, Settings settings)
string Fix(string scriptDefinition, Range range)
string Fix(string scriptDefinition, Settings settings)
string Fix(string scriptDefinition)

Task<string> FixAsync(string scriptDefinition, Range range, Settings settings)
Task<string> FixAsync(string scriptDefinition, Range range)
Task<string> FixAsync(string scriptDefinition, Settings settings)
Task<string> FixAsync(string scriptDefinition)

string Format(string scriptDefinition, Range range, Settings settings)
string Format(string scriptDefinition, Range range)
string Format(string scriptDefinition, Settings settings)
string Format(string scriptDefinition)

Task<string> FormatAsync(string scriptDefinition, Range range, Settings settings)
Task<string> FormatAsync(string scriptDefinition, Range range)
Task<string> FormatAsync(string scriptDefinition, Settings settings)
Task<string> FormatAsync(string scriptDefinition)

```

#### Creating Settings

These enable the creation of settings which are to be used during analysis or script formatting and fixing

```cs
Settings CreateSettings()
Settings CreateSettings(string settings)
Settings CreateSettings(string[] settings)
Settings CreateSettings(Hashtable settings)
Settings CreateSettingsFromFile(string filename)
```

#### Retrieving available analysis rules

The following API provides the list of available rules.

```cs
List<RuleInfo> GetBuiltinRules(string[] ruleNames)
```

#### Results

When analysis is executed, an instance of `Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.AnalyzerResult` will be returned.
This includes the result from the analysis in the form of a collection of `DiagnosticRecord`s and the contents of the PowerShell streams which would normally be output by the cmdlet.
This object has the following properties:

* Result
  * This is the list of diagnostic records which were generated when the script was analyzed.
* Debug
  * A list of strings which would have been visible via a PowerShell Cmdlets use of the WriteDebug API
* Errors
  * A list of ErrorRecords which represent non-terminating errors generated during the analysis
* TerminatingErrors
  * A list of ErrorRecords which represent the terminating errors generated during analysis
* Verbose
  * A list of strings which would have been visible via a PowerShell Cmdlets use of the WriteVerbose API
* Warning
  * A list of strings which would have been visible via a PowerShell Cmdlets use of the WriteWarning API

## Examples

### Example 1

The simplest example taking advantage of this from PowerShell is as follows, which returns the issues found by script analyzer for the script `gci`.

```powershell
PS> $hostedAnalyzer = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer]::new()
PS> $result = $hostedAnalyzer.Analyze('gci')
PS> $result.Result

RuleName                            Severity     ScriptName Line  Message
--------                            --------     ---------- ----  -------
PSAvoidUsingCmdletAliases           Warning                 1     'gci' is an alias of 'Get-ChildItem'. Alias can introduce
                                                                  possible problems and make scripts hard to maintain. Please
                                                                  consider changing alias to its full content.
```

### Example 2

This example shows how to change settings to show only warnings.

```powershell
PS> $hostedAnalyzer = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer]::new()
PS> $hostedAnalyzer.Analyze('$a =').Result
RuleName                             Severity     ScriptName Line  Message
--------                             --------     ---------- ----  -------
ExpectedValueExpression              ParseError              1     You must provide a value expression following the '='
                                                                   operator.
PSUseDeclaredVarsMoreThanAssignments Warning                 1     The variable 'a' is assigned but never used.

PS> $settings = $hostedAnalyzer.CreateSettings()
PS> $settings.Severities.Add("Warning")
PS> $hostedAnalyzer.analyze('$a =', $settings).Result

RuleName                             Severity     ScriptName Line  Message
--------                             --------     ---------- ----  -------
PSUseDeclaredVarsMoreThanAssignments Warning                 1     The variable 'a' is assigned but never used.
```

### Example 3

This example shows how the analyzer can both format and fix a script using the CodeFormatting settings which is available in the Module.
Note, that you do not need to provide a path to the settings file as it can be found in the module settings directory.
This example creates a hosted analyzer instance, a multi-line script, and settings for code formatting.

```powershell
PS> $hostedAnalyzer = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer]::new()
PS> $codeFormattingSetting = $hostedAnalyzer.CreateSettings("CodeFormatting")
PS> $scriptDefinition = 'gci|%{
>> $_
>> }
>> '
PS> $hostedAnalyzer.Format($scriptDefinition, $codeFormattingSetting)
gci | % {
    $_
}

PS> $hostedAnalyzer.Format($hostedAnalyzer.Fix($scriptDefinition), $codeFormattingSetting)
Get-ChildItem | ForEach-Object {
    $_
}
```

### Example 4

This example shows how you can create a class which can then be used from other code, module, or script.

```cs
// Example.cs
using System;
using Microsoft.Windows.PowerShell.ScriptAnalyzer;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting;

namespace TestHostedAnalyzer
{
    public class TestAnalyzer
    {
        HostedAnalyzer hostedAnalyzer;
        public TestAnalyzer() {
            hostedAnalyzer = new HostedAnalyzer();
        }

        public AnalyzerResult AnalyzeScript(string scriptDefinition)
        {
            return hostedAnalyzer.Analyze(scriptDefinition);
        }

        public AnalyzerResult AnalyzeScript(string scriptDefinition, Settings settings) {
            return hostedAnalyzer.Analyze(scriptDefinition, settings);
        }

        public AnalyzerResult AnalyzeScriptForWarnings(string scriptDefinition) {
            Settings settings = hostedAnalyzer.CreateSettings();
            settings.Severities.Add("Warning");
            return AnalyzeScript(scriptDefinition, settings);
        }

        public string FixScript(string scriptDefinition) {
            return hostedAnalyzer.Fix(scriptDefinition);
        }

        public string FormatScript(string scriptDefinition) {
            return hostedAnalyzer.Format(scriptDefinition);
        }

        public string FormatAndFixScript(string scriptDefinition) {
            return hostedAnalyzer.Format(hostedAnalyzer.Fix(scriptDefinition));
        }
    }
}
```

```xml
<!-- example.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <RestoreSources>$(RestoreSources);../../Reference;/usr/local/share/PackageManagement/NuGet/Packages</RestoreSources>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="PowerShellStandard.Library" Version="5.1.1" />
    <PackageReference Include="Microsoft.Windows.PowerShell.ScriptAnalyzer" Version="1.18.4" />
  </ItemGroup>

</Project>
```

#### build and use

Note that fix alone does not reformat the script, but just changes the aliases into full cmdlet names

```powershell
PS> dotnet build
PS> Import-Module PSScriptAnalyzer
PS> Import-Module ./bin/Debug/netstandard2.0/example.dll
PS> $ta = [TestHostedAnalyzer.TestAnalyzer]::new()
PS> $ta.AnalyzeScript('gci').Result

RuleName                            Severity     ScriptName Line  Message
--------                            --------     ---------- ----  -------
PSAvoidUsingCmdletAliases           Warning                 1     'gci' is an alias of 'Get-ChildItem'. Alias can introduce
                                                                  possible problems and make scripts hard to maintain. Please
                                                                  consider changing alias to its full content.

PS> $ta.FixScript('gci|?{$_}')
Get-ChildItem|Where-Object{$_}

PS> $ta.FormatAndFixScript('gci|?{$_}')
Get-ChildItem | Where-Object { $_ }

PS> $ta.AnalyzeScript('$a =').Result

RuleName                             Severity     ScriptName Line  Message
--------                             --------     ---------- ----  -------
ExpectedValueExpression              ParseError              1     You must provide a value expression following the '='
                                                                   operator.
PSUseDeclaredVarsMoreThanAssignments Warning                 1     The variable 'a' is assigned but never used.

PS> $ta.AnalyzeScriptForWarnings('$a =').Result

RuleName                             Severity     ScriptName Line  Message
--------                             --------     ---------- ----  -------
PSUseDeclaredVarsMoreThanAssignments Warning                 1     The variable 'a' is assigned but never used.

```
