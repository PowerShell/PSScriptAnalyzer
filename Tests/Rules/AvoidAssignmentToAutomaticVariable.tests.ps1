Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables" {
        It "'?' Variable" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$? = Get-Alias' | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should Be 1
        }

        It "True Variable" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$true = Get-Alias' | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should Be 1
        }
    }
}