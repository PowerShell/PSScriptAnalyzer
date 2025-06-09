#
# Script module for module 'PSScriptAnalyzer'
#
Set-StrictMode -Version Latest

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# Import the appropriate nested binary module based on the current PowerShell version
$binaryModuleRoot = $PSModuleRoot
[Version] $minimumPowerShellCoreVersion = '7.4.7'
if ($PSVersionTable.PSVersion.Major -ge 6) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath "PSv$($PSVersionTable.PSVersion.Major)"
    # Minimum PowerShell Core version given by PowerShell Core support itself and
    # the version of Nuget references, such as e.g. Newtonsoft.Json inside PowerShell itself
    # or the version of the System.Management.Automation NuGet reference in PSSA.

    if ($PSVersionTable.PSVersion -lt $minimumPowerShellCoreVersion) {
        throw "Minimum supported version of PSScriptAnalyzer for PowerShell Core is $minimumPowerShellCoreVersion but current version is '$($PSVersionTable.PSVersion)'. Please update PowerShell Core."
    }
}
elseif ($PSVersionTable.PSVersion.Major -eq 5) {
    # Without this, PSSA tries to load this from $PSHome
    Add-Type -Path "$PSScriptRoot/Newtonsoft.Json.dll"
}
elseif ($PSVersionTable.PSVersion.Major -le 4) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath "PSv$($PSVersionTable.PSVersion.Major)"
    # Without this, PSSA tries to load this from $PSHome
    Add-Type -Path "$binaryModuleRoot/Newtonsoft.Json.dll"
}

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Microsoft.Windows.PowerShell.ScriptAnalyzer.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}

if (Get-Command Register-ArgumentCompleter -ErrorAction Ignore) {
    $settingPresetCompleter = {
        param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParmeter)

        [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings]::GetSettingPresets() | `
            Where-Object {$_ -like "$wordToComplete*"} | `
            ForEach-Object { New-Object System.Management.Automation.CompletionResult $_ }
    }

    @('Invoke-ScriptAnalyzer', 'Invoke-Formatter') | ForEach-Object {
        Register-ArgumentCompleter -CommandName $_ `
            -ParameterName 'Settings' `
            -ScriptBlock $settingPresetCompleter

    }

    Function RuleNameCompleter {
        param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParmeter)

        Get-ScriptAnalyzerRule *$wordToComplete* | `
            ForEach-Object { New-Object System.Management.Automation.CompletionResult $_.RuleName }
    }

    Register-ArgumentCompleter -CommandName 'Invoke-ScriptAnalyzer' -ParameterName 'IncludeRule' -ScriptBlock $Function:RuleNameCompleter
    Register-ArgumentCompleter -CommandName 'Invoke-ScriptAnalyzer' -ParameterName 'ExcludeRule' -ScriptBlock $Function:RuleNameCompleter
    Register-ArgumentCompleter -CommandName 'Get-ScriptAnalyzerRule' -ParameterName 'Name' -ScriptBlock $Function:RuleNameCompleter
}
