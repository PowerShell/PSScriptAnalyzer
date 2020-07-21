# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "ReviewUnusedParameter" {
    BeforeAll {
        $RuleName = 'PSReviewUnusedParameter'
        $RuleSeverity = "Warning"
    }

    Context "When there are violations" {
        It "has 1 violation - function with 1 unused parameter" {
            $ScriptDefinition = 'function BadFunc1 { param ($Param1, $Param2) $Param1}'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 1
        }

        It "has 2 violations - function with 2 unused parameters" {
            $ScriptDefinition = 'function BadFunc1 { param ($Param1, $Param2) }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 2
        }

        It "has 1 violation - scriptblock with 1 unused parameter" {
            $ScriptDefinition = '{ param ($Param1) }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 1
        }

        It "doesn't traverse scriptblock scope" {
            $ScriptDefinition = '{ param ($Param1) }; { $Param1 }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 1
        }

        It "violations have correct rule and severity" {
            $ScriptDefinition = 'function BadFunc1 { param ($Param1, $Param2) $Param1}'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Severity | Select-Object -Unique | Should -Be $RuleSeverity
            $Violations.RuleName | Select-Object -Unique | Should -Be $RuleName
        }
    }

    Context "When there are no violations" {
        It "has no violations - function that uses all parameters" {
            $ScriptDefinition = 'function GoodFunc1 { param ($Param1, $Param2) $Param1; $Param2}'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations - function with splatting" {
            $ScriptDefinition = 'function GoodFunc1 { param ($Param1) $Splat = @{InputObject = $Param1}}'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when using PSBoundParameters" {
            $ScriptDefinition = 'function Bound { param ($Param1) Get-Foo @PSBoundParameters }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when using MyInvocation.BoundParameters" {
            $ScriptDefinition = 'function Bound { param ($Param1) $splat = $MyInvocation.BoundParameters; Get-Foo @splat }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when using PSCmdlet.MyInvocation.BoundParameters" {
            $ScriptDefinition = 'function Bound { param ($Param1) $splat = $PSCmdlet.MyInvocation.BoundParameters; Get-Foo @splat }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when parameter is called in child scope" -skip {
            $ScriptDefinition = 'function foo { param ($Param1) function Child { $Param1 } }'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }

        It "has no violations when case of parameter and variable usage do not match" -skip {
            $ScriptDefinition = 'function foo { param ($Param1, $param2) $param1; $Param2}'
            $Violations = Invoke-ScriptAnalyzer -ScriptDefinition $ScriptDefinition -IncludeRule $RuleName
            $Violations.Count | Should -Be 0
        }
    }
}
