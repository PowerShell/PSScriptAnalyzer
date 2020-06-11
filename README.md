# PSScriptAnalyzer

<img src="logo.png" width="180">

[![Build Status](https://dev.azure.com/powershell/psscriptanalyzer/_apis/build/status/psscriptanalyzer-ci?branchName=master)](https://dev.azure.com/powershell/psscriptanalyzer/_build/latest?definitionId=80&branchName=master)
[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/master?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/master)
[![Join the chat at https://gitter.im/PowerShell/PSScriptAnalyzer](https://badges.gitter.im/PowerShell/PSScriptAnalyzer.svg)](https://gitter.im/PowerShell/PSScriptAnalyzer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Table of Contents
=================

<!-- toc -->

- [Introduction](#introduction)
- [Usage](#usage)
- [Installation](#installation)
    + [From PowerShell Gallery](#from-powershell-gallery)
      - [Supported PowerShell Versions and Platforms](#supported-powerShell-versions-and-platforms)
    + [From Source](#from-source)
      - [Requirements](#requirements)
      - [Steps](#steps)
      - [Tests](#tests)
    + [From Chocolatey](#from-chocolatey)
- [Suppressing Rules](#suppressing-rules)
- [Settings Support in ScriptAnalyzer](#settings-support-in-scriptanalyzer)
  * [Built-in Presets](#built-in-presets)
  * [Explicit](#explicit)
  * [Implicit](#implicit)
- [ScriptAnalyzer as a .NET library](#scriptanalyzer-as-a-net-library)
- [Violation Correction](#violation-correction)
- [Project Management Dashboard](#project-management-dashboard)
- [Contributions are welcome](#contributions-are-welcome)
- [Creating a Release](#creating-a-release)
- [Code of Conduct](#code-of-conduct)

<!-- tocstop -->

Introduction
============
PSScriptAnalyzer is a static code checker for PowerShell modules and scripts. PSScriptAnalyzer checks the quality of PowerShell code by running a set of rules.
The rules are based on PowerShell best practices identified by PowerShell Team and the community. It generates DiagnosticResults (errors and warnings) to inform users about potential
code defects and suggests possible solutions for improvements.

PSScriptAnalyzer is shipped with a collection of built-in rules that checks various aspects of PowerShell code such as presence of uninitialized variables, usage of PSCredential Type,
usage of Invoke-Expression etc. Additional functionalities such as exclude/include specific rules are also supported.

[Back to ToC](#table-of-contents)

Usage
======================

``` PowerShell
Get-ScriptAnalyzerRule [-CustomRulePath <String[]>] [-RecurseCustomRulePath] [-Name <String[]>] [-Severity <String[]>] [<CommonParameters>]

Invoke-ScriptAnalyzer [-Path] <String> [-CustomRulePath <String[]>] [-RecurseCustomRulePath] [-ExcludeRule <String[]>] [-IncludeDefaultRules] [-IncludeRule <String[]>] [-Severity <String[]>] [-Recurse] [-SuppressedOnly] [-Fix] [-EnableExit] [-ReportSummary] [-Settings <Object>] [-SaveDscDependency] [<CommonParameters>]

Invoke-ScriptAnalyzer [-ScriptDefinition] <String> [-CustomRulePath <String[]>] [-RecurseCustomRulePath] [-ExcludeRule <String[]>] [-IncludeDefaultRules] [-IncludeRule <String[]>] [-Severity <String[]>] [-Recurse] [-SuppressedOnly] [-EnableExit] [-ReportSummary] [-Settings <Object>] [-SaveDscDependency] [<CommonParameters>]

Invoke-Formatter [-ScriptDefinition] <String> [[-Settings] <Object>] [[-Range] <Int32[]>] [<CommonParameters>]
```

[Back to ToC](#table-of-contents)

Installation
============

### From PowerShell Gallery
```powershell
Install-Module -Name PSScriptAnalyzer
```

**Note**: For PowerShell version `5.1.14393.206` or newer, before installing PSScriptAnalyzer, please install the latest Nuget provider by running the following in an elevated PowerShell session.
```powershell
Install-PackageProvider Nuget -MinimumVersion 2.8.5.201 –Force
Exit
```

#### Supported PowerShell Versions and Platforms

- Windows PowerShell 3.0 or greater
- PowerShell Core 6.2 or greater on Windows/Linux/macOS
- Docker (tested only using Docker Desktop on Windows 10 1809)
  - PowerShell 6 Windows Image tags from [mcr.microsoft.com/powershell](https://hub.docker.com/r/microsoft/powershell). Example (1 warning gets produced by `Save-Module` but can be ignored):

    ```docker run -it mcr.microsoft.com/powershell:nanoserver pwsh -command "Save-Module -Name PSScriptAnalyzer -Path .; Import-Module .\PSScriptAnalyzer; Invoke-ScriptAnalyzer -ScriptDefinition 'gci'"```
  - PowerShell 5.1 (Windows): Only the [mcr.microsoft.com/windowsservercore](https://hub.docker.com/r/microsoft/windowsservercore/) images work but not the [microsoft/nanoserver](https://hub.docker.com/r/microsoft/windowsservercore/) images because they contain a Core version of it. Example:

    ```docker run -it mcr.microsoft.com/windowsservercore powershell -command "Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force; Install-Module PSScriptAnalyzer -Force; Invoke-ScriptAnalyzer -ScriptDefinition 'gci'"```
  - Linux tags from  [mcr.microsoft.com/powershell](https://hub.docker.com/r/microsoft/powershell/). - Example:

     ```docker run -it mcr.microsoft.com/powershell pwsh -c "Install-Module PSScriptAnalyzer -Force; Invoke-ScriptAnalyzer -ScriptDefinition 'gci'"```

### From Chocolatey

If you prefer to manage PSScriptAnalyzer as a Windows package, you can use [Chocolatey](https://chocolatey.org) to install it.

If you don't have Chocolatey, you can install it from the [Chocolately Install page](https://chocolatey.org/install).
With Chocolatey installed, execute the following command to install PSScriptAnalyzer:

```powershell
choco install psscriptanalyzer
```

Note: the PSScriptAnalyzer Chocolatey package is provided and supported by the community.

### From Source

#### Requirements

* [.NET Core 3.1.102 SDK](https://www.microsoft.com/net/download/dotnet-core/3.1#sdk-3.1.102) or newer patch release
* [Pester v5 PowerShell module, available on PowerShell Gallery](https://github.com/pester/Pester)
* [PlatyPS PowerShell module, available on PowerShell Gallery](https://github.com/PowerShell/platyPS/releases)
* Optionally but recommended for development: [Visual Studio 2017/2019](https://www.visualstudio.com/downloads/)

#### Steps
* Obtain the source
    - Download the latest source code from the [release page](https://github.com/PowerShell/PSScriptAnalyzer/releases) OR
    - Clone the repository (needs git)
    ```powershell
    git clone https://github.com/PowerShell/PSScriptAnalyzer
    ```
* Navigate to the source directory
    ```powershell
    cd path/to/PSScriptAnalyzer
    ```
* Building

    You can either build using the `Visual Studio` solution `PSScriptAnalyzer.sln` or build using `PowerShell` specifically for your platform as follows:
    * The default build is for the currently used version of PowerShell
    ```powershell
    .\build.ps1
    ```
    * Windows PowerShell version 5.0
    ```powershell
    .\build.ps1 -PSVersion 5
    ```
    * Windows PowerShell version 4.0
    ```powershell
    .\build.ps1 -PSVersion 4
    ```
    * Windows PowerShell version 3.0
    ```powershell
    .\build.ps1 -PSVersion 3
    ```
    * PowerShell Core
    ```powershell
    .\build.ps1 -PSVersion 6
    ```
* Rebuild documentation since it gets built automatically only the first time
    ```powershell
    .\build.ps1 -Documentation
    ```
* Build all versions (PowerShell v3, v4, v5, and v6) and documentation
    ```powershell
    .\build.ps1 -All
    ```
* Import the module
```powershell
Import-Module .\out\PSScriptAnalyzer\PSScriptAnalyzer.psd1
```

To confirm installation: run `Get-ScriptAnalyzerRule` in the PowerShell console to obtain the built-in rules

* Adding/Removing resource strings

For adding/removing resource strings in the `*.resx` files, it is recommended to use `Visual Studio` since it automatically updates the strongly typed `*.Designer.cs` files. The `Visual Studio 2017 Community Edition` is free to use but should you not have/want to use `Visual Studio` then you can either manually adapt the `*.Designer.cs` files or use the `New-StronglyTypedCsFileForResx.ps1` script although the latter is discouraged since it leads to a bad diff of the `*.Designer.cs` files.

#### Tests
Pester-based ScriptAnalyzer Tests are located in `path/to/PSScriptAnalyzer/Tests` folder.

* Ensure [Pester 4.3.1](https://www.powershellgallery.com/packages/Pester/4.3.1) or higher is installed
* In the root folder of your local repository, run:
``` PowerShell
./build -Test
```

To retrieve the results of the run, you can use the tools which are part of the build module (`build.psm1`)

```powershell
Import-Module ./build.psm1
Get-TestResults
```

To retrieve only the errors, you can use the following:

```powershell
Import-Module ./build.psm1
Get-TestFailures
```

[Back to ToC](#table-of-contents)

Parser Errors
=============

In prior versions of ScriptAnalyer, errors found during parsing were reported as errors and diagnostic records were not created.
ScriptAnalyzer now emits parser errors as diagnostic records in the output stream with other diagnostic records.

```powershell
PS> Invoke-ScriptAnalyzer -ScriptDefinition '"b" = "b"; function eliminate-file () { }'

RuleName            Severity   ScriptName Line Message
--------            --------   ---------- ---- -------
InvalidLeftHandSide ParseError            1    The assignment expression is not
                                               valid. The input to an
                                               assignment operator must be an
                                               object that is able to accept
                                               assignments, such as a variable
                                               or a property.
PSUseApprovedVerbs  Warning               1    The cmdlet 'eliminate-file' uses an
                                               unapproved verb.
```

The RuleName is set to the `ErrorId` of the parser error.

If ParseErrors would like to be suppressed, do not include it as a value in the `-Severity` parameter.

```powershell
PS> Invoke-ScriptAnalyzer -ScriptDefinition '"b" = "b"; function eliminate-file () { }' -Severity Warning

RuleName           Severity ScriptName Line Message
--------           -------- ---------- ---- -------
PSUseApprovedVerbs Warning             1    The cmdlet 'eliminate-file' uses an
                                            unapproved verb.
```




Suppressing Rules
=================

You can suppress a rule by decorating a script/function or script/function parameter with .NET's [SuppressMessageAttribute](https://docs.microsoft.com/dotnet/api/system.diagnostics.codeanalysis.suppressmessageattribute).
`SuppressMessageAttribute`'s constructor takes two parameters: a category and a check ID. Set the `categoryID` parameter to the name of the rule you want to suppress and set the `checkID` parameter to a null or empty string. You can optionally add a third named parameter with a justification for suppressing the message:

``` PowerShell
function SuppressMe()
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSProvideCommentHelp', '', Justification='Just an example')]
    param()

    Write-Verbose -Message "I'm making a difference!"

}
```

All rule violations within the scope of the script/function/parameter you decorate will be suppressed.

To suppress a message on a specific parameter, set the `SuppressMessageAttribute`'s `CheckId` parameter to the name of the parameter:
``` PowerShell
function SuppressTwoVariables()
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSProvideDefaultParameterValue', 'b')]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSProvideDefaultParameterValue', 'a')]
    param([string]$a, [int]$b)
    {
    }
}
```

Use the `SuppressMessageAttribute`'s `Scope` property to limit rule suppression to functions or classes within the attribute's scope.

Use the value `Function` to suppress violations on all functions within the attribute's scope. Use the value `Class` to suppress violations on all classes within the attribute's scope:

``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSProvideCommentHelp', '', Scope='Function')]
param(
)

function InternalFunction
{
    param()

    Write-Verbose -Message "I am invincible!"
}
```

You can further restrict suppression based on a function/parameter/class/variable/object's name by setting the `SuppressMessageAttribute's` `Target` property to a regular expression or a glob pattern. Few examples are given below.

Suppress `PSAvoidUsingWriteHost` rule violation in `start-bar` and `start-baz` but not in `start-foo` and `start-bam`:
``` PowerShell
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-ba[rz]')]
param()
function start-foo {
    write-host "start-foo"
}

function start-bar {
    write-host "start-bar"
}

function start-baz {
    write-host "start-baz"
}

function start-bam {
    write-host "start-bam"
}
```

Suppress violations in all the functions:
``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', Scope='Function', Target='*')]
Param()
```

Suppress violation in `start-bar`, `start-baz` and `start-bam` but not in `start-foo`:
``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', Scope='Function', Target='start-b*')]
Param()
```

**Note**: Parser Errors cannot be suppressed via the `SuppressMessageAttribute`

[Back to ToC](#table-of-contents)

Settings Support in ScriptAnalyzer
==================================
Settings that describe ScriptAnalyzer rules to include/exclude based on `Severity` can be created and supplied to
`Invoke-ScriptAnalyzer` using the `Setting` parameter. This enables a user to create a custom configuration for a specific environment. We support the following modes for specifying the settings file.

## Built-in Presets
ScriptAnalyzer ships a set of built-in presets that can be used to analyze scripts. For example, if the user wants to run *PowerShell Gallery* rules on their module, then they use the following command.

```powershell
PS> Invoke-ScriptAnalyzer -Path /path/to/module/ -Settings PSGallery -Recurse
```

Along with `PSGallery` there are a few other built-in presets, including, `DSC` and `CodeFormatting`, that can be used. These presets can be tab completed for the `Settings` parameter.

## Explicit

The following example excludes two rules from the default set of rules and any rule
that does not output an Error or Warning diagnostic record.

``` PowerShell
# PSScriptAnalyzerSettings.psd1
@{
    Severity=@('Error','Warning')
    ExcludeRules=@('PSAvoidUsingCmdletAliases',
                'PSAvoidUsingWriteHost')
}
```

Then invoke that settings file when using `Invoke-ScriptAnalyzer`:

``` PowerShell
Invoke-ScriptAnalyzer -Path MyScript.ps1 -Settings PSScriptAnalyzerSettings.psd1
```

The next example selects a few rules to execute instead of all the default rules.

``` PowerShell
# PSScriptAnalyzerSettings.psd1
@{
    IncludeRules=@('PSAvoidUsingPlainTextForPassword',
                'PSAvoidUsingConvertToSecureStringWithPlainText')
}
```

Then invoke that settings file:
``` PowerShell
Invoke-ScriptAnalyzer -Path MyScript.ps1 -Settings PSScriptAnalyzerSettings.psd1
```

## Implicit
If you place a PSScriptAnayzer settings file named `PSScriptAnalyzerSettings.psd1` in your project root, PSScriptAnalyzer will discover it if you pass the project root as the `Path` parameter.

```PowerShell
Invoke-ScriptAnalyzer -Path "C:\path\to\project" -Recurse
```

Note that providing settings explicitly takes higher precedence over this implicit mode. Sample settings files are provided [here](https://github.com/PowerShell/PSScriptAnalyzer/tree/master/Engine/Settings).

[Back to ToC](#table-of-contents)

ScriptAnalyzer as a .NET library
================================

ScriptAnalyzer engine and functionality can now be directly consumed as a library.

Here are the public interfaces:
``` c#
using Microsoft.Windows.PowerShell.ScriptAnalyzer;

public void Initialize(System.Management.Automation.Runspaces.Runspace runspace,
Microsoft.Windows.PowerShell.ScriptAnalyzer.IOutputWriter outputWriter,
[string[] customizedRulePath = null],
[string[] includeRuleNames = null],
[string[] excludeRuleNames = null],
[string[] severity = null],
[bool suppressedOnly = false],
[string profile = null])

public System.Collections.Generic.IEnumerable<DiagnosticRecord> AnalyzePath(string path,
    [bool searchRecursively = false])

public System.Collections.Generic.IEnumerable<IRule> GetRule(string[] moduleNames, string[] ruleNames)
```

[Back to ToC](#table-of-contents)

Violation Correction
====================

Some violations can be fixed by replacing the violation causing content with a suggested alternative. You can use the `-Fix` switch to automatically apply the suggestions. Since `Invoke-ScriptAnalyzer` implements `SupportsShouldProcess`, you can additionally use `-WhatIf` or `-Confirm` to find out which corrections would be applied. It goes without saying that you should use source control when applying those corrections since some some of them such as the one for `AvoidUsingPlainTextForPassword` might require additional script modifications that cannot be made automatically. Should your scripts be sensitive to encoding you should also check that because the initial encoding can not be preserved in all cases.

The initial motivation behind having the `SuggestedCorrections` property on the `ErrorRecord` (which is how the `-Fix` switch works under the hood) was to enable quick-fix like scenarios in editors like VSCode, Sublime, etc. At present, we provide valid `SuggestedCorrection` only for the following rules, while gradually adding this feature to more rules.

- AvoidAlias.cs
- AvoidUsingPlainTextForPassword.cs
- MisleadingBacktick.cs
- MissingModuleManifestField.cs
- UseToExportFieldsInManifest.cs

[Back to ToC](#table-of-contents)

Project Management Dashboard
============================
You can track issues, pull requests, backlog items here:

[![Stories in progress](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=In%20Progress&title=In%20Progress)](https://waffle.io/PowerShell/PSScriptAnalyzer)

[![Stories in ready](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=ready&title=Ready)](https://waffle.io/PowerShell/PSScriptAnalyzer)

[![Stories in backlog](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=BackLog&title=BackLog)](https://waffle.io/PowerShell/PSScriptAnalyzer)

Throughput Graph

[![Throughput Graph](https://graphs.waffle.io/powershell/psscriptanalyzer/throughput.svg)](https://waffle.io/powershell/psscriptanalyzer/metrics)

[Back to ToC](#table-of-contents)

Contributions are welcome
==============================

There are many ways to contribute:

1. Open a new bug report, feature request or just ask a question by opening a new issue [here]( https://github.com/PowerShell/PSScriptAnalyzer/issues/new/choose).
2. Participate in the discussions of [issues](https://github.com/PowerShell/PSScriptAnalyzer/issues), [pull requests](https://github.com/PowerShell/PSScriptAnalyzer/pulls) and verify/test fixes or new features.
3. Submit your own fixes or features as a pull request but please discuss it beforehand in an issue if the change is substantial.
4. Submit test cases.

[Back to ToC](#table-of-contents)

Creating a Release
================

- Update changelog (`changelog.md`) with the new version number and change set. When updating the changelog please follow the same pattern as that of previous change sets (otherwise this may break the next step).
- Import the ReleaseMaker module and execute `New-Release` cmdlet to perform the following actions.
  - Update module manifest (engine/PSScriptAnalyzer.psd1) with the new version number and change set
  - Update the version number in `Engine/Engine.csproj` and `Rules/Rules.csproj`
  - Create a release build in `out/`

```powershell
    PS> Import-Module .\Utils\ReleaseMaker.psm1
    PS> New-Release
```

- Sign the binaries and PowerShell files in the release build and publish the module to [PowerShell Gallery](www.powershellgallery.com).
- Draft a new release on github and tag `master` with the new version number.

[Back to ToC](#table-of-contents)

Code of Conduct
===============
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

[Back to ToC](#table-of-contents)
