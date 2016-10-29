Import-Module PSScriptAnalyzer

$functionErroMessage = "Avoid creating functions with a Global scope."
$violationName = "AvoidGlobalFunctions"

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidGlobalFunctions.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidGlobalFunctionsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}


Describe "$violationName " {
    Context "When there are violations" {
        It "Has 5 avoid using empty Catch block violations" {
            $violations.Count | Should Be 1
        }

        It "Has the correct description message" {
            $violations[0].Message | Should Match $functionErroMessage
        }

    }

    Context "When there are no violations" {
        It "Returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}
