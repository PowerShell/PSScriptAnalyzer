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
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "The cmdlet 'Get-Command' should be replaced with the fully qualified cmdlet name 'Microsoft.PowerShell.Core\\Get-Command'"
        }

        It "detects unqualified alias usage" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'gci C:\temp'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Message | Should -Match "The alias 'gci' should be replaced with the fully qualified cmdlet name 'Microsoft.PowerShell.Management\\Get-ChildItem'"
        }

        It "provides correct suggested corrections for cmdlets" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations[0].SuggestedCorrections.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Core\Get-Command'
            $violations[0].SuggestedCorrections[0].Description | Should -Be "Replace 'Get-Command' with 'Microsoft.PowerShell.Core\Get-Command'"
        }

        It "provides correct suggested corrections for aliases" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'gci'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations[0].SuggestedCorrections.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be 'Microsoft.PowerShell.Management\Get-ChildItem'
            $violations[0].SuggestedCorrections[0].Description | Should -Be "Replace 'gci' with 'Microsoft.PowerShell.Management\Get-ChildItem'"
        }

        It "detects multiple violations in same script" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
Get-Command
Write-Host "test"
gci -Recurse
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 3
            $violations[0].Extent.Text | Should -Be "Get-Command"
            $violations[1].Extent.Text | Should -Be "Write-Host"
            $violations[2].Extent.Text | Should -Be "gci"
        }

        It "detects violations in pipelines" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Process | Where-Object { $_.Name -eq "notepad" }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 2
            $violations[0].Extent.Text | Should -Be "Get-Process"
            $violations[1].Extent.Text | Should -Be "Where-Object"
        }

        It "detects violations in script blocks" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Invoke-Command -ScriptBlock { Get-Process }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 2
            ($violations.Extent.Text -contains "Invoke-Command") | Should -Be $true
            ($violations.Extent.Text -contains "Get-Process") | Should -Be $true
        }

        It "detects violations with parameters" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-ChildItem -Path C:\temp -Recurse'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-ChildItem"
        }

        It "detects violations with splatting" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
$params = @{ Name = "notepad" }
Get-Process @params
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Process"
        }
    }

    Context "Configuration - Default Behavior" {
        It "is disabled by default (no configuration)" {
            $scriptDefinition = @'
Get-Process
Get-ChildItem
Start-Process notepad
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "processes all cmdlets when enabled with empty IgnoredModules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @()
                    }
                }
            }
            $scriptDefinition = @'
Get-Process
Get-ChildItem
Start-Process notepad
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 3
            ($violations.Extent.Text -contains "Get-Process") | Should -Be $true
            ($violations.Extent.Text -contains "Get-ChildItem") | Should -Be $true
            ($violations.Extent.Text -contains "Start-Process") | Should -Be $true
        }

        It "processes all cmdlets from all modules when enabled" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
Write-Host "test"
ConvertTo-Json @{}
Out-String
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 3
            ($violations.Extent.Text -contains "Write-Host") | Should -Be $true
            ($violations.Extent.Text -contains "ConvertTo-Json") | Should -Be $true
            ($violations.Extent.Text -contains "Out-String") | Should -Be $true
        }

        It "processes cmdlets from Core module when enabled" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Command"
        }
    }

    Context "Configuration - Custom IgnoredModules" {
        It "respects custom ignored modules configuration" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @('Microsoft.PowerShell.Core')
                    }
                }
            }

            $scriptDefinition = @'
Get-Command
Get-Process
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Process"  # Get-Command should be ignored
        }

        It "handles empty IgnoredModules array (flags everything)" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @()
                    }
                }
            }

            $scriptDefinition = @'
Get-Process
Write-Host "test"
Get-Command
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 3
            ($violations.Extent.Text -contains "Get-Process") | Should -Be $true
            ($violations.Extent.Text -contains "Write-Host") | Should -Be $true
            ($violations.Extent.Text -contains "Get-Command") | Should -Be $true
        }

        It "handles multiple custom ignored modules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @(
                            'Microsoft.PowerShell.Core',
                            'Microsoft.PowerShell.Management',
                            'Microsoft.PowerShell.Utility'
                        )
                    }
                }
            }

            $scriptDefinition = @'
Get-Command
Get-Process
Write-Host "test"
ConvertTo-Json @{}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "is case-insensitive for module names in IgnoredModules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @('microsoft.powershell.core')  # lowercase
                    }
                }
            }

            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }
    }

    Context "Configuration - Enable/Disable" {
        It "can be disabled via configuration" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $false
                    }
                }
            }

            $scriptDefinition = @'
Get-Command
Get-Process
Write-Host "test"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "is disabled by default when no Enable setting is specified" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        IgnoredModules = @()  # Only specify IgnoredModules, not Enable
                    }
                }
            }

            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }
    }

    Context "Configuration - Mixed Scenarios" {
        It "handles aliases from ignored modules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @('Microsoft.PowerShell.Management')
                    }
                }
            }

            $scriptDefinition = 'gci C:\temp'  # gci resolves to Get-ChildItem from Management module
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "handles mixed ignored and non-ignored modules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @('Microsoft.PowerShell.Management')
                    }
                }
            }

            $scriptDefinition = @'
Get-Process
Get-Command
Write-Host "test"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 2
            ($violations.Extent.Text -contains "Get-Command") | Should -Be $true
            ($violations.Extent.Text -contains "Write-Host") | Should -Be $true
            ($violations.Extent.Text -contains "Get-Process") | Should -Be $false  # Should be ignored
        }

        It "caches ignored module decisions correctly" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                        IgnoredModules = @('Microsoft.PowerShell.Management')
                    }
                }
            }

            $scriptDefinition = @'
Get-Process
Get-ChildItem
Get-Process
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0  # All should be ignored due to caching
        }
    }

    Context "Violation Extent" {
        It "should return only the cmdlet extent, not parameters" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command -Name Test'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations[0].Extent.Text | Should -Be "Get-Command"
        }

        It "should return only the alias extent, not parameters" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'gci -Recurse'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
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
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
Microsoft.PowerShell.Core\Get-Command
Get-Process
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be "Get-Process"
        }
    }

    Context "Different Module Contexts" {
        It "handles cmdlets from different modules" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
Get-Content "file.txt"
ConvertTo-Json @{}
Test-Connection "server"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 3

            $getContentViolation = $violations | Where-Object { $_.Extent.Text -eq "Get-Content" }
            $getContentViolation.SuggestedCorrections[0].Text | Should -Match "Get-Content$"

            $convertToJsonViolation = $violations | Where-Object { $_.Extent.Text -eq "ConvertTo-Json" }
            $convertToJsonViolation.SuggestedCorrections[0].Text | Should -Match "ConvertTo-Json$"
        }

        It "suggests different modules for different cmdlets" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = @'
Get-Command
Write-Host "test"
Get-ChildItem
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
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
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations[0].Severity | Should -Be ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticSeverity]::Warning)
        }

        It "has correct rule name" {
            $settings = @{
                Rules = @{
                    PSUseFullyQualifiedCmdletNames = @{
                        Enable = $true
                    }
                }
            }
            $scriptDefinition = 'Get-Command'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings -IncludeRule $violationName
            $violations[0].RuleName | Should -Be $violationName
        }
    }
}