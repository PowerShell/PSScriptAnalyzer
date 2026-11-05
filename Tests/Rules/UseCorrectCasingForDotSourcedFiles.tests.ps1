# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "UseCorrectCasingForDotSourcedFiles" {
    # PowerShell folds '\' to '/' for FileSystem paths on Linux and macOS, so a backslash-separated
    # path is valid and must be checked on every platform. Evaluated at discovery time
    # since -TestCases is bound during discovery.
    $separatorTestCases = @(@{ Separator = '\' }, @{ Separator = '/' })

    # File system case rules differ per platform and even per volume, so they are probed rather than
    # assumed from the OS:
    #   Windows: case-insensitive, case-preserving; '\' and '/' both separate path segments.
    #   Linux:   usually case-sensitive; 'Helpers.ps1' and 'helpers.ps1' can coexist and a wrong-case
    #            reference does not load at all. A literal '\' in a file name is unreachable from
    #            PowerShell because it is folded to '/'.
    #   macOS:   usually case-insensitive and case-preserving (APFS/HFS+), but case-sensitive
    #            volumes exist.
    # Tests that need two files differing only by case run only when this probe says the file system
    # is case-sensitive; the remaining tests hold on every file system.
    # Evaluated at discovery time because -Skip is bound during discovery.
    $caseSensitive = & {
        $probeDirectory = Join-Path ([IO.Path]::GetTempPath()) ([IO.Path]::GetRandomFileName())
        $null = New-Item -ItemType Directory -Path $probeDirectory
        try {
            $null = New-Item -ItemType File -Path (Join-Path $probeDirectory 'probe.txt')
            -not (Test-Path -LiteralPath (Join-Path $probeDirectory 'PROBE.TXT'))
        }
        finally { Remove-Item -LiteralPath $probeDirectory -Recurse -Force }
    }

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
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". `$PSScriptRoot${Separator}HELPERS.ps1"

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].Message | Should -Match 'HELPERS\.ps1'
            $diagnostics[0].Message | Should -Match 'Helpers\.ps1'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". `$PSScriptRoot${Separator}Helpers.ps1"

            Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
        }
    }

    Context "Relative path" {
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
            param($Separator)
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent ". .${Separator}HELPERS.ps1"

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].RuleName | Should -BeExactly 'PSUseCorrectCasingForDotSourcedFiles'
            $diagnostics[0].SuggestedCorrections[0].Text | Should -Match 'Helpers\.ps1'
        }

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
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
        It "flags a dot-sourced path whose casing does not match the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
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

        It "does not flag a dot-sourced path whose casing matches the file on disk (separator: '<Separator>')" -TestCases $separatorTestCases {
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

    Context "Correction span" {
        It "only corrects the final file name, not a directory that contains the same text" {
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path (Join-Path $root 'HELPERS.ps1.d') -ItemType Directory -Force
            Set-Content -Path (Join-Path $root 'HELPERS.ps1.d/Helpers.ps1') -Value 'function Get-Something { }'
            $callerPath = Join-Path $root 'Caller.ps1'
            Set-Content -Path $callerPath -Value '. "$PSScriptRoot/HELPERS.ps1.d/HELPERS.ps1"'

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly '"$PSScriptRoot/HELPERS.ps1.d/Helpers.ps1"'
        }

        It "offers no correction when escaping prevents locating the file name span" {
            $callerPath = NewWorkload -FileNames @('Helpers.ps1') -CallerContent '. "$PSScriptRoot/HELP`ERS.ps1"'

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)

            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections | Should -BeNullOrEmpty
        }
    }

    Context "Files that differ only by case on a case-sensitive file system" {
        It "does not flag an exact match when a differently-cased sibling is enumerated first" -Skip:(-not $caseSensitive) {
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            foreach ($name in 'Helpers.ps1', 'helpers.ps1') {
                Set-Content -LiteralPath (Join-Path $root $name) -Value 'function Get-Something { }'
            }
            foreach ($reference in 'Helpers.ps1', 'helpers.ps1') {
                $callerPath = Join-Path $root 'Caller.ps1'
                Set-Content -LiteralPath $callerPath -Value ". `$PSScriptRoot/$reference"
                Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings | Should -BeNullOrEmpty
            }
        }

        It "does not guess when several differently-cased files could match" -Skip:(-not $caseSensitive) {
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            foreach ($name in 'Helpers.ps1', 'helpers.ps1') {
                Set-Content -LiteralPath (Join-Path $root $name) -Value 'function Get-Something { }'
            }
            $callerPath = Join-Path $root 'Caller.ps1'
            Set-Content -LiteralPath $callerPath -Value '. $PSScriptRoot/HELPERS.ps1'

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $callerPath -Settings $settings)
            @($diagnostics | ? { $_.SuggestedCorrections }) | Should -BeNullOrEmpty
        }
    }
}
