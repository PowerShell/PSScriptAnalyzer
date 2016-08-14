[![Join the chat at https://gitter.im/PowerShell/PSScriptAnalyzer](https://badges.gitter.im/PowerShell/PSScriptAnalyzer.svg)](https://gitter.im/PowerShell/PSScriptAnalyzer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Announcements
=============
###### [Get the latest PSScriptAnalyzer version from PowerShellGallery](https://www.powershellgallery.com/packages/PSScriptAnalyzer)

###### [VSCode-PowerShell has built-in ScriptAnalyzer support](https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell)

###### [ISE-Steroids has ScriptAnalyzer integration](http://www.powertheshell.com/psscriptanalyzer-integration-and-more/)

###### [ISE Add-On for ScriptAnalyzer is available in PowerShellGallery](https://www.powershellgallery.com/packages/ISEScriptAnalyzerAddOn/)

=============
##### ScriptAnalyzer community meeting schedule:
 - Next Meeting - June 2, 2016 - 11am to 12pm PDT
    - [Meeting Agenda](https://github.com/PowerShell/PSScriptAnalyzer/wiki/Community-Meeting-Agenda)
 - [Notes and recordings from earlier meetings](https://github.com/PowerShell/PSScriptAnalyzer/wiki)

=============
#####Builds
|Master   |  Development |
|:------:|:------:|:-------:|:-------:|
[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/master?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/master)|[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/development?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/development) |

=============
#### Code Review Dashboard on [reviewable.io](https://reviewable.io/reviews/PowerShell/PSScriptAnalyzer#-)
=============

Introduction
============
PSScriptAnalyzer is a static code checker for Windows PowerShell modules and scripts. PSScriptAnalyzer checks the quality of Windows PowerShell code by running a set of rules. 
The rules are based on PowerShell best practices identified by PowerShell Team and the community. It generates DiagnosticResults (errors and warnings) to inform users about potential 
code defects and suggests possible solutions for improvements.

PSScriptAnalyzer is shipped with a collection of built-in rules that checks various aspects of PowerShell code such as presence of uninitialized variables, usage of PSCredential Type, 
usage of Invoke-Expression etc. Additional functionalities such as exclude/include specific rules are also supported.

PSScriptAnalyzer CMDLets
======================
``` PowerShell
Get-ScriptAnalyzerRule [-CustomizedRulePath <string[]>] [-Name <string[]>] [<CommonParameters>] [-Severity <string[]>]

Invoke-ScriptAnalyzer [-Path] <string> [-CustomizedRulePath <string[]>] [-ExcludeRule <string[]>] [-IncludeRule <string[]>] [-Severity <string[]>] [-Recurse] [<CommonParameters>]
```

Requirements
============
Windows Server 2012 R2 / Windows 8.1 / Windows OS running a **minimum of PowerShell v3.0**

Installation
============
1. Build the Code using Visual Studio [solution part of the repository] and navigate to the binplace location [``~/ProjectRoot/PSScriptAnalyzer``]
2. In PowerShell Console:
``` PowerShell
Import-Module PSScriptAnalyzer
```
If you have previous version of PSScriptAnalyzer installed on your machine, you may need to override old binaries by copying content of [``~/ProjectRoot/PSScriptAnalyzer``] to PSModulePath.

To confirm installation: run ```Get-ScriptAnalyzerRule``` in the PowerShell console to obtain the built-in rules


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

The above example demonstrates how to suppress rule violations for internal functions using the `SuppressMessageAttribute`'s `Scope` property.

You can further restrict suppression based on a function/parameter/class/variable/object's name by setting the `SuppressMessageAttribute's` `Target` property to a regular expression. Any function/parameter/class/variable/object whose name matches the regular expression is skipped.

``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", Scope="Function", Target="PositionalParametersAllowed")]
Param(
)

function PositionalParametersAllowed()
{
    Param([string]$Parameter1)
    {
        Write-Verbose $Parameter1
    }

}

function PositionalParametersNotAllowed()
{
    param([string]$Parameter1)
    {
        Write-Verbose $Parameter1
    }
}

# The script analyzer will skip this violation
PositionalParametersAllowed 'value1'

# The script analyzer will report this violation
PositionalParametersNotAllowed 'value1
```

To match all functions/variables/parameters/objects, use `*` as the value of the Target parameter:
``` PowerShell
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPositionalParameters", Scope="Function", Target="*")]
Param(
)
```

**Note**: Rule suppression is currently supported only for built-in rules. 

Settings Support in ScriptAnalyzer
========================================
Settings that describe ScriptAnalyzer rules to include/exclude based on ```Severity``` can be created and supplied to
```Invoke-ScriptAnalyzer``` using the ```Setting``` parameter. This enables a user to create a custom configuration for a specific environment.

Using Settings support:

The following example excludes two rules from the default set of rules and any rule
that does not output an Error or Warning diagnostic record.

``` PowerShell
# ScriptAnalyzerSettings.psd1
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
# ScriptAnalyzerSettings.psd1
@{
    IncludeRules=@('PSAvoidUsingPlainTextForPassword',
                'PSAvoidUsingConvertToSecureStringWithPlainText')
}
```

Then invoke that settings file when using:
``` PowerShell
Invoke-ScriptAnalyzer -Path MyScript.ps1 -Setting ScriptAnalyzerSettings.psd1
```

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

Violation Correction
====================
Most violations can be fixed by replacing the violation causing content with the correct alternative. 

In an attempt to provide the user with the ability to correct the violation we provide a property, ```SuggestedCorrections```, in each DiagnosticRecord instance,
that contains information needed to rectify the violation.

For example, consider a script ```C:\tmp\test.ps1``` with the following content:
``` PowerShell
PS> Get-Content C:\tmp\test.ps1
gci C:\
```

Invoking PSScriptAnalyzer on the file gives the following output:
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

The ```*LineNumber``` and ```*ColumnNumber``` properties give the region of the script that can be replaced by the contents of ```Text``` property, i.e., replace gci with Get-ChildItem.

The main motivation behind having ```SuggestedCorrections``` is to enable quick-fix like scenarios in editors like VSCode, Sublime, etc. At present, we provide valid ```SuggestedCorrection``` only for the following rules, 
while gradually adding this feature to more rules. 

* AvoidAlias.cs 
* AvoidUsingPlainTextForPassword.cs
* MisleadingBacktick.cs
* MissingModuleManifestField.cs
* UseToExportFieldsInManifest.cs

Building the Code
=================

Use Visual Studio to build "PSScriptAnalyzer.sln". Use ~/PSScriptAnalyzer/ folder to load PSScriptAnalyzer.psd1

**Note: If there are any build errors, please refer to Requirements section and make sure all dependencies are properly installed**


Running Tests
=============

Pester-based ScriptAnalyzer Tests are located in ```<branch>/PSScriptAnalyzer/Tests``` folder.

1. Ensure Pester is installed on the machine
2. Go the Tests folder in your local repository
3. Run Engine Tests:
``` PowerShell
.\InvokeScriptAnalyzer.tests.ps1
```
4. Run Tests for Built-in rules:
``` PowerShell
.\*.ps1 (Example - .\ AvoidConvertToSecureStringWithPlainText.ps1)
```
You can also run all tests under \Engine or \Rules by calling Invoke-Pester in the Engine/Rules directory.

Project Management Dashboard
==============================
You can track issues, pull requests, backlog items here:

[![Stories in progress](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=In%20Progress&title=In%20Progress)](https://waffle.io/PowerShell/PSScriptAnalyzer)

[![Stories in ready](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=ready&title=Ready)](https://waffle.io/PowerShell/PSScriptAnalyzer)

[![Stories in backlog](https://badge.waffle.io/PowerShell/PSScriptAnalyzer.png?label=BackLog&title=BackLog)](https://waffle.io/PowerShell/PSScriptAnalyzer)

Throughput Graph

[![Throughput Graph](https://graphs.waffle.io/powershell/psscriptanalyzer/throughput.svg)](https://waffle.io/powershell/psscriptanalyzer/metrics)

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
