Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables" {

        $testCases_ReadOnlyVariables = @(
            @{ VariableName = '?'; ExpectedSeverity = 'Error'; }
            @{ VariableName = 'Error' ; ExpectedSeverity = 'Error' }
            @{ VariableName = 'ExecutionContext'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'false'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'Home'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'Host'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PID'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PSCulture'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PSEdition'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PSHome'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PSUICulture'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'PSVersionTable'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'ShellId'; ExpectedSeverity = 'Error' }
            @{ VariableName = 'true'; ExpectedSeverity = 'Error' }
            # Variables introuced only in PowerShell 6.0 have a Severity of Warning only
            @{ VariableName = 'IsCoreCLR'; ExpectedSeverity = 'Warning'; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsLinux'; ExpectedSeverity = 'Warning'; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsMacOS'; ExpectedSeverity = 'Warning'; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsWindows'; ExpectedSeverity = 'Warning'; OnlyPresentInCoreClr = $true }
        )

        It "Variable <VariableName> produces warning of Severity <ExpectedSeverity>" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "`$${VariableName} = 'foo'" | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
        }

        It "Using Variable <VariableName> as parameter name produces warning of Severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo{Param(`$$VariableName)}" | Where-Object {$_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
        }

        It "Using Variable <VariableName> as parameter name in param block produces warning of Severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo(`$$VariableName){}" | Where-Object {$_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
        }

        It "Does not flag parameter attributes" {
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'function foo{Param([Parameter(Mandatory=$true)]$param1)}' | Where-Object { $_.RuleName -eq $ruleName }
            $warnings.Count | Should -Be 0
        }

        It "Setting Variable <VariableName> throws exception to verify the variables is read-only" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity, $OnlyPresentInCoreClr)

            if ($OnlyPresentInCoreClr -and !$IsCoreCLR)
            {
                continue
            }
            
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