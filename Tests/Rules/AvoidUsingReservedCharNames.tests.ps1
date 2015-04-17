Import-Module PSScriptAnalyzer
$reservedCharMessage = "The cmdlet 'Use-#Reserved' uses a reserved char in its name."
$reservedCharName = "PSReservedCmdletChar"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingReservedCharNames.ps1 | Where-Object {$_.RuleName -eq $reservedCharName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $reservedCharName}

Describe "Avoid Using Reserved Char" {
    Context "When there are violations" {
        It "has 1 Reserved Char Violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $reservedCharMessage
        }
    }

    Context "When there are no violations" {
        It "has no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}