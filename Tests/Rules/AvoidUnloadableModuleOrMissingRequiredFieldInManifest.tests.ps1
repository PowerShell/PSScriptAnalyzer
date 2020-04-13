# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

    $missingMessage = "The member 'ModuleVersion' is not present in the module manifest."
    $missingMemberRuleName = "PSMissingModuleManifestField"
    $violationFilepath = Join-Path $PSScriptRoot "TestBadModule\TestBadModule.psd1"
    $violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object { $_.RuleName -eq $missingMemberRuleName }
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\TestGoodModule\TestGoodModule.psd1 | Where-Object { $_.RuleName -eq $missingMemberRuleName }
    $noHashtableFilepath = Join-Path $PSScriptRoot "TestBadModule\NoHashtable.psd1"
}

Describe "MissingRequiredFieldModuleManifest" {
    BeforeAll {
        Import-Module (Join-Path $PSScriptRoot "PSScriptAnalyzerTestHelper.psm1") -Force
    }

    AfterAll {
        Remove-Module PSScriptAnalyzerTestHelper
    }

    Context "When there are violations" {
        It "has 1 missing required field module manifest violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations.Message | Should -Match $missingMessage
        }

        It "has correct suggested corrections count" {
            $violations.SuggestedCorrections.Count | Should -Be 1
        }

        It "has the right suggested correction" {
            $expectedText = [System.Environment]::NewLine + '# Version number of this module.' +
            [System.Environment]::NewLine + "ModuleVersion = '1.0.0.0'" + [System.Environment]::NewLine

            $violations[0].SuggestedCorrections[0].Text | Should -BeExactly $expectedText
            Get-ExtentText $violations[0].SuggestedCorrections[0] $violationFilepath | Should -BeNullOrEmpty
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }

    Context "When an .psd1 file doesn't contain a hashtable" {
        It "does not throw exception" {
            Invoke-ScriptAnalyzer -Path $noHashtableFilepath -IncludeRule $missingMemberRuleName
        }
    }

    Context "Validate the contents of a .psd1 file" {
        It "detects a valid module manifest file" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/ManifestGood.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should -BeTrue
        }

        It "detects a .psd1 file which is not module manifest" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/PowerShellDataFile.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should -BeFalse
        }

        It "detects valid module manifest file for PSv5" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/ManifestGoodPsv5.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should -BeTrue
        }

        It "does not validate PSv5 module manifest file for PSv3 check" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/ManifestGoodPsv5.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"3.0.0") | Should -BeFalse
        }

        It "detects valid module manifest file for PSv4" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/ManifestGoodPsv4.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"4.0.0") | Should -BeTrue
        }

        It "detects valid module manifest file for PSv3" {
            $filepath = Join-Path $PSScriptRoot "TestManifest/ManifestGoodPsv3.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"3.0.0") | Should -BeTrue
        }
    }

    Context "When given a non module manifest file" {
        It "does not flag a PowerShell data file" {
            Invoke-ScriptAnalyzer `
                -Path "$PSScriptRoot/TestManifest/PowerShellDataFile.psd1" `
                -IncludeRule "PSMissingModuleManifestField" `
                -OutVariable ruleViolation
            $ruleViolation.Count | Should -Be 0
        }
    }
}
