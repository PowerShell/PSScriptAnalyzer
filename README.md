[![Join the chat at https://gitter.im/PowerShell/PSScriptAnalyzer](https://badges.gitter.im/PowerShell/PSScriptAnalyzer.svg)](https://gitter.im/PowerShell/PSScriptAnalyzer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

|Master   |  Development |
|:------:|:------:|
|[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/master?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/master)|[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/development?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/development) |

Table of Contents
=================

<!-- toc -->

- [Introduction](#introduction)
- [Usage](#usage)
- [Installation](#installation)
    + [From PowerShell Gallery](#from-powershell-gallery)
      - [Requirements](#requirements)
        * [Windows](#windows)
        * [Linux (*Tested only on Ubuntu 14.04*)](#linux-tested-only-on-ubuntu-1404)
    + [From Source](#from-source)
      - [Requirements](#requirements-1)
      - [Steps](#steps)
      - [Tests](#tests)
- [Suppressing Rules](#suppressing-rules)
- [Settings Support in ScriptAnalyzer](#settings-support-in-scriptanalyzer)
  * [Built-in Presets](#built-in-presets)
  * [Explicit](#explicit)
  * [Implicit](#implicit)
- [ScriptAnalyzer as a .NET library](#scriptanalyzer-as-a-net-library)
- [Violation Correction](#violation-correction)
- [Project Management Dashboard](#project-management-dashboard)
- [Contributing to ScriptAnalyzer](#contributing-to-scriptanalyzer)
- [Code of Conduct](#code-of-conduct)

<!-- tocstop -->

Introduction
============
PSScriptAnalyzer is a static code checker for Windows PowerShell modules and scripts. PSScriptAnalyzer checks the quality of Windows PowerShell code by running a set of rules.
The rules are based on PowerShell best practices identified by PowerShell Team and the community. It generates DiagnosticResults (errors and warnings) to inform users about potential
code defects and suggests possible solutions for improvements.

PSScriptAnalyzer is shipped with a collection of built-in rules that checks various aspects of PowerShell code such as presence of uninitialized variables, usage of PSCredential Type,
usage of Invoke-Expression etc. Additional functionalities such as exclude/include specific rules are also supported.

[Back to ToC](#table-of-contents)

Usage
======================

``` PowerShell
Get-ScriptAnalyzerRule [-CustomizedRulePath <string[]>] [-Name <string[]>] [<CommonParameters>] [-Severity <string[]>]

Invoke-ScriptAnalyzer [-Path] <string> [-CustomizedRulePath <string[]>] [-ExcludeRule <string[]>] [-IncludeRule <string[]>] [-Severity <string[]>] [-Recurse] [<CommonParameters>]
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
Install-PackageProvider Nuget –force –verbose
Exit
```

#### Requirements

##### Windows
- Windows PowerShell 3.0 or greater
- PowerShell Core

##### Linux (*Tested only on Ubuntu 14.04*)
- PowerShell Core

### From Source

#### Requirements
* [.NET Core 1.1.5 SDK](https://github.com/dotnet/core/blob/master/release-notes/download-archives/1.1.5.md)
* [PlatyPS 0.5.0 or greater](https://github.com/PowerShell/platyPS)
* Optionally but recommended for development: [Visual Studio 2017](https://www.visualstudio.com/downloads/)

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
    * Windows PowerShell version 5.0 and greater
    ```powershell
    .\buildCoreClr.ps1 -Framework net451 -Configuration Release -Build
    ```
    * Windows PowerShell version 3.0 and 4.0
    ```powershell
    .\buildCoreClr.ps1 -Framework net451 -Configuration PSV3Release -Build
    ```
    * PowerShell Core
    ```powershell
    .\buildCoreClr.ps1 -Framework netstandard1.6 -Configuration Release -Build
    ```
* Build documenatation
    ```powershell
    .\build.ps1 -BuildDocs
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

* Ensure Pester 3.4 is installed on the machine
* Copy `path/to/PSScriptAnalyzer/out/PSScriptAnalyzer` to a folder in `PSModulePath`
* Go the Tests folder in your local repository
* Run Engine Tests:
``` PowerShell
cd /path/to/PSScriptAnalyzer/Tests/Engine
Invoke-Pester
```
* Run Tests for Built-in rules:
``` PowerShell
cd /path/to/PSScriptAnalyzer/Tests/Rules
Invoke-Pester
```

[Back to ToC](#table-of-contents)

Suppressing Rules
=================

You can suppress a rule by decorating a script/function or script/function parameter with .NET's [SuppressMessageAttribute](https://msdn.microsoft.com/en-us/library/system.diagnostics.codeanalysis.suppressmessageattribute.aspx).
`SuppressMessageAttribute`'s constructor takes two parameters: a category and a check ID. Set the `categoryID` parameter to the name of the rule you want to suppress and set the `checkID` parameter to a null or empty string:

``` PowerShell
function SuppressMe()
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "")]
    param()

    Write-Verbose -Message "I'm making a difference!"

}
```

All rule violations within the scope of the script/function/parameter you decorate will be suppressed.

To suppress a message on a specific parameter, set the `SuppressMessageAttribute`'s `CheckId` parameter to the name of the parameter:
``` PowerShell
function SuppressTwoVariables()
{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideDefaultParameterValue", "b")]
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideDefaultParameterValue", "a")]
    param([string]$a, [int]$b)
    {
    }
}
```

Use the `SuppressMessageAttribute`'s `Scope` property to limit rule suppression to functions or classes within the attribute's scope.

Use the value `Function` to suppress violations on all functions within the attribute's scope. Use the value `Class` to suppress violations on all classes within the attribute's scope:

``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSProvideCommentHelp", "", Scope="Function")]
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
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", Scope="Function", Target="*")]
Param()
```

Suppress violation in `start-bar`, `start-baz` and `start-bam` but not in `start-foo`:
``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", Scope="Function", Target="start-b*")]
Param()
```

**Note**: Rule suppression is currently supported only for built-in rules.

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
Invoke-ScriptAnalyzer -Path MyScript.ps1 -Setting ScriptAnalyzerSettings.psd1
```

The next example selects a few rules to execute instead of all the default rules.

``` PowerShell
# PSScriptAnalyzerSettings.psd1
@{
    IncludeRules=@('PSAvoidUsingPlainTextForPassword',
                'PSAvoidUsingConvertToSecureStringWithPlainText')
}
```

Then invoke that settings file when using:
``` PowerShell
Invoke-ScriptAnalyzer -Path MyScript.ps1 -Setting ScriptAnalyzerSettings.psd1
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
Most violations can be fixed by replacing the violation causing content with the correct alternative.

In an attempt to provide the user with the ability to correct the violation we provide a property, `SuggestedCorrections`, in each DiagnosticRecord instance,
that contains information needed to rectify the violation.

For example, consider a script `C:\tmp\test.ps1` with the following content:
``` PowerShell
PS> Get-Content C:\tmp\test.ps1
gci C:\
```

Invoking PSScriptAnalyzer on the file gives the following output.
``` PowerShell
PS>$diagnosticRecord = Invoke-ScriptAnalyzer -Path C:\tmp\test.p1
PS>$diagnosticRecord | select SuggestedCorrections | Format-Custom

class DiagnosticRecord
{
  SuggestedCorrections =
    [
    class CorrectionExtent
    {
        EndColumnNumber = 4
        EndLineNumber = 1
        File = C:\Users\kabawany\tmp\test3.ps1
        StartColumnNumber = 1
        StartLineNumber = 1
        Text = Get-ChildItem
        Description = Replace gci with Get-ChildItem
    }
  ]
}
```

The `*LineNumber` and `*ColumnNumber` properties give the region of the script that can be replaced by the contents of `Text` property, i.e., replace gci with Get-ChildItem.

The main motivation behind having `SuggestedCorrections` is to enable quick-fix like scenarios in editors like VSCode, Sublime, etc. At present, we provide valid `SuggestedCorrection` only for the following rules, while gradually adding this feature to more rules.

* AvoidAlias.cs
* AvoidUsingPlainTextForPassword.cs
* MisleadingBacktick.cs
* MissingModuleManifestField.cs
* UseToExportFieldsInManifest.cs

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

Contributing to ScriptAnalyzer
==============================
You are welcome to contribute to this project. There are many ways to contribute:

1. Submit a bug report via [Issues]( https://github.com/PowerShell/PSScriptAnalyzer/issues). For a guide to submitting good bug reports, please read [Painless Bug Tracking](http://www.joelonsoftware.com/articles/fog0000000029.html).
2. Verify fixes for bugs.
3. Submit your fixes for a bug. Before submitting, please make sure you have:
  * Performed code reviews of your own
  * Updated the test cases if needed
  * Run the test cases to ensure no feature breaks or test breaks
  * Added the test cases for new code
4. Submit a feature request.
5. Help answer questions in the discussions list.
6. Submit test cases.
7. Tell others about the project.
8. Tell the developers how much you appreciate the product!

You might also read these two blog posts about contributing code: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza, and [Don’t “Push” Your Pull Requests](http://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik.

Before submitting a feature or substantial code contribution, please discuss it with the Windows PowerShell team via [Issues](https://github.com/PowerShell/PSScriptAnalyzer/issues), and ensure it follows the product roadmap. Note that all code submissions will be rigorously reviewed by the Windows PowerShell Team. Only those that meet a high bar for both quality and roadmap fit will be merged into the source.

[Back to ToC](#table-of-contents)

Code of Conduct
===============
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

[Back to ToC](#table-of-contents)