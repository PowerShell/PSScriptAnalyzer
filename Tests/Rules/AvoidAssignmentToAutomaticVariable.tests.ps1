Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables produce warning of severity error" {
        It "Variable '<VariableName>'" -TestCases @(
            @{ VariableName = '?' }
            @{ VariableName = 'Error' }
            @{ VariableName = 'ExecutionContext' }
            @{ VariableName = 'false' }
            @{ VariableName = 'Home' }
            @{ VariableName = 'Host' }
            @{ VariableName = 'PID' }
            @{ VariableName = 'PSCulture' }
            @{ VariableName = 'PSEdition' }
            @{ VariableName = 'PSHome' }
            @{ VariableName = 'PSUICulture' }
            @{ VariableName = 'PSVersionTable' }
            @{ VariableName = 'ShellId' }
            @{ VariableName = 'true' }
        ) {
            param ($VariableName)
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "`$$($VariableName) = Get-Alias" | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should Be 1
            $warnings.Severity | Should Be  "Error"
        }
    }
}