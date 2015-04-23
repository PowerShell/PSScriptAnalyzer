Import-Module PSScriptAnalyzer
$nounViolationMessage = "The cmdlet 'Verb-Files' uses a plural noun. A singular noun should be used instead."
$verbViolationMessage = "The cmdlet 'Verb-Files' uses an unapproved verb."
$nounViolationName = "PSUseSingularNouns"
$verbViolationName = "PSUseApprovedVerbs"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\BadCmdlet.ps1
$nounViolations = $violations | Where-Object {$_.RuleName -eq $nounViolationName}
$verbViolations = $violations | Where-Object {$_.RuleName -eq $verbViolationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1
$nounNoViolations = $noViolations | Where-Object {$_.RuleName -eq $nounViolationName}
$verbNoViolations = $noViolations | Where-Object {$_.RuleName -eq $verbViolationName}

Describe "UseSingularNouns" {
    Context "When there are violations" {
        It "has a cmdlet singular noun violation" {
            $nounViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $nounViolations[0].Message | Should Match $nounViolationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $nounNoViolations.Count | Should Be 0
        }
    }
}

Describe "UseApprovedVerbs" {
    Context "When there are violations" {
        It "has an approved verb violation" {
            $verbViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $verbViolations[0].Message | Should Match $verbViolationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $verbNoViolations.Count | Should Be 0
        }
    }
}