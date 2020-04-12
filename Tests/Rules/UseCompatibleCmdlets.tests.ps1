# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSUseCompatibleCmdlets"
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    $ruleTestDirectory = Join-Path $PSScriptRoot 'UseCompatibleCmdlets'
    Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')
}

Describe "UseCompatibleCmdlets" {
    Context "script has violation" {
        It "detects violation" {
            $violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
            $settingsFilePath =  [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettings.psd1');
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings $settingsFilePath
            $diagnosticRecords.Count | Should -Be 1
        }
    }

    Function Test-Command
    {
        param (
            [Parameter(ValueFromPipeline)]
            $command,
            $settings,
            $expectedViolations
        )
        process
        {
            It ("found {0} violations for '{1}'" -f $expectedViolations, $command) {
                $warnings = Invoke-ScriptAnalyzer -ScriptDefinition $command -IncludeRule $ruleName -Settings $settings
                $warnings.Count | Should -Be  $expectedViolations
                $warnings | ForEach-Object {
                    $_.RuleName | Should -Be 'PSUseCompatibleCmdlets'
                }
            }
        }
    }

    $settings = @{rules=@{PSUseCompatibleCmdlets=@{compatibility=@("core-6.1.0-windows")}}}

    Context "Microsoft.PowerShell.Core" {
         @('Enter-PSSession', 'Foreach-Object', 'Get-Command') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Non-builtin commands" {
        @('get-foo', 'get-bar', 'get-baz') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Aliases" {
        @('where', 'select', 'cd') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Commands present in reference platform but not in target platform" {
        @("Start-VM", "New-SmbShare", "Get-Disk") | `
            Test-Command -Settings $settings -ExpectedViolations 1
    }

    Context "Default reference can also be used as target platform" {
        $settings = @{rules=@{PSUseCompatibleCmdlets=@{compatibility=@("desktop-5.1.14393.206-windows")}}}
        @("Remove-Service") | Test-Command -Settings $settings -ExpectedViolations 1
    }
}
