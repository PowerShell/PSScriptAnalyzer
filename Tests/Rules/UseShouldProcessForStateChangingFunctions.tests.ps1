# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "Function 'Set-MyObject' has verb that could change system state. Therefore, the function has to support 'ShouldProcess'"
    $violationName = "PSUseShouldProcessForStateChangingFunctions"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\UseShouldProcessForStateChangingFunctions.ps1 | Where-Object { $_.RuleName -eq $violationName }
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\UseShouldProcessForStateChangingFunctionsNoViolations.ps1 | Where-Object { $_.RuleName -eq $violationName }
}

Describe "It checks UseShouldProcess is enabled when there are state changing verbs in the function names" {
    Context "When function name has state changing verb" {
        It 'Finds verb "<Verb>" in function name' {
            Param($Verb)
            $scriptDefinition = "Function New-${Verb} () {{ }}"
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
        } -TestCases @(
            @{
                Verb = 'New'
            }
            @{
                Verb = 'Set'
            }
            @{
                Verb = 'Remove'
            }
            @{
                Verb = 'Start'
            }
            @{
                Verb = 'Stop'
            }
            @{
                Verb = 'Restart'
            }
            @{
                Verb = 'Reset'
            }
            @{
                Verb = 'Update'
            }
        )
    }

    Context "When there are violations" {
        It "has correct number of violations where ShouldProcess is not supported" {
            $violations.Count | Should -Be 5
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }

        It "has the correct extent" {
            $violations[0].Extent.Text | Should -Be "Set-MyObject"
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        It "Workflows should not trigger a warning because they do not allow SupportsShouldProcess" -Skip:$IsCoreCLR {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'workflow Set-Something {[CmdletBinding()]Param($Param1)}' | Where-Object {
                $_.RuleName -eq 'PSUseShouldProcessForStateChangingFunctions' }
            $violations.Count | Should -Be 0
        }
    }
}
