# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationName = "PSUseFullyQualifiedCmdletNames"
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
}

Describe "UseFullyQualifiedCmdletNames" {
    Context "When there are violations" {
        It "detects unqualified cmdlet calls" {
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "The cmdlet 'Get-Command' should be replaced with the fully qualified cmdlet name 'Microsoft.PowerShell.Core\\Get-Command'"
        }

        It "detects unqualified alias usage" {
            $scriptDefinition = 'gci C:\temp'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "The alias 'gci' should be replaced with the fully qualified cmdlet name 'Microsoft.PowerShell.Management\\Get-ChildItem'"
        }

        It "provides correct suggested corrections for cmdlets" {
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].SuggestedCorrections.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Core\Get-Command'
            $violations[0].SuggestedCorrections[0].Description | Should -Be "Replace 'Get-Command' with 'Microsoft.PowerShell.Core\Get-Command'"
        }

        It "provides correct suggested corrections for aliases" {
            $scriptDefinition = 'gci'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].SuggestedCorrections.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Management\Get-ChildItem'
            $violations[0].SuggestedCorrections[0].Description | Should -Be "Replace 'gci' with 'Microsoft.PowerShell.Management\Get-ChildItem'"
        }

        It "detects multiple violations in same script" {
            $scriptDefinition = @'
Get-Command
Write-Host "test"
gci -Recurse
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 3
            $violations[0].Extent.Text | Should -Be "Get-Command"
            $violations[1].Extent.Text | Should -Be "Write-Host"
            $violations[2].Extent.Text | Should -Be "gci"
        }

        It "detects violations in pipelines" {
            $scriptDefinition = 'Get-Process | Where-Object { $_.Name -eq "notepad" }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 2
            $violations[0].Extent.Text | Should -Be "Get-Process"
            $violations[1].Extent.Text | Should -Be "Where-Object"
        }

        It "detects violations in script blocks" {
            $scriptDefinition = 'Invoke-Command -ScriptBlock { Get-Process }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 2
            ($violations.Extent.Text -contains "Invoke-Command") | Should -Be $true
            ($violations.Extent.Text -contains "Get-Process") | Should -Be $true
        }

        It "detects violations with parameters" {
            $scriptDefinition = 'Get-ChildItem -Path C:\temp -Recurse'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-ChildItem"
        }

        It "detects violations with splatting" {
            $scriptDefinition = @'
$params = @{ Name = "notepad" }
Get-Process @params
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Process"
        }
    }

    Context "Violation Extent" {
        It "should return only the cmdlet extent, not parameters" {
            $scriptDefinition = 'Get-Command -Name Test'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].Extent.Text | Should -Be "Get-Command"
        }

        It "should return only the alias extent, not parameters" {
            $scriptDefinition = 'gci -Recurse'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].Extent.Text | Should -Be "gci"
        }
    }

    Context "When there are no violations" {
        It "ignores already qualified cmdlets" {
            $scriptDefinition = @'
Microsoft.PowerShell.Core\Get-Command
Microsoft.PowerShell.Utility\Write-Host "test"
Microsoft.PowerShell.Management\Get-ChildItem -Path C:\temp
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "ignores native commands" {
            $scriptDefinition = @'
where.exe notepad
cmd /c dir
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "ignores variables" {
            $scriptDefinition = @'
$GetCommand = "test"
$variable = $true
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "ignores string literals containing cmdlet names" {
            $scriptDefinition = @'
$command = "Get-Command"
"The Get-Command cmdlet is useful"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "handles mixed qualified and unqualified cmdlets" {
            $scriptDefinition = @'
Microsoft.PowerShell.Core\Get-Command
Get-Process
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Process"
        }
    }

    Context "Different Module Contexts" {
        It "handles cmdlets from different modules" {
            $scriptDefinition = @'
Get-Content "file.txt"
ConvertTo-Json @{}
Test-Connection "server"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 3
            
            $getContentViolation = $violations | Where-Object { $_.Extent.Text -eq "Get-Content" }
            $getContentViolation.SuggestedCorrections[0].Text | Should -Match "Get-Content$"
            
            $convertToJsonViolation = $violations | Where-Object { $_.Extent.Text -eq "ConvertTo-Json" }
            $convertToJsonViolation.SuggestedCorrections[0].Text | Should -Match "ConvertTo-Json$"
        }

        It "suggests different modules for different cmdlets" {
            $scriptDefinition = @'
Get-Command
Write-Host "test"
Get-ChildItem
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 3

            $getCmdViolation = $violations | Where-Object { $_.Extent.Text -eq "Get-Command" }
            $getCmdViolation.SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Core\Get-Command'

            $writeHostViolation = $violations | Where-Object { $_.Extent.Text -eq "Write-Host" }
            $writeHostViolation.SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Utility\Write-Host'

            $getChildItemViolation = $violations | Where-Object { $_.Extent.Text -eq "Get-ChildItem" }
            $getChildItemViolation.SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Management\Get-ChildItem'
        }
    }

    Context "Severity and Rule Properties" {
        It "has Warning severity" {
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].Severity | Should -Be ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity]::Warning)
        }

        It "has correct rule name" {
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations[0].RuleName | Should -Be $violationName
        }
    }
}