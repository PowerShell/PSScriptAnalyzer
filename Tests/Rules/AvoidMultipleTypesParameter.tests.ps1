BeforeAll {
    $violationMessage = 'Parameter ''\$s1'' has more than one type specifier.'
    $violationName = "PSAvoidMultipleTypesParameter"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidMultipleTypesParameter.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidMultipleTypesParameterNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidMultipleTypesParameter" {
    Context "When there are violations" {
        It "has two AvoidMultipleTypesParameter violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
            $violations[1].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}