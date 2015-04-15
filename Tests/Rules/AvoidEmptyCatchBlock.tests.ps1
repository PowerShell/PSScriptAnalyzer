Import-Module PSScriptAnalyzer
$violationMessage = "Empty catch block is used. Please use Write-Error or throw statements in catch blocks."
$violationName = "PSAvoidUsingEmptyCatchBlock"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidEmptyCatchBlock.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidEmptyCatchBlockNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "UseDeclaredVarsMoreThanAssignments" {
    Context "When there are violations" {
        It "has 2 avoid using empty Catch block violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}