Import-Module -Verbose PSScriptAnalyzer
$violationMessage = "Cmdlet 'Write-Warning' may be used incorrectly. Please check that all mandatory parameters are supplied."
$violationName = "PSUseCmdletCorrectly"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\UseCmdletCorrectly.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "UseCmdletCorrectly" {
    Context "When there are violations" {
        It "has 1 Use Cmdlet Correctly violation" {
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