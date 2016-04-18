Import-Module PSScriptAnalyzer
$violationMessage = "File 'AvoidUsingPing.ps1' uses Ping command. For PowerShell 5.0 and above, use Test-NetConnection or Test-Connection cmdlet which perform the same tasks as the Ping command."
$violationName = "PSAvoidUsingPing"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingPing.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingPingNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingAlias" {
    Context "When there are violations" {
        It "has 2 Avoid Using Alias Cmdlets violations" {
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
