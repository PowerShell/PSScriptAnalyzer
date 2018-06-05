Import-Module PSScriptAnalyzer
$oneCharMessage = "The cmdlet name O only has one character."
$oneCharName = "PSOneChar"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$invoke = Invoke-ScriptAnalyzer $directory\AvoidUsingReservedCharOneCharNames.ps1 | Where-Object {$_.RuleName -eq $oneCharName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $oneCharName}

Describe "Avoid Using One Char" {
    Context "When there are violations" {
        It "has 1 One Char Violation" {
            $oneCharViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $oneCharViolations[0].Message | Should -Match $oneCharMessage
        }
    }

    Context "When there are no violations" {
        It "has no violations" {
            $noReservedCharViolations.Count | Should -Be 0
        }
    }
}