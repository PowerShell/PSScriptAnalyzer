Import-Module PSScriptAnalyzer
$violationMessage = [regex]::Escape("The ComputerName parameter of cmdlet 'Invoke-Command' is hardcoded. This will expose sensitive information about the system if the script is shared.")
$violationName = "PSAvoidUsingComputerNameHardcoded"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingComputerNameHardcoded.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingComputerNameHardcodedNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingComputerNameHardcoded" {
    Context "When there are violations" {
        It "has 2 avoid using ComputerName hardcoded violations" {
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