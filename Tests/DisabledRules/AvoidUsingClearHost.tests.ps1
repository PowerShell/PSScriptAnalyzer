Import-Module PSScriptAnalyzer
Set-Alias ctss ConvertTo-SecureString
$clearHostMessage = "File 'AvoidUsingClearHostWriteHost.ps1' uses Clear-Host. This is not recommended because it may not work in some hosts or there may even be no hosts at all."
$clearHostName = "PSAvoidUsingClearHost"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingClearHost.ps1 | Where-Object {$_.RuleName -eq $clearHostName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingClearHostNoViolations.ps1 | Where-Object {$_.RuleName -eq $writeHostName}

Describe "AvoidUsingClearHost" {
    Context "When there are violations" {
        It "has 3 Clear-Host violations" {
            $violations.Count | Should -Be 3
        }

        It "has the correct description message for Clear-Host" {
            $violations[0].Message | Should -Match $clearHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}