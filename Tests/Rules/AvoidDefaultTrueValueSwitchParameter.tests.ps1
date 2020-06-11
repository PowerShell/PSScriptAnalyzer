# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "File 'AvoidDefaultTrueValueSwitchParameter.ps1' has a switch parameter default to true."
    $violationName = "PSAvoidDefaultValueSwitchParameter"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidDefaultTrueValueSwitchParameter.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidDefaultTrueValueSwitchParameterNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidDefaultTrueValueSwitchParameter" {
    Context "When there are violations" {
        It "has 2 avoid using switch parameter default to true violation" {
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
