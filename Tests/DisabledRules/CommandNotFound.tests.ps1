Import-Module -Verbose ScriptAnalyzer
$violationMessage = "Command Get-WrongCommand Is Not Found"
$violationName = "PSCommandNotFound"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\CommandNotFound.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolationsDSC = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\serviceconfigdisabled.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "CommandNotFound" {
    Context "When there are violations" {
        It "has 1 Command Not Found violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        It "returns no violations for DSC configuration" {
            $noViolationsDSC.Count | Should -Be 0
        }
    }
}