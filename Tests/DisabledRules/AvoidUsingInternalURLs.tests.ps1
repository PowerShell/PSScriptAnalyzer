Import-Module PSScriptAnalyzer
$violationMessage = "could be an internal URL. Using internal URL directly in the script may cause potential information disclosure."
$violationName = "PSAvoidUsingInternalURLs"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingInternalURLs.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingInternalURLsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingInternalURLs" {
    Context "When there are violations" {
        It "has 3 violations" {
            $violations.Count | Should -Be 3
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}