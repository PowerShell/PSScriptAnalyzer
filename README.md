# PSScriptAnalyzer

<img src="logo.png" width="180">

[![Build Status](https://dev.azure.com/powershell/psscriptanalyzer/_apis/build/status/psscriptanalyzer-ci?branchName=master)](https://dev.azure.com/powershell/psscriptanalyzer/_build/latest?definitionId=80&branchName=master)
[![Build status](https://ci.appveyor.com/api/projects/status/h5mot3vqtvxw5d7l/branch/master?svg=true)](https://ci.appveyor.com/project/PowerShell/psscriptanalyzer/branch/master)
[![Join the chat at https://gitter.im/PowerShell/PSScriptAnalyzer](https://badges.gitter.im/PowerShell/PSScriptAnalyzer.svg)](https://gitter.im/PowerShell/PSScriptAnalyzer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Table of Contents

<!-- toc -->

- [Introduction](#introduction)
- [Documentation Notice](#documentation-notice)
- [Installation](#installation)
- [Contributions are welcome](#contributions-are-welcome)
- [Creating a Release](#creating-a-release)
- [Code of Conduct](#code-of-conduct)

<!-- tocstop -->

## Introduction

PSScriptAnalyzer is a static code checker for PowerShell modules and scripts. PSScriptAnalyzer
checks the quality of PowerShell code by running a [set of rules](docs/Rules). The rules are based
on PowerShell best practices identified by PowerShell Team and the community. It generates
DiagnosticResults (errors and warnings) to inform users about potential code defects and suggests
possible solutions for improvements.

PSScriptAnalyzer is shipped with a collection of built-in rules that checks various aspects of
PowerShell code such as presence of uninitialized variables, usage of PSCredential Type, usage of
Invoke-Expression etc. Additional functionalities such as exclude/include specific rules are also
supported.

[Back to ToC](#table-of-contents)

## DOCUMENTATION NOTICE

Conceptual user documentation has been moved out of the source code repository and into the
documentation repository so that it can be published on docs.microsoft.com.

The goal of this migration is to have the user documentation on docs.microsoft.com. The source code
repository should only contain documentation for the code base, such as how to build the code or how
to contribute to the code.

User documentation that has been migrated:

- Most of the contents of this README can be found in the
  [PSScriptAnalyzer overview](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/overview)
- For cmdlet reference, see
  [PSScriptAnalyzer](https://docs.microsoft.com/powershell/module/psscriptanalyzer)
- For rules, see
  [Rules overview](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/rules/readme)
- The `PowerShellBestPractices.md` content has been moved to
  [PSScriptAnalyzer rules and recommendations](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/rules-recommendations)
- The `ScriptRuleDocumentation.md` content has been moved to
  [Creating custom rules](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/create-custom-rule)

There is one exception - the documentation for the rules and cmdlets will remain in the [docs](docs)
folder to facilitate build testing and to be archived as part of each release. Only the
documentation for the latest release is published on on docs.microsoft.com.

## Installation

To install **PSScriptAnalyzer** from the PowerShell Gallery, see
[Installing PSScriptAnalyzer](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/overview#installing-psscriptanalyzer).

To install **PSScriptAnalyzer** from source code:

### Requirements

- [.NET Core 3.1.102 SDK](https://www.microsoft.com/net/download/dotnet-core/3.1#sdk-3.1.102) or
  newer patch release
* If building for Windows PowerShell versions, then the .NET Framework 4.6.2 [targeting pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462) (also referred to as developer/targeting pack) need to be installed. This is only possible on Windows.
* Optionally but recommended for development: [Visual Studio 2017/2019](https://www.visualstudio.com/downloads)
- [Pester v5 PowerShell module, available on PowerShell Gallery](https://github.com/pester/Pester)
- [PlatyPS PowerShell module, available on PowerShell Gallery](https://github.com/PowerShell/platyPS/releases)
- Optionally but recommended for development: [Visual Studio](https://www.visualstudio.com/downloads)

### Steps

- Obtain the source
  - Download the latest source code from the
    [release page](https://github.com/PowerShell/PSScriptAnalyzer/releases) OR
  - Clone the repository (needs git)

    ```powershell
    git clone https://github.com/PowerShell/PSScriptAnalyzer
    ```

- Navigate to the source directory

  ```powershell
  cd path/to/PSScriptAnalyzer
  ```

- Building You can either build using the `Visual Studio` solution `PSScriptAnalyzer.sln` or build
  using `PowerShell` specifically for your platform as follows:
  - The default build is for the currently used version of PowerShell

    ```powershell
    .\build.ps1
    ```

  - Windows PowerShell version 5.0

    ```powershell
    .\build.ps1 -PSVersion 5
    ```

  - Windows PowerShell version 4.0

    ```powershell
    .\build.ps1 -PSVersion 4
    ```

  - Windows PowerShell version 3.0

    ```powershell
    .\build.ps1 -PSVersion 3
    ```

  - PowerShell 7

    ```powershell
    .\build.ps1 -PSVersion 7
    ```

- Rebuild documentation since it gets built automatically only the first time

  ```powershell
  .\build.ps1 -Documentation
  ```

- Build all versions (PowerShell v3, v4, v5, and v6) and documentation

  ```powershell
  .\build.ps1 -All
  ```

- Import the module

  ```powershell
  Import-Module .\out\PSScriptAnalyzer\PSScriptAnalyzer.psd1
  ```

To confirm installation: run `Get-ScriptAnalyzerRule` in the PowerShell console to obtain the
built-in rules.

- Adding/Removing resource strings

  For adding/removing resource strings in the `*.resx` files, it is recommended to use
  `Visual Studio` since it automatically updates the strongly typed `*.Designer.cs` files. The
  `Visual Studio 2017 Community Edition` is free to use but should you not have/want to use
  `Visual Studio` then you can either manually adapt the `*.Designer.cs` files or use the
  `New-StronglyTypedCsFileForResx.ps1` script although the latter is discouraged since it leads to a
  bad diff of the `*.Designer.cs` files.

### Tests

Pester-based ScriptAnalyzer Tests are located in `path/to/PSScriptAnalyzer/Tests` folder.

- Ensure [Pester 4.3.1](https://www.powershellgallery.com/packages/Pester/4.3.1) or higher is
  installed
- In the root folder of your local repository, run:

```powershell
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

## Using PSScriptAnalyzer

The documentation in this section can be found in
[Using PSScriptAnalyzer](https://docs.microsoft.com/powershell/utility-modules/psscriptanalyzer/using-scriptanalyzer).

## Contributions are welcome

There are many ways to contribute:

1. Open a new bug report, feature request or just ask a question by opening a
   [new issue](https://github.com/PowerShell/PSScriptAnalyzer/issues/new/choose).
2. Participate in the discussions of
   [issues](https://github.com/PowerShell/PSScriptAnalyzer/issues),
   [pull requests](https://github.com/PowerShell/PSScriptAnalyzer/pulls) and test fixes or new
   features.
3. Submit your own fixes or features as a pull request but please discuss it beforehand in an issue.
4. Submit test cases.

[Back to ToC](#table-of-contents)

## Creating a Release

- Update changelog (`changelog.md`) with the new version number and change set. When updating the
  changelog please follow the same pattern as that of previous change sets (otherwise this may break
  the next step).
- Import the ReleaseMaker module and execute `New-Release` cmdlet to perform the following actions.
  - Update module manifest (engine/PSScriptAnalyzer.psd1) with the new version number and change set
  - Update the version number in `Engine/Engine.csproj` and `Rules/Rules.csproj`
  - Create a release build in `out/`

```powershell
Import-Module .\Utils\ReleaseMaker.psm1
New-Release
```

- Sign the binaries and PowerShell files in the release build and publish the module to
  [PowerShell Gallery](https://www.powershellgallery.com).
- Draft a new release on github and tag `master` with the new version number.

[Back to ToC](#table-of-contents)

## Code of Conduct

This project has adopted the
[Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more
information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or
comments.

[Back to ToC](#table-of-contents)
