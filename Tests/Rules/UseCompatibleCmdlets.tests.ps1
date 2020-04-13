# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$testCases = @(
    # Microsoft.PowerShell.Core
    @{
        Command = 'Enter-PSSession'
    }
    @{
        Command = 'Foreach-Object'
    }
    @{
        Command = 'Get-Command'
    }
    # Non-builtin commands
    @{
        Command = 'get-foo'
    }
    @{
        Command = 'get-bar'
    }
    @{
        Command = 'get-baz'
    }
    # Aliases
    @{
        Command = 'where'
    }
    @{
        Command = 'select'
    }
    @{
        Command = 'cd'
    }
    # Commands present in reference platform but not in target platform
    @{
        Command              = 'Start-VM'
        ExpectedOneViolation = $true
    }
    @{
        Command              = 'New-SmbShare'
        ExpectedOneViolation = $true
    }
    @{
        Command              = 'Get-Disk'
        ExpectedOneViolation = $true
    }
    # Default reference can also be used as target platform
    @{
        Command              = 'Remove-Service'
        ExpectedOneViolation = $true
        Settings             = @{ rules = @{ PSUseCompatibleCmdlets = @{ compatibility = @('desktop-5.1.14393.206-windows') } } }
    }
)

Describe "UseCompatibleCmdlets" {
    BeforeAll {
        $ruleName = 'PSUseCompatibleCmdlets'
        $settings = @{ rules = @{ PSUseCompatibleCmdlets = @{ compatibility = @('core-6.1.0-windows') } } }

        Function Test-Command
        {
            Param (
                [string] $Command,
                [hashtable] $Settings,
                [switch] $ExpectedOneViolation
            )

            $ruleName = 'PSUseCompatibleCmdlets'
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition $Command -IncludeRule $ruleName -Settings $Settings
            if ($ExpectedOneViolation.IsPresent) {
                $warnings.Count | Should -Be 1
            }
            else {
                $warnings.Count | Should -Be 0
            }
            $warnings | ForEach-Object {
                $_.RuleName | Should -Be $ruleName
            }
        }

        $settings = @{rules = @{PSUseCompatibleCmdlets = @{compatibility = @('core-6.1.0-windows') } } }
    }

    Context "script has violation" {
        It "detects violation" {
            $ruleTestDirectory = Join-Path $PSScriptRoot 'UseCompatibleCmdlets'
            $violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
            $settingsFilePath = Join-Path $ruleTestDirectory 'PSScriptAnalyzerSettings.psd1'
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings $settingsFilePath
            $diagnosticRecords.Count | Should -Be 1
        }
    }

    Context 'Correct Violations for commands' {
        It "Command '<Command>' - Settings '<Settings>'" -TestCases $testCases {
            Test-Command -Command $Command -Settings $settings -ExpectedOneViolation:$ExpectedOneViolation
        }
    }
}
