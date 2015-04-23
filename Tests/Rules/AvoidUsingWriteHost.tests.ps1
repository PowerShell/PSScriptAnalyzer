Import-Module PSScriptAnalyzer
Set-Alias ctss ConvertTo-SecureString
$writeHostMessage = [Regex]::Escape("File 'AvoidUsingWriteHost.ps1' uses Write-Host. This is not recommended because it may not work in some hosts or there may even be no hosts at all. Use Write-Output instead.")
$writeHostName = "PSAvoidUsingWriteHost"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingWriteHost.ps1 | Where-Object {$_.RuleName -eq $writeHostName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingWriteHostNoViolations.ps1 | Where-Object {$_.RuleName -eq $clearHostName}

Describe "AvoidUsingWriteHost" {
    Context "When there are violations" {
        It "has 3 Write-Host violations" {
            $violations.Count | Should Be 3
        }

        It "has the correct description message for Write-Host" {
            $violations[0].Message | Should Match $writeHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}