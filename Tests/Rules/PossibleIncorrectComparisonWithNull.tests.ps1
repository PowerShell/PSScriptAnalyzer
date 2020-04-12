# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = [regex]::Escape('$null should be on the left side of equality comparisons.')
    $violationName = "PSPossibleIncorrectComparisonWithNull"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\PossibleIncorrectComparisonWithNull.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\PossibleIncorrectComparisonWithNullNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "PossibleIncorrectComparisonWithNull" {
    Context "When there are violations" {
        It "has 4 possible incorrect comparison with null violation" {
            $violations.Count | Should -Be 4
        }

        It "has the correct description message" {
            $violations.Message | Should -Match $violationMessage
        }

        It "has the correct description message" {
            $violations[0].SuggestedCorrections[0].Text | Should -Be '$null -eq @("dfd", "eee")'
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
