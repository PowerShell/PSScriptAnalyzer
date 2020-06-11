# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    Set-Alias ctss ConvertTo-SecureString
    $writeHostMessage = [Regex]::Escape("File 'AvoidUsingWriteHost.ps1' uses Write-Host. Avoid using Write-Host because it might not work in all hosts, does not work when there is no host, and (prior to PS 5.0) cannot be suppressed, captured, or redirected. Instead, use Write-Output, Write-Verbose, or Write-Information.")
    $writeHostName = "PSAvoidUsingWriteHost"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingWriteHost.ps1 | Where-Object {$_.RuleName -eq $writeHostName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingWriteHostNoViolations.ps1 | Where-Object {$_.RuleName -eq $clearHostName}
}

Describe "AvoidUsingWriteHost" {
    Context "When there are violations" {
        It "has 4 Write-Host violations" {
            $violations.Count | Should -Be 4
        }

        It "has the correct description message for Write-Host" {
            $violations[0].Message | Should -Match $writeHostMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
