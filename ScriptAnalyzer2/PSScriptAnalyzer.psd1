#
# Module manifest for module 'PSScriptAnalyzer'
#

@{

# Author of this module
Author = 'Microsoft Corporation'

# Script module or binary module file associated with this manifest.
RootModule = if ($PSEdition -eq 'Core')
    {
        'netcoreapp3.1/Microsoft.PowerShell.ScriptAnalyzer.dll'
    }
    else
    {
        'net452/Microsoft.PowerShell.ScriptAnalyzer.dll'
    }

# Version number of this module.
ModuleVersion = '2.0.0'

# ID used to uniquely identify this module
GUID = 'd6245802-193d-4068-a631-8863a4342a18'

# Company or vendor of this module
CompanyName = 'Microsoft Corporation'

# Copyright statement for this module
Copyright = '(c) Microsoft Corporation'

# Description of the functionality provided by this module
Description = 'PSScriptAnalyzer is a static analyzer and formatter for PowerShell, checking for potential code defects in the scripts by applying a group of built-in or customized rules to analyzed scripts.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Minimum version of Microsoft .NET Framework required by this module
DotNetFrameworkVersion = '4.6.1'

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @()

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @('Get-ScriptAnalyzerRule', 'Invoke-ScriptAnalyzer', 'Write-Diagnostic')

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module
AliasesToExport = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess
PrivateData = @{
    PSData = @{
        Tags = 'lint', 'bestpractice'
        LicenseUri = 'https://github.com/PowerShell/PSScriptAnalyzer/blob/master/LICENSE'
        ProjectUri = 'https://github.com/PowerShell/PSScriptAnalyzer'
        IconUri = 'https://raw.githubusercontent.com/powershell/psscriptanalyzer/master/logo.png'
        ReleaseNotes = ''
        Prerelease = 'preview.1'
    }
}

CompatiblePSEditions = @('Core', 'Desktop')

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
