# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Test-ScriptAnalyzerSettingsFile" {
    Context "Given a valid generated settings file" {
        BeforeAll {
            $testDir = Join-Path $TestDrive 'valid'
            New-Item -ItemType Directory -Path $testDir | Out-Null
            New-ScriptAnalyzerSettingsFile -Path $testDir
            $settingsPath = Join-Path $testDir 'PSScriptAnalyzerSettings.psd1'
        }

        It "Should produce no output when the file is valid" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }

        It "Should return true with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeTrue
        }
    }

    Context "Given a valid preset-based settings file" {
        BeforeAll {
            $testDir = Join-Path $TestDrive 'preset'
            New-Item -ItemType Directory -Path $testDir | Out-Null
            New-ScriptAnalyzerSettingsFile -Path $testDir -BaseOnPreset CodeFormatting
            $settingsPath = Join-Path $testDir 'PSScriptAnalyzerSettings.psd1'
        }

        It "Should produce no output when the file is valid" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }

        It "Should return true with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeTrue
        }
    }

    Context "Given a file that does not exist" {
        It "Should write a non-terminating error and produce no output" {
            $bogusPath = Join-Path $TestDrive 'nonexistent.psd1'
            $result = Test-ScriptAnalyzerSettingsFile -Path $bogusPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
            $errs[0].FullyQualifiedErrorId | Should -BeLike 'SettingsFileNotFound*'
        }

        It "Should return false with -Quiet" {
            $bogusPath = Join-Path $TestDrive 'nonexistent.psd1'
            Test-ScriptAnalyzerSettingsFile -Path $bogusPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an unknown rule name" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'unknown-rule.psd1'
            $content = "
                @{
                    IncludeRules = @(
                        'PSBogusRuleThatDoesNotExist'
                    )
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0] | Should -BeOfType ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord])
        }

        It "Should report the unknown rule name in the message" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Message | Should -BeLike "*PSBogusRuleThatDoesNotExist*"
        }

        It "Should include an extent pointing to the offending text" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Extent | Should -Not -BeNullOrEmpty
            $result[0].Extent.Text | Should -Be "'PSBogusRuleThatDoesNotExist'"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid rule option name" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-option.psd1'
            $content = "
                @{
                    IncludeRules = @('PSUseConsistentIndentation')
                    Rules = @{
                        PSUseConsistentIndentation = @{
                            Enable = `$true
                            CompletelyBogusOption = 42
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0] | Should -BeOfType ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord])
        }

        It "Should report the unrecognised option in the message" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Message | Should -BeLike "*CompletelyBogusOption*unrecognised option*"
        }

        It "Should include an extent pointing to the option name" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Extent.Text | Should -Be 'CompletelyBogusOption'
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid rule option value" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-value.psd1'
            $content = "
                @{
                    IncludeRules = @('PSUseConsistentIndentation')
                    Rules = @{
                        PSUseConsistentIndentation = @{
                            Enable = `$true
                            Kind = 'banana'
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0] | Should -BeOfType ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord])
        }

        It "Should report the invalid value in the message" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Message | Should -BeLike "*banana*not a valid value*"
        }

        It "Should include an extent pointing to the bad value" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Extent.Text | Should -Be "'banana'"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid severity value" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-severity.psd1'
            $content = "
                @{
                    Severity = @('Critical')
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0] | Should -BeOfType ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord])
        }

        It "Should report the invalid severity in the message" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Message | Should -BeLike "*Critical*not a valid severity*"
        }

        It "Should include an extent pointing to the bad value" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result[0].Extent.Text | Should -Be "'Critical'"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with wildcard rule names in IncludeRules" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'wildcard.psd1'
            $content = "
                @{
                    IncludeRules = @('PSDSC*')
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should produce no output - wildcards are valid" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Given an unparseable file" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'broken.psd1'
            Set-Content -Path $settingsPath -Value 'this is not valid psd1 content {{{'
        }

        It "Should output DiagnosticRecord objects with parse errors" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0] | Should -BeOfType ([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord])
            $result[0].Severity | Should -Be 'ParseError'
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "DiagnosticRecord properties" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'diag-props.psd1'
            $content = "
                @{
                    Severity = @('Critical')
                }
            "
            Set-Content -Path $settingsPath -Value $content
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
        }

        It "Should set RuleName to Test-ScriptAnalyzerSettingsFile" {
            $result[0].RuleName | Should -Be 'Test-ScriptAnalyzerSettingsFile'
        }

        It "Should set ScriptPath to the settings file path" {
            $result[0].ScriptPath | Should -Be $settingsPath
        }

        It "Should set Severity to Error for validation problems" {
            $result[0].Severity | Should -Be 'Error'
        }

        It "Should include line number information in the extent" {
            $result[0].Extent.StartLineNumber | Should -BeGreaterThan 0
        }
    }

    Context "Given a file with a wrong type for a bool option" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-bool.psd1'
            $content = "
                @{
                    Rules = @{
                        PSUseConsistentIndentation = @{
                            Enable = 123
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord for the type mismatch" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0].Message | Should -BeLike "*Enable*expected a value of type bool*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with a string where an int is expected" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-int.psd1'
            $content = "
                @{
                    Rules = @{
                        PSUseConsistentIndentation = @{
                            Enable = `$true
                            IndentationSize = 'abc'
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should output a DiagnosticRecord for the type mismatch" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0].Message | Should -BeLike "*IndentationSize*expected a value of type int*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with a string where a string array is expected" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-array.psd1'
            $content = "
                @{
                    Rules = @{
                        PSUseSingularNouns = @{
                            NounAllowList = 'Data'
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should accept a single string for a string array property" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Given a file with valid types for all options" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'valid-types.psd1'
            $content = "
                @{
                    Rules = @{
                        PSUseConsistentIndentation = @{
                            Enable = `$true
                            IndentationSize = 4
                        }
                    }
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Given a file with IncludeDefaultRules" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'include-defaults.psd1'
            $content = "
                @{
                    IncludeDefaultRules = `$true
                    IncludeRules = @('PSUseConsistentIndentation')
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should validate built-in rules when IncludeDefaultRules is true" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }

        It "Should return true with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeTrue
        }
    }

    Context "Given a file with CustomRulePath pointing to community rules" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'custom-rules.psd1'
            $communityRulesPath = Join-Path $PSScriptRoot 'CommunityAnalyzerRules'
            $content = "
                @{
                    CustomRulePath = @('$communityRulesPath')
                    IncludeDefaultRules = `$true
                    IncludeRules = @('PSUseConsistentIndentation', 'Measure-RequiresModules')
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should validate both built-in and custom rule names" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath
            $result | Should -BeNullOrEmpty
        }

        It "Should return true with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeTrue
        }
    }

    Context "Given a file with CustomRulePath but without IncludeDefaultRules" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'custom-no-defaults.psd1'
            $communityRulesPath = Join-Path $PSScriptRoot 'CommunityAnalyzerRules'
            $content = "
                @{
                    CustomRulePath = @('$communityRulesPath')
                    IncludeRules = @('PSUseConsistentIndentation')
                }
            "
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should report built-in rules as unknown when IncludeDefaultRules is not set" {
            $result = @(Test-ScriptAnalyzerSettingsFile -Path $settingsPath)
            $result.Count | Should -BeGreaterThan 0
            $result[0].Message | Should -BeLike "*PSUseConsistentIndentation*"
        }
    }
}
