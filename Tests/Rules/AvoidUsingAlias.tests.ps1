# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "'cls' is an alias of 'Clear-Host'. Alias can introduce possible problems and make scripts hard to maintain. Please consider changing alias to its full content."
    $violationName = "PSAvoidUsingCmdletAliases"
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    $violationFilepath = Join-Path $PSScriptRoot 'AvoidUsingAlias.ps1'
    $violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingAliasNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
}

Describe "AvoidUsingAlias" {
    Context "When there are violations" {
        It "has 2 Avoid Using Alias Cmdlets violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }

        It "suggests correction" {
            Test-CorrectionExtent $violationFilepath $violations[0] 1 'iex' 'Invoke-Expression'
            $violations[0].SuggestedCorrections[0].Description | Should -Be 'Replace iex with Invoke-Expression'

            Test-CorrectionExtent $violationFilepath $violations[1] 1 'cls' 'Clear-Host'
            $violations[1].SuggestedCorrections[0].Description | Should -Be 'Replace cls with Clear-Host'
        }

        It "warns when 'Get-' prefix was omitted" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'verb' | Where-Object { $_.RuleName -eq $violationName }
            $violations.Count | Should -Be 1
        }
    }

    Context "Violation Extent" {
        It "should return only the cmdlet extent" {
            $target = @'
gci -Path C:\
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName
            $violations[0].Extent.Text | Should -Be "gci"
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        It "should return no violation for assignment statement-like command in dsc configuration" -skip:($IsLinux -or $IsMacOS) {
            $target = @'
Configuration MyDscConfiguration {
    Node "NodeA" {
        SomeResource MyResourceInstance {
            Type = "Present"
            Name =    "RSAT"
        }
    }
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName | `
                Get-Count | `
                Should -Be 0
        }
    }

    Context "Settings file provides whitelist" {
        BeforeAll {
            $whiteListTestScriptDef = 'gci; cd;'
            $settings = @{
                'Rules' = @{
                    'PSAvoidUsingCmdletAliases' = @{
                        'Whitelist' = @('cd')
                    }
                }
            }
        }

        It "honors the whitelist provided as hashtable" {
            $settings = @{
                'Rules' = @{
                    'PSAvoidUsingCmdletAliases' = @{
                        'Whitelist' = @('cd')
                    }
                }
            }
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $whiteListTestScriptDef -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 1
        }

        It "honors the whitelist provided through settings file" {
            # even though join-path returns string, if we do not use tostring, then invoke-scriptanalyzer cannot cast it to string type
            $settingsFilePath = (Join-Path $PSScriptRoot (Join-Path 'TestSettings' 'AvoidAliasSettings.psd1')).ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $whiteListTestScriptDef -Settings $settingsFilePath -IncludeRule $violationName
            $violations.Count | Should -Be 1
        }

        It "honors the whitelist in a case-insensitive manner" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition "CD" -Settings $settings -IncludeRule $violationName
            $violations.Count | Should -Be 0
        }

        It "do not warn when about Get-* completed cmdlets when the command exists natively on Unix platforms" -skip:(-not ($IsLinux -or $IsMacOS)) {
            $nativeLinuxCommand = 'date'
            $dateCommand = Get-Command $nativeLinuxCommand -CommandType Application -ErrorAction Ignore

            $violations = Invoke-ScriptAnalyzer -IncludeRule PSAvoidUsingCmdletAliases -ExcludeRule PSUseCompatibleCommands -ScriptDefinition $nativeLinuxCommand | Where-Object { $_.RuleName -eq $violationName }

            $expectedViolations = 1
            if ($dateCommand)
            {
                $expectedViolations = 0
            }

            $violations.Count | Should -Be $expectedViolations
        }
    }
}
