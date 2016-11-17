Import-Module PSScriptAnalyzer

$AvoidGlobalAliasesError = "Avoid creating aliases with a Global scope."
$violationName = "PSAvoidGlobalAliases"

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliases.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliasesNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}


Describe "$violationName " {
    Context "When there are violations" {
        It "Has 4 avoid global alias violations" {
            $violations.Count | Should Be 4
        }

        It "Has the correct description message" {
            $violations[0].Message | Should Match $AvoidGlobalAliasesError
        }
    }

    Context "When there are no violations" {
        It "Returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}
