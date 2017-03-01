#
# Module manifest for module 'PSScriptAnalyzer'
#

@{

# Author of this module
Author = 'Microsoft Corporation'

# Script module or binary module file associated with this manifest.
RootModule = 'PSScriptAnalyzer.psm1'

# Version number of this module.
ModuleVersion = '1.10.0'

# ID used to uniquely identify this module
GUID = 'd6245802-193d-4068-a631-8863a4342a18'

# Company or vendor of this module
CompanyName = 'Microsoft Corporation'

# Copyright statement for this module
Copyright = '(c) Microsoft Corporation 2016. All rights reserved.'

# Description of the functionality provided by this module
Description = 'PSScriptAnalyzer provides script analysis and checks for potential code defects in the scripts by applying a group of built-in or customized rules on the scripts being analyzed.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @('ScriptAnalyzer.types.ps1xml')

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @('ScriptAnalyzer.format.ps1xml')

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @('Get-ScriptAnalyzerRule','Invoke-ScriptAnalyzer')

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module
AliasesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess
PrivateData = @{
    PSData = @{
        Tags = 'lint', 'bestpractice'
        LicenseUri = 'https://github.com/PowerShell/PSScriptAnalyzer/blob/master/LICENSE'
        ProjectUri = 'https://github.com/PowerShell/PSScriptAnalyzer'
        IconUri = ''
        ReleaseNotes = @'
### Added
- Built-in settings presets to specify settings from command line (#717). Currently, PSSA ships with `PSGallery`, `CodeFormatting`, `DSC`, and other settings presets. All of them can be found in the `Settings/` directory in the module. To use them just pass them as an argument to the `Settings` parameter. For example, if you want to run rules that *powershellgallery* runs, then use the following command.
```powershell
PS> Invoke-ScriptAnalyzer -Path /path/to/your/module -Settings PSGallery
```
- Argument completion for built-in settings presets (#717).
- Argument completion for `IncludeRule` and `ExcludeRule` parameters (#717).
- Option to `PSCloseBrace` rule to add new line after the brace (#713).
- Option to `PSCloseBrace` rule to ignore expressions that have open and close braces on the same line (#706).
- New rule, PSUseConsistentWhitespace, to check for whitespace style around operators and separators (#702).

### Fixed
- Indentation when pipes precede new lines in a multi-line command expression in `PSUseConsistentIdentation` rule (#705).
- Handling of SubExpressionAsts (`$(...)`) in `PSUseConsistentIdentation` rule (#700).
- Performance issues caused by `get-command` cmdlet (#695).

### Changed
- Settings implementation to decouple it from engine (#717).
'@
    }
}

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}




