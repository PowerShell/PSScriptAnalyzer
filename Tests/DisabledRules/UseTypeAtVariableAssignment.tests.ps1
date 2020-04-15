$violationMessage = [regex]::Escape('Specify type at the assignment of variable $test')
$violationName = "PSUseTypeAtVariableAssignment"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\UseTypeAtVariableAssignment.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "UseTypeAtVariableAssignment" {
    Context "When there are violations" {
        It "has 3 Use Type At Variable Assignement violations" {
            $violations.Count | Should -Be 3
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}