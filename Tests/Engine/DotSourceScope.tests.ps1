# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Functions reach across the files of a dot-sourced group" {
    BeforeAll {
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules        = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }

        function NewWorkload {
            param([hashtable]$Files)

            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            foreach ($relativePath in $Files.Keys) {
                $fullPath = Join-Path $root $relativePath
                $null = New-Item -Path (Split-Path $fullPath -Parent) -ItemType Directory -Force
                Set-Content -Path $fullPath -Value $Files[$relativePath] -Encoding utf8
            }
            return $root
        }
    }

    Context "Files sharing a root" {
        It "sees a function defined by a file the same root dot-sourced" {
            # Library files call each other without dot-sourcing; they share a session state only
            # because the root sourced them both.
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Definition.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "follows a chain of dot-sources" {
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Middle.ps1'     = ". `$PSScriptRoot/Definition.ps1"
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Middle.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }
    }

    Context "Files that only share a dependency" {
        It "does not see a function from another root that happens to share a dot-sourced file" {
            # Both roots source Shared.ps1, but that does not put the first root's functions in
            # the second's session state. Merging the two groups would hide this diagnostic.
            $root = NewWorkload @{
                'lib/Shared.ps1'     = 'function Get-Shared { }'
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'RootOne.ps1'        = ". `$PSScriptRoot/lib/Shared.ps1`n. `$PSScriptRoot/lib/Definition.ps1"
                'RootTwo.ps1'        = ". `$PSScriptRoot/lib/Shared.ps1`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not see a function from an unrelated file in the same run" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Unrelated.ps1'  = "Get-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not carry scope into a later single-file analysis" {
            $root = NewWorkload @{
                'lib/Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'lib/Caller.ps1'     = "Get-ChildItem -PATH 'x'"
                'Root.psm1'          = ". `$PSScriptRoot/lib/Definition.ps1`n. `$PSScriptRoot/lib/Caller.ps1"
            }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty

            $caller = Join-Path $root 'lib/Caller.ps1'
            $diagnostics = @(Invoke-ScriptAnalyzer -Path $caller -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }
    }

    Context "Case-sensitive filesystems" {
        It "does not collapse files that differ only by case" -Skip:(-not $IsLinux) {
            # Regression test: on a case-sensitive filesystem (e.g. Linux ext4), 'A.ps1' and 'a.ps1'
            # are distinct files. If they were folded into a single dictionary entry (case-insensitively),
            # 'a.ps1' could incorrectly inherit 'A.ps1's function definitions even though it never
            # dot-sources it, and its own call would go unreported.
            # PowerShell hashtable literals are case-insensitive, so the two files are written directly
            # rather than through NewWorkload's [hashtable]$Files parameter.
            $root = Join-Path $TestDrive ([System.IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory -Force
            Set-Content -Path (Join-Path $root 'A.ps1') -Value 'function Get-ChildItem { param($PATH) }' -Encoding utf8
            Set-Content -Path (Join-Path $root 'a.ps1') -Value "Get-ChildItem -PATH 'x'" -Encoding utf8

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'a.ps1')
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }
    }

    Context "Dot-source targets that cannot be resolved" {
        It "ignores a target built from a variable" {
            $root = NewWorkload @{
                'Definition.ps1' = 'function Get-ChildItem { param($PATH) }'
                'Caller.ps1'     = ". `$someFile`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "ignores a target outside the analyzed set" {
            $root = NewWorkload @{
                'Caller.ps1' = ". `$PSScriptRoot/DoesNotExist.ps1`nGet-ChildItem -PATH 'x'"
            }

            $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not loop on a dot-source cycle" {
            $root = NewWorkload @{
                'First.ps1'  = ". `$PSScriptRoot/Second.ps1`nfunction Get-ChildItem { param(`$PATH) }"
                'Second.ps1' = ". `$PSScriptRoot/First.ps1`nGet-ChildItem -PATH 'x'"
            }

            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
        }
    }

    Context "Files rewritten during the run" {
        It "analyzes the fixed content rather than the content of the pre-pass" {
            $root = NewWorkload @{
                'Fixable.ps1' = 'gci'
            }

            $aliasSettings = @{ IncludeRules = @('PSAvoidUsingCmdletAliases') }
            Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $aliasSettings -Fix | Should -BeNullOrEmpty
            (Get-Content (Join-Path $root 'Fixable.ps1') -Raw).Trim() | Should -BeExactly 'Get-ChildItem'
        }
    }
}
