# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $reservedCharMessage = "The cmdlet 'Use-#Reserved' uses a reserved char in its name."
    $reservedCharName = "PSReservedCmdletChar"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingReservedCharNames.ps1 | Where-Object {$_.RuleName -eq $reservedCharName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $reservedCharName}
}

Describe "Avoid Using Reserved Char" {
    Context "When there are violations" {
        It "has 1 Reserved Char Violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $reservedCharMessage
        }
    }

    Context "When there are no violations" {
        It "has no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
