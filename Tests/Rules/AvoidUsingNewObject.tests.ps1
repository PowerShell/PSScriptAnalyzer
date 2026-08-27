# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $NewObjectMessage = "Consider replacing the 'New-Object' cmdlet with type literals"
    $NewObjectName = "PSAvoidUsingNewObject"
    $violations = Invoke-ScriptAnalyzer "$PSScriptRoot\AvoidUsingNewObject.ps1" | Where-Object { $_.RuleName -eq $NewObjectName }
    $noViolations = Invoke-ScriptAnalyzer "$PSScriptRoot\AvoidUsingNewObjectNoViolations.ps1" | Where-Object { $_.RuleName -eq $NewObjectName }
}

Describe "AvoidUsingNewObject" {
    Context "When there are violations" {
        It "has 20 New-Object violations" {
            $violations.Count | Should -Be 20
        }

        It "has the correct description message for New-Object" {
            $violations[0].Message | Should -Match $NewObjectMessage
        }
    }

    Context "When there are no violations" {
        It "has 0 violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
