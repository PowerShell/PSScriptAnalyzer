# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "() has non-constant members. Invoking non-constant members may cause bugs in the script."
    $violationName = "PSAvoidInvokingEmptyMembers"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidInvokingEmptyMembers.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidInvokingEmptyMembersNonViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidInvokingEmptyMembers" {
    Context "When there are violations" {
        It "has one InvokeEmptyMember violations" {
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
