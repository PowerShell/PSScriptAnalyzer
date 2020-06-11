# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $functionErroMessage = "Avoid creating functions with a Global scope."
    $violationName = "PSAvoidGlobalFunctions"

    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalFunctions.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalFunctionsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "$violationName " {
    Context "When there are violations" {
        It "Has 1 avoid global function violations" {
            $violations.Count | Should -Be 1
        }

        It "Has the correct description message" {
            $violations[0].Message | Should -Match $functionErroMessage
        }

    }

    Context "When there are no violations" {
        It "Returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
