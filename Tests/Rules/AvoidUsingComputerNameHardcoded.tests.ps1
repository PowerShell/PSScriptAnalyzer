# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = [regex]::Escape("The ComputerName parameter of cmdlet 'Invoke-Command' is hardcoded. This will expose sensitive information about the system if the script is shared.")
    $violationName = "PSAvoidUsingComputerNameHardcoded"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingComputerNameHardcoded.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingComputerNameHardcodedNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidUsingComputerNameHardcoded" {
    Context "When there are violations" {
        It "has 2 avoid using ComputerName hardcoded violations" {
            $violations.Count | Should -Be 2
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
