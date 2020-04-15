# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSPossibleIncorrectUsageOfRedirectionOperator"
}

Describe "PossibleIncorrectUsageOfComparisonOperator" {
    Context "When there are violations" {
        It "File redirection operator inside if statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a > $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "File redirection operator with equals sign inside if statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a >=){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "File redirection operator inside if statement causes warning when wrapped in command expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a > ($b)){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "File redirection operator inside if statement causes warning when wrapped in expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a > "$b"){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "File redirection operator inside elseif statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){}elseif($a > $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }
    }

    Context "When there are no violations" {
        It "returns no violations when using correct greater than operator" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -gt $b){ }' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 0
        }
    }
}
