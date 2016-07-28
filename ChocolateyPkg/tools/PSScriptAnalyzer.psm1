#
# Script module for module 'PSScriptAnalyzer'
#

# Clear PSDefaultParameterValues in the module scope and enable strict mode
$PSDefaultParameterValues.Clear()
Set-StrictMode -Version Latest

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# Import the appropriate nested binary module based on the current PowerShell version
$binaryModuleRoot = $PSModuleRoot
if ($PSVersionTable.PSVersion -lt [Version]'5.0') {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'PSv3'
}

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Microsoft.Windows.PowerShell.ScriptAnalyzer.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}