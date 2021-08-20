# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidMultipleTypesParameter"

    $settings = @{
        IncludeRules = @($ruleName)
    }
}

Describe 'AvoidMultipleTypesParameter' {
    Context 'When there are violations' {
        BeforeAll {
            $scriptDefinition = @'
function F10 ([int][switch] $s1, [int] $p1){}
function F11 ([switch][boolean] $s1, [int] $p1){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violationMessage = 'Parameter ''\$s1'' has more than one type specifier.'
        }
        It "has two AvoidMultipleTypesParameter violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
            $violations[1].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        BeforeAll {
            $scriptDefinition = @'
function F10 ([switch] $s1, [int] $p1){}
function F11 ([boolean] $s1, [int] $p1){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
        }

        It "returns no violations" {
            $violations.Count | Should -Be 0
        }
    }
}
