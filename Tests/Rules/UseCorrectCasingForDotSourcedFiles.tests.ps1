# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "UseCorrectCasingForDotSourcedFiles" {
    BeforeAll {
        $settings = @{
            IncludeRules = @('PSUseCorrectCasingForDotSourcedFiles')
            Rules        = @{ PSUseCorrectCasingForDotSourcedFiles = @{ Enable = $true } }
        }

        function NewWorkload {
            param([string[]]$FileNames, [string]$CallerContent)

            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            foreach ($fileName in $FileNames) {
                Set-Content -Path (Join-Path $root $fileName) -Value 'function Get-Something { }' -Encoding utf8
            }
            $callerPath = Join-Path $root 'Caller.ps1'
            Set-Content -Path $callerPath -Value $CallerContent -Encoding utf8
            return $callerPath
        }
    }

    Context "`$PSScriptRoot-relative path" {
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". `$PSScriptRoot${Separator}HELPERS.ps1"

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].Message | Should -Match 'HELPERS\.ps1'
            $diagnostics[0].Message | Should -Match 'Helpers\.ps1'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". `$PSScriptRoot${Separator}Helpers.ps1"

            Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
        }
    }

    Context "Relative path" {
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". .${Separator}HELPERS.ps1"

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". .${Separator}Helpers.ps1"

            Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
        }

        It "flags a dot-sourced path with no leading separator (bare file name)" {
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent '. HELPERS.ps1'

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }
    }

    Context "Absolute path" {
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent 'PLACEHOLDER'
            $root = Split-Path $callerPath -Parent
            $wrongCasedAbsolutePath = "$root${Separator}HELPERS.ps1"
            Set-Content -Path $callerPath -Value ". '$wrongCasedAbsolutePath'" -Encoding utf8

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases @(
            @{ Separator = '\' }
            @{ Separator = '/' }
        ) {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent 'PLACEHOLDER'
            $root = Split-Path $callerPath -Parent
            $correctlyCasedAbsolutePath = "$root${Separator}Helpers.ps1"
            Set-Content -Path $callerPath -Value ". '$correctlyCasedAbsolutePath'" -Encoding utf8

            Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
        }
    }

    It "does not flag a dot-sourced path that does not resolve to any file" {
        $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent '. $PSScriptRoot\DoesNotExist.ps1'

        Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
    }

    It "does not flag a dot-sourced path built from a dynamic expression" {
        $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent '$name = "HELPERS"; . "$PSScriptRoot\$name.ps1"'

        Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
    }
}
