Import-Module PSScriptAnalyzer 
$violationMessage = [regex]::Escape('$null should be on the left side of equality comparisons.')
$violationName = "PSPossibleIncorrectComparisonWithNull"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\PossibleIncorrectComparisonWithNull.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\PossibleIncorrectComparisonWithNullNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "PossibleIncorrectComparisonWithNull" {
    Context "When there are violations" {
        It "has 4 possible incorrect comparison with null violation" {
            $violations.Count | Should Be 4
        }

        It "has the correct description message" {
            $violations.Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}