Describe "ReviewUnusedParameter" {
    BeforeAll {
        $RuleName = 'PSReviewUnusedParameter'
        $RuleSeverity = "Warning"
    }

    Context "When there are violations" {
        $Cases = @(
            @{
                ScriptDefinition   = 'function BadFunc1 { param ($Param1, $Param2) $Param1}'
                Name               = "function with 1 unused parameter"
                NumberOfViolations = 1
            }
            @{
                ScriptDefinition   = 'function BadFunc1 { param ($Param1, $Param2) }'
                Name               = "function with 2 unused parameters"
                NumberOfViolations = 2
            }
            @{
                ScriptDefinition   = '{ param ($Param1) }'
                Name               = "scriptblock with unused parameter"
                NumberOfViolations = 1
            }
            @{
                ScriptDefinition   = '{ param ($Param1) }; { $Param1 }'
                Name               = "parameter with same name in different scope"
                NumberOfViolations = 1
            }
        )
        It "has 1 violation for <Name>" -TestCases $Cases {
            param ($ScriptDefinition, $Name, $NumberOfViolations)

            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be $NumberOfViolations
            $Violations.Severity | Select-Object -Unique | Should -Be $RuleSeverity
            $Violations.RuleName | Select-Object -Unique | Should -Be $RuleName
        }
    }

    Context "When there are no violations" {
        $Cases = @(
            @{
                ScriptDefinition = 'function GoodFunc1 { param ($Param1, $Param2) $Param1; $Param2}'
                Name             = "function with 0 unused parameters"
            }
            @{
                ScriptDefinition = 'function GoodFunc1 { param ($Param1) $Splat = @{InputObject = $Param1}}'
                Name             = "function with splatted parameter"
            }
        )
        It "has no violations for function <Name>" -TestCases $Cases {
            param ($ScriptDefinition, $Name)

            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when using PSBoundParameters" {
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function Bound { param ($Param1) Get-Foo @PSBoundParameters }' -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when parameter is called in child scope" -Skip {
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function Param { param ($Param1) function Child { $Param1 } }' -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }
    }
}
