$ruleName = "PSAvoidAssignmentToAutomaticVariable"

Describe "AvoidAssignmentToAutomaticVariables" {
    Context "ReadOnly Variables" {

        $excpectedSeverityForAutomaticVariablesInPowerShell6 = 'Warning'
        if ($PSVersionTable.PSVersion.Major -ge 6)
        {
            $excpectedSeverityForAutomaticVariablesInPowerShell6 = 'Error'
        }

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
            @{ VariableName = 'IsCoreCLR'; ExpectedSeverity = $excpectedSeverityForAutomaticVariablesInPowerShell6; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsLinux'; ExpectedSeverity = $excpectedSeverityForAutomaticVariablesInPowerShell6; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsMacOS'; ExpectedSeverity = $excpectedSeverityForAutomaticVariablesInPowerShell6; OnlyPresentInCoreClr = $true }
            @{ VariableName = 'IsWindows'; ExpectedSeverity = $excpectedSeverityForAutomaticVariablesInPowerShell6; OnlyPresentInCoreClr = $true }
        )

        It "Variable <VariableName> produces warning of Severity <ExpectedSeverity>" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "`$${VariableName} = 'foo'" -ExcludeRule PSUseDeclaredVarsMoreThanAssignments
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
            $warnings.RuleName | Should -Be $ruleName
        }

        It "Using Variable <VariableName> as parameter name produces warning of Severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo{Param(`$$VariableName)}"
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
            $warnings.RuleName | Should -Be $ruleName
        }

        It "Using Variable <VariableName> as parameter name in param block produces warning of Severity error" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity)

            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition "function foo(`$$VariableName){}" -ExcludeRule PSReviewUnusedParameter
            $warnings.Count | Should -Be 1
            $warnings.Severity | Should -Be $ExpectedSeverity
            $warnings.RuleName | Should -Be $ruleName
        }

        It "Does not flag parameter attributes" {
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition 'function foo{Param([Parameter(Mandatory=$true)]$param1)}' -ExcludeRule PSReviewUnusedParameter
            $warnings.Count | Should -Be 0
        }

        It "Does not throw a NullReferenceException when using assigning a .Net property to a .Net property (Bug in 1.17.0 - issue 1007)" {
            Invoke-ScriptAnalyzer -ScriptDefinition '[foo]::bar = [baz]::qux' -ErrorAction Stop
        }

        It "Does not flag properties of a readonly variable (issue 1012)" {
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$Host.PrivateData["ErrorBackgroundColor"] = "Black"'
            $warnings.Count | Should -Be 0
        }

        It "Does not flag RHS of variable assignment (Bug in 1.17.0, issue 1013)" {
            [System.Array] $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '[foo]::bar = $true'
            $warnings.Count | Should -Be 0
        }

        It "Setting Variable <VariableName> throws exception in applicable PowerShell version to verify the variables is read-only" -TestCases $testCases_ReadOnlyVariables {
            param ($VariableName, $ExpectedSeverity, $OnlyPresentInCoreClr)

            if ($OnlyPresentInCoreClr -and !$IsCoreCLR)
            {
                # In this special case we expect it to not throw
                Set-Variable -Name $VariableName -Value 'foo'
                continue
            }

            # Setting the $Error variable has the side effect of the ErrorVariable to contain only the exception message string, therefore exclude this case.
            # For the library test in WMF 4, assigning a value $PSEdition does not seem to throw an error, therefore this special case is excluded as well.
            if ($VariableName -ne 'Error' -and ($VariableName -ne 'PSEdition' -and $PSVersionTable.PSVersion.Major -ne 4))
            {
                try
                {
                    # Global scope has to be used due to a bug in PS. https://github.com/PowerShell/PowerShell/issues/6378
                    Set-Variable -Name $VariableName -Value 'foo' -ErrorVariable errorVariable -ErrorAction Stop -Scope Global
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
