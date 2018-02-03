Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables produce warning of severity error" {
        It "Variable '<VariableName>' produce warning of severity error and throw SessionStateUnauthorizedAccessException" -TestCases @(
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
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "`$$($VariableName) = 'foo'" | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should Be 1
            $warnings.Severity | Should Be  "Error"
            
            Set-Variable -Name $VariableName -Value 'foo' -ErrorVariable errorVariable -ErrorAction SilentlyContinue
            $ErrorVariable | Should Not Be Null
            $ErrorVariable.Exception | Should Not Be Null
            if ($VariableName -ne 'Error') # setting the $Error variable has the side effect of the ErrorVariable to contain only the exception message string
            {
                $ErrorVariable.Exception.GetType() | Should Not Be Null
                $ErrorVariable.Exception.GetType().FullName | Should Be 'System.Management.Automation.SessionStateUnauthorizedAccessException'
            }
        }
    }
}