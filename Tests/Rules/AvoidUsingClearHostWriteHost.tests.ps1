Import-Module ScriptAnalyzer
Set-Alias ctss ConvertTo-SecureString
$consoleWriteMessage = "File AvoidUsingClearHostWriteHost.ps1 uses Console.WriteLine. Using Console to write is not recommended because it may not work in all hosts or there may even be no hosts at all. Use Write-Output instead."
$writeHostMessage = "File AvoidUsingClearHostWriteHost.ps1 uses Write-Host. This is not recommended because it may not work in some hosts or there may even be no hosts at all. Use Write-Output instead."
$clearHostMessage = "File AvoidUsingClearHostWriteHost.ps1 uses Clear-Host. This is not recommended because it may not work in some hosts or there may even be no hosts at all."
$writeHostName = "PSAvoidUsingWriteHost"
$clearHostName = "PSAvoidUsingClearHost"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$invocation = Invoke-ScriptAnalyzer $directory\AvoidUsingClearHostWriteHost.ps1
$clearHostViolations = $invocation | Where-Object {$_.RuleName -eq $clearHostName}
$writeHostViolations = $invocation | Where-Object {$_.RuleName -eq $writeHostName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingClearHostWriteHostNoViolations.ps1
$noClearHostViolations = $noViolations | Where-Object {$_.RuleName -eq $writeHostName}
$noWriteHostViolations = $noViolations | Where-Object {$_.RuleName -eq $clearHostName}

Describe "AvoidUsingClearHost" {
    Context "When there are violations" {
        It "has 3 Clear-Host violations" {
            $clearHostViolations.Count | Should Be 3
        }

        It "has the correct description message for Clear-Host" {
            $clearHostViolations[0].Message | Should Match $clearHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noClearHostViolations.Count | Should Be 0
        }
    }
}

Describe "AvoidUsingWriteHost" {
    Context "When there are violations" {
        It "has 3 Write-Host violations" {
            $writeHostViolations.Count | Should Be 3
        }

        It "has the correct description message for Write-Host" {
            $writeHostViolations[0].Message | Should Match $writeHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noWriteHostViolations.Count | Should Be 0
        }
    }
}