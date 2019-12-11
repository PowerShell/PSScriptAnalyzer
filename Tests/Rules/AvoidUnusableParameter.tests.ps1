$ruleName = 'PSAvoidUnusableParameter'

Describe "AvoidUnusableParameter" {
    Context "When there are violations" {

        It "returns violation when the parameter name is 1 defined in function header" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p1([switch]$1) {if ($1) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "returns violation when the parameter name is 1 defined in param block" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p2 { param([switch]$2) if ($2) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "returns violations when there is more than 1 unusable parameter" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p3 { param([switch]$2,[switch]$3) if ($2 -or $3) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 2
        }

        It "returns violations when the parameter name is specified as using $`{variable} syntax" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function foo { param([object] ${|UnusableParam}) Write-Output ${|UnusableParam} }'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }
    }

    Context "When there are no violations" {

        It "returns no violations when the parameter name is p1" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p4([switch]$p1) {if ($p1) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
        It "returns no violations when there is more functions and more good parameters" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p123([switch]$p1,[int]$p2,[int]$p3) {if ($p1) {$p2} else {$p3}}; function p456([switch]$p4,[int]$p5,[int]$p6) {if ($p4) {$p5} else {$p6}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }
}
