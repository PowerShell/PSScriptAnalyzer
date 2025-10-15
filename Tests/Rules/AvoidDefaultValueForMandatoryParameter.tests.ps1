# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = 'PSAvoidDefaultValueForMandatoryParameter'
}

Describe "AvoidDefaultValueForMandatoryParameter" {

    Context "Basic mandatory parameter violations" {
        It "should flag mandatory parameter with default value (implicit)" {
            $script = 'Function Test { Param([Parameter(Mandatory)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "should flag mandatory parameter with default value (explicit true)" {
            $script = 'Function Test { Param([Parameter(Mandatory=$true)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "should flag mandatory parameter with default value (numeric true)" {
            $script = 'Function Test { Param([Parameter(Mandatory=1)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }
    }

    Context "Parameter sets (multiple Parameter attributes)" {
        It "should NOT flag parameter mandatory in some but not all parameter sets" {
            $script = @'
Function Test {
    Param(
        [Parameter(Mandatory, ParameterSetName='Set1')]
        [Parameter(ParameterSetName='Set2')]
        $Param = 'default'
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should flag parameter mandatory in ALL parameter sets" {
            $script = @'
Function Test {
    Param(
        [Parameter(Mandatory, ParameterSetName='Set1')]
        [Parameter(Mandatory, ParameterSetName='Set2')]
        $Param = 'default'
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "should handle mixed mandatory/non-mandatory in multiple parameter sets" {
            $script = @'
Function Test {
    Param(
        [Parameter(Mandatory=$true, ParameterSetName='Set1')]
        [Parameter(Mandatory=$false, ParameterSetName='Set2')]
        [Parameter(ParameterSetName='Set3')]
        $Param = 'default'
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }

    Context "Script-level param blocks" {
        It "should flag mandatory parameters with defaults in script-level param blocks" {
            $script = @'
Param(
    [Parameter(Mandatory)]
    $ScriptParam = 'default'
)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "should NOT flag non-mandatory parameters in script-level param blocks" {
            $script = @'
Param(
    [Parameter()]
    $ScriptParam = 'default'
)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }

    Context "Non-Parameter attributes" {
        It "should NOT flag non-Parameter attributes with Mandatory property" {
            $script = 'Function Test { Param([MyCustomAttribute(Mandatory)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should NOT flag parameters with only validation attributes" {
            $script = 'Function Test { Param([ValidateNotNull()]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }

    Context "Valid scenarios (no violations)" {
        It "should NOT flag mandatory parameters without default values" {
            $script = 'Function Test { Param([Parameter(Mandatory)]$Param) }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should NOT flag non-mandatory parameters with default values" {
            $script = 'Function Test { Param([Parameter(Mandatory=$false)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should NOT flag parameters without Parameter attributes" {
            $script = 'Function Test { Param($Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should NOT flag mandatory=0 parameters" {
            $script = 'Function Test { Param([Parameter(Mandatory=0)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }

    Context "Complex scenarios" {
        It "should handle multiple parameters with mixed violations" {
            $script = @'
Function Test {
    Param(
        [Parameter(Mandatory)]$BadParam = "default",
        [Parameter()]$GoodParam = "default",
        [Parameter(Mandatory)]$AnotherBadParam = "default"
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 2
        }

        It "should work with CmdletBinding" {
            $script = 'Function Test { [CmdletBinding()]Param([Parameter(Mandatory)]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }
    }

    Context "Edge cases" {
        It "should handle empty param blocks gracefully" {
            $script = 'Function Test { Param() }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }

        It "should handle null/empty default values" {
            $script = 'Function Test { Param([Parameter(Mandatory)]$Param = $null) }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "should handle parameters with multiple non-Parameter attributes" {
            $script = 'Function Test { Param([ValidateNotNull()][Alias("P")]$Param = "default") }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $script | Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }

}
