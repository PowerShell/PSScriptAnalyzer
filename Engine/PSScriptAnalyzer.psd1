#
# Module manifest for module 'PSScriptAnalyzer'
#

@{
 
# Author of this module
Author = 'Microsoft Corporation'

# Script module or binary module file associated with this manifest.
RootModule = 'PSScriptAnalyzer.psm1'

# Version number of this module.
ModuleVersion = '1.0'

# ID used to uniquely identify this module
GUID = '324fc715-36bf-4aee-8e58-72e9b4a08ad9'

# Company or vendor of this module
CompanyName = 'Microsoft Corporation'

# Copyright statement for this module
Copyright = '(c) Microsoft Corporation 2015. All rights reserved.'

# Description of the functionality provided by this module
Description = 'PSScriptAnalyzer provides script analysis and checks for potential code defects in the scripts by applying a group of builtin or customized rules on the scripts being analyzed.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @('ScriptAnalyzer.types.ps1xml')

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @('ScriptAnalyzer.format.ps1xml')

# Cmdlets to export from this module
CmdletsToExport = @('Get-ScriptAnalyzerRule','Invoke-ScriptAnalyzer')

# Private data to pass to the module specified in RootModule/ModuleToProcess
PrivateData = @{
    PSData = @{
        Tags = 'lint best practice'
        LicenseUri = 'https://github.com/PowerShell/PSScriptAnalyzer/blob/master/LICENSE'
        ProjectUri = 'https://github.com/PowerShell/PSScriptAnalyzer'
        IconUri = ''
        ReleaseNotes = ''
    }
}

# HelpInfo URI of this module
HelpInfoURI = 'http://go.microsoft.com/fwlink/?LinkId=525911'

}

