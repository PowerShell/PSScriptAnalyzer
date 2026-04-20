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

        It "Should return true" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath | Should -BeTrue
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

        It "Should return true" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath | Should -BeTrue
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
            $content = @"
@{
    IncludeRules = @(
        'PSBogusRuleThatDoesNotExist'
    )
}
"@
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should write a non-terminating error and produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
        }

        It "Should report the unknown rule name in the error" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $errs[0].Exception.Message | Should -BeLike "IncludeRules: rule 'PSBogusRuleThatDoesNotExist' not found.*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid rule option name" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-option.psd1'
            $content = @"
@{
    IncludeRules = @('PSUseConsistentIndentation')
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = `$true
            CompletelyBogusOption = 42
        }
    }
}
"@
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should write a non-terminating error and produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
        }

        It "Should report the unrecognised option in the error" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $errs[0].Exception.Message | Should -BeLike "Rules.PSUseConsistentIndentation.CompletelyBogusOption: unrecognised option.*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid rule option value" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-value.psd1'
            $content = @"
@{
    IncludeRules = @('PSUseConsistentIndentation')
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = `$true
            Kind = 'banana'
        }
    }
}
"@
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should write a non-terminating error and produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
        }

        It "Should report the invalid value in the error" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $errs[0].Exception.Message | Should -BeLike "Rules.PSUseConsistentIndentation.Kind: 'banana' is not a valid value.*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with an invalid severity value" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'bad-severity.psd1'
            $content = @"
@{
    Severity = @('Critical')
}
"@
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should write a non-terminating error and produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
        }

        It "Should report the invalid severity in the error" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $errs[0].Exception.Message | Should -BeLike "Severity: 'Critical' is not a valid severity.*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }

    Context "Given a file with wildcard rule names in IncludeRules" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'wildcard.psd1'
            $content = @"
@{
    IncludeRules = @('PSDSC*')
}
"@
            Set-Content -Path $settingsPath -Value $content
        }

        It "Should return true - wildcards are valid" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath | Should -BeTrue
        }
    }

    Context "Given an unparseable file" {
        BeforeAll {
            $settingsPath = Join-Path $TestDrive 'broken.psd1'
            Set-Content -Path $settingsPath -Value 'this is not valid psd1 content {{{'
        }

        It "Should write a non-terminating error and produce no output" {
            $result = Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $result | Should -BeNullOrEmpty
            $errs | Should -Not -BeNullOrEmpty
        }

        It "Should report the parse failure in the error" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -ErrorVariable errs -ErrorAction SilentlyContinue
            $errs[0].Exception.Message | Should -BeLike "Failed to parse settings file:*"
        }

        It "Should return false with -Quiet" {
            Test-ScriptAnalyzerSettingsFile -Path $settingsPath -Quiet | Should -BeFalse
        }
    }
}
