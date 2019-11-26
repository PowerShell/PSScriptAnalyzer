$ruleName = 'PSAvoidUnusableParameter'

Describe "AvoidUnusableParameter" {
    Context "When there are violations" {

        # It "returns violations when the parameter name is 1 defined in function header" {
		# 	$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p1([switch]$1) {if ($1) {"Yes"} else {"No"}}'
        #         Where-Object { $_.RuleName -eq $ruleName }
        #     $violations.Count | Should -Be 1
        # }

        It "returns violations when the parameter name is 1 defined in param block" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p2 { param([switch]$2) if ($2) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "returns violations when the parameter name is specified as using $`{variable} syntax" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function foo { param([object] ${|UnusableParam}) Write-Output ${|UnusableParam} }'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }
    }

    Context "When there are no violations" {

        It "returns no violations when the parameter name is p1" {
			$violations = Invoke-ScriptAnalyzer -ScriptDefinition 'function p1([switch]$p1) {if ($p1) {"Yes"} else {"No"}}'
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }
}
