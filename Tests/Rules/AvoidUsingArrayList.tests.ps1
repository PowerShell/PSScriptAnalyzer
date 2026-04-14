# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidArrayList"
    $ruleMessage = "The ArrayList class is used in '*'. Consider using a generic collection or a fixed array instead."
}

Describe "AvoidUsingWriteHost" {
    Context "When there are violations" {
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
        It "has ArrayList violations" {
            $violations.Count | Should -Be 12
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Like $ruleMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
