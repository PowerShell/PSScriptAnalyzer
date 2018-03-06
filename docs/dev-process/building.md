# Building and Testing ScriptAnalyzer

### From Source

* [.NET Core 2.1.4 SDK](https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.0.5-download.md)
* [PlatyPS 0.9.0 or greater](https://github.com/PowerShell/platyPS/releases)
* Recommended for development: [Visual Studio Code](https://code.visualstudio.com/download)
* Optionally for development: [Visual Studio](https://www.visualstudio.com/downloads/)

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
Import-Module .\out\PSScriptAnalyzer
```

To confirm installation: run `Get-ScriptAnalyzerRule` in the PowerShell console to obtain the built-in rules

### Resource changes
See [Resources](resx-files.md)

### Testing

Pester-based ScriptAnalyzer Tests are located in `path/to/PSScriptAnalyzer/Tests` folder.

* Ensure [Pester 4.3.1](https://www.powershellgallery.com/packages/Pester/4.3.1) (or later) is installed
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
``
