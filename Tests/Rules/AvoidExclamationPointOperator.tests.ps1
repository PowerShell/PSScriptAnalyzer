# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidExclamationPointOperator"

    $ruleSettings = @{
        Enable = $true
    }
    $settings = @{
        IncludeRules = @($ruleName)
        Rules        = @{ $ruleName = $ruleSettings }
    }
}

Describe "AvoidExclamationPointOperator" {
    Context "When the rule is not enabled explicitly" {
        It "Should not find violations" {
            $def = '!$true'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def
            $violations.Count | Should -Be 0
        }
    }

    Context "Given a line with the exclamation point operator" {
        It "Should find one violation" {
            $def = '!$true'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context "Given a line with the exclamation point operator" {
        It "Should replace the exclamation point operator with the -not operator" {
            $def = '!$true'
            $expected = '-not $true'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
    Context "Given a line with the exclamation point operator followed by a space" {
        It "Should replace the exclamation point operator without adding an additional space" {
            $def = '! $true'
            $expected = '-not $true'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
    Context "Given a line with a string containing an exclamation point" {
        It "Should not replace it" {
            $def = '$MyVar = "Should not replace!"'
            $expected = '$MyVar = "Should not replace!"'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
}