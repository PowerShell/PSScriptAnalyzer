Import-Module PSScriptAnalyzer
$violationMessage = [regex]::Escape("There is no call to Write-Verbose in the function 'Verb-Files'.")
$violationName = "PSProvideVerboseMessage"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
$dscViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "ProvideVerboseMessage" {
    Context "When there are violations" {
        It "has 1 provide verbose violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }

        It "Does not count violation in DSC class" {
            $dscViolations.Count | Should -Be 0
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
