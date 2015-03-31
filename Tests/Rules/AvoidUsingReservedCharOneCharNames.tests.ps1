Import-Module ScriptAnalyzer
$oneCharMessage = "The cmdlet name O only has one character."
$reservedCharMessage = "The cmdlet Use-#Reserved uses a reserved char in its name."
$oneCharName = "PSOneChar"
$reservedCharName = "PSReservedCmdletChar"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$invoke = Invoke-ScriptAnalyzer $directory\AvoidUsingReservedCharOneCharNames.ps1
$oneCharViolations = $invoke | Where-Object {$_.RuleName -eq $oneCharName}
$reservedCharViolations = $invoke | Where-Object {$_.RuleName -eq $reservedCharName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1
$noOneCharViolations = $noViolations | Where-Object {$_.RuleName -eq $oneCharName}
$noReservedCharViolations = $noViolations | Where-Object {$_.RuleName -eq $reservedCharName}

Describe "Avoid Using Reserved Char" {
    Context "When there are violations" {
        It "has 1 Reserved Char Violation" {
            $reservedCharViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $reservedCharViolations[0].Message | Should Match $reservedCharMessage
        }
    }

    Context "When there are no violations" {
        It "has no violations" {
            $noOneCharViolations.Count | Should Be 0
        }
    }
}

Describe "Avoid Using One Char" {
    Context "When there are violations" {
        It "has 1 One Char Violation" {
            $oneCharViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $oneCharViolations[0].Message | Should Match $oneCharMessage
        }
    }

    Context "When there are no violations" {
        It "has no violations" {
            $noReservedCharViolations.Count | Should Be 0
        }
    }
}