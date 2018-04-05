$ruleName = "PSPossibleIncorrectUsageOfAssignmentOperator"

Describe "PossibleIncorrectUsageOfAssignmentOperator" {
    Context "When there are violations" {
        It "assignment inside if statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a=$b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "assignment inside if statement causes warning when when wrapped in command expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a=($b)){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "assignment inside if statement causes warning when wrapped in expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a="$b"){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "assignment inside elseif statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){}elseif($a = $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "double equals inside if statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a == $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "double equals inside if statement causes warning when wrapping it in command expresion" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a == ($b)){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "double equals inside if statement causes warning when wrapped in expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a == "$b"){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }
    }

    Context "When there are no violations" {
        It "returns no violations when there is no equality operator" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){$a=$b}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 0
        }

        It "returns no violations when there is an evaluation on the RHS" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a = Get-ChildItem){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 0
        }

        It "returns no violations when there is an evaluation on the RHS wrapped in an expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a = (Get-ChildItem)){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 0
        }

        It "returns no violations when there is an evaluation on the RHS wrapped in an expression and also includes a variable" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a = (Get-ChildItem $b)){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 0
        }
    }
}
