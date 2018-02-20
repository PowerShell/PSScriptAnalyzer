Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables" {

        $readOnlyVariableSeverity = "Error"
        $testCases_ReadOnlyVariables = @(
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
        )

        It "Variable '<VariableName>' produces warning of severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName)

            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "`$${VariableName} = 'foo'" | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $readOnlyVariableSeverity
        }

        It "Using Variable '<VariableName>' as parameter name produces warning of severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo{Param(`$$VariableName)}" | Where-Object {$_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $readOnlyVariableSeverity
        }

        It "Using Variable '<VariableName>' as parameter name in param block produces warning of severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo(`$$VariableName){}" | Where-Object {$_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $readOnlyVariableSeverity
        }

        It "Does not flag parameter attributes" {
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'function foo{Param([Parameter(Mandatory=$true)]$param1)}' | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 0
        }

        It "Setting Variable '<VariableName>' throws exception to verify the variables is read-only" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName)
            
            # Setting the $Error variable has the side effect of the ErrorVariable to contain only the exception message string, therefore exclude this case.
            # For the library test in WMF 4, assigning a value $PSEdition does not seem to throw an error, therefore this special case is excluded as well.
            if ($VariableName -ne 'Error' -and ($VariableName -ne 'PSEdition' -and $PSVersionTable.PSVersion.Major -ne 4))
            {
                try
                {
                    Set-Variable -Name $VariableName -Value 'foo' -ErrorVariable errorVariable -ErrorAction Stop
                    throw "Expected exception did not occur when assigning value to read-only variable '$VariableName'"
                }
                catch
                {
                    $_.FullyQualifiedErrorId | Should -Be 'VariableNotWritable,Microsoft.PowerShell.Commands.SetVariableCommand'
                }
            }
        }

    }
}