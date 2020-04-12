# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = [regex]::Escape("Verb-Files' defines the reserved common parameter 'Verbose'.")
    $violationName = "PSReservedParams"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidReservedParams" {
    Context "When there are violations" {
        It "has 1 avoid reserved parameters violations" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
