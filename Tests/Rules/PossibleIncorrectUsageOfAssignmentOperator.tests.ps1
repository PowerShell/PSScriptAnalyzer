Import-Module PSScriptAnalyzer
$ruleName = "PSPossibleIncorrectUsageOfAssignmentOperator"

Describe "PossibleIncorrectUsageOfAssignmentOperator" {
    Context "When there are violations" {
        It "assignment inside if statemenet causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a=$b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 1
        }

        It "assignment inside elseif statemenet causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){}elseif($a = $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 1
        }

        It "assignment inside else statemenet causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){}else{$a = $b}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 1
        }

        It "double equals inside if statemenet causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a == $b){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 1
        }
    }

    Context "When there are no violations" {
        It "returns no violations when there is no equality operator" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a -eq $b){$a=$b}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 0
        }

        It "returns no violations when there is an evaluation on the RHS" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'if ($a = Get-ChildItem){}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should Be 0
        }
    }
}