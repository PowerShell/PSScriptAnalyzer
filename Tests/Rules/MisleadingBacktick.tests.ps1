Import-Module PSScriptAnalyzer
$writeHostName = "PSMisleadingBacktick"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\MisleadingBacktick.ps1 | Where-Object {$_.RuleName -eq $writeHostName}
$noViolations = Invoke-ScriptAnalyzer $directory\NoMisleadingBacktick.ps1 | Where-Object {$_.RuleName -eq $clearHostName}

Describe "Avoid Misleading Backticks" {
    Context "When there are violations" {
        It "has 5 misleading backtick violations" {
            $violations.Count | Should Be 5
        }

        It "is " {
            $violations[0].Message | Should Match $writeHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}