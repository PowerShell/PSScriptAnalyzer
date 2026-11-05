# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'Scoped declarations in dot-sourced groups' {
    BeforeAll {
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }
        function NewScopedWorkload {
            param([string]$Definition, [string]$Caller)
            $root = Join-Path $TestDrive ([IO.Path]::GetRandomFileName())
            $null = New-Item -Path $root -ItemType Directory
            Set-Content -LiteralPath (Join-Path $root 'Definition.ps1') -Value $Definition -Encoding utf8
            Set-Content -LiteralPath (Join-Path $root 'Caller.ps1') -Value $Caller -Encoding utf8
            Set-Content -LiteralPath (Join-Path $root 'Root.psm1') -Value ". `$PSScriptRoot/Definition.ps1`n. `$PSScriptRoot/Caller.ps1" -Encoding utf8
            return $root
        }
    }

    It 'recognizes a dot-sourced <Scope> declaration at file scope' -TestCases @(
        @{ Scope = 'script' }, @{ Scope = 'global' }, @{ Scope = 'local' }, @{ Scope = 'private' }
    ) {
        param($Scope)
        $root = NewScopedWorkload ('function ' + $Scope + ':Get-ChildItem { param($PATH) }') "Get-ChildItem -PATH 'x'"
        Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'does not inherit a dot-sourced private declaration into a child function' {
        $root = NewScopedWorkload 'function private:Get-ChildItem { param($PATH) }' "function Child { Get-ChildItem -PATH 'x' }"
        $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
    }

    It 'does not flatten an ordinary nested function into the dot-source group' {
        $root = NewScopedWorkload 'function Outer { function Get-ChildItem { param($PATH) } }' "Get-ChildItem -PATH 'x'"
        $diagnostics = @(Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].ScriptPath | Should -BeExactly (Join-Path $root 'Caller.ps1')
    }

    It 'shares declarations made by an ampersand command argument block in the caller scope' {
        $root = NewScopedWorkload '1 | & ForEach-Object { function Get-ChildItem { param($PATH) } }' "Get-ChildItem -PATH 'x'"
        Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'shares a nested <Scope> declaration with the dot-source group' -TestCases @(
        @{ Scope = 'script' }, @{ Scope = 'global' }
    ) {
        param($Scope)
        $root = NewScopedWorkload ('function Outer { function ' + $Scope + ':Get-ChildItem { param($PATH) } }') "Outer`nGet-ChildItem -PATH 'x'"
        Invoke-ScriptAnalyzer -Path $root -Recurse -Settings $casingSettings | Should -BeNullOrEmpty
    }
}
