# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "Function 'Verb-Noun2' in file 'AvoidShouldContinueWithoutForce.ps1' uses ShouldContinue but does not have a boolean force parameter. The force parameter will allow users of the script to bypass ShouldContinue prompt"
    $violationName = "PSAvoidShouldContinueWithoutForce"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidShouldContinueWithoutForce.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidShouldContinueWithoutForce" {
    Context "When there are violations" {
        It "has 2 avoid ShouldContinue without boolean Force parameter violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
