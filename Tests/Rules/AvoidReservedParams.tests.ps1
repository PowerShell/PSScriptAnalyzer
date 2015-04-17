Import-Module PSScriptAnalyzer
$violationMessage = [regex]::Escape("Verb-Files' defines the reserved common parameter 'Verbose'.")
$violationName = "PSReservedParams"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidReservedParams" {
    Context "When there are violations" {
        It "has 1 avoid reserved parameters violations" {
            $violations.Count | Should Be 1
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