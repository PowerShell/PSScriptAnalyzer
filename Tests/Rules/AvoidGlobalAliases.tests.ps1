Import-Module PSScriptAnalyzer

$AvoidGlobalAliasesError = "Avoid creating aliases with a Global scope."
$violationName = "AvoidGlobalAliases"

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliases.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliasesNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}


Describe "$violationName " {
    Context "When there are violations" {
        It "Has 5 avoid using empty Catch block violations" {
            $violations.Count | Should Be 4
        }

        It "Has the correct description message" {
            $violations[0].Message | Should Match $AvoidGlobalAliasesError
            $violations[1].Message | Should Match $AvoidGlobalAliasesError
            $violations[2].Message | Should Match $AvoidGlobalAliasesError
            $violations[3].Message | Should Match $AvoidGlobalAliasesError
        }

    }

    Context "When there are no violations" {
        It "Returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}
