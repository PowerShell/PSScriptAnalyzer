# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe 'Scope-qualified function declarations' {
    BeforeAll {
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }
    }

    It 'recognizes a <Scope> declaration in its defining scope' -TestCases @(
        @{ Scope = 'script' }, @{ Scope = 'global' }, @{ Scope = 'local' }, @{ Scope = 'private' }
    ) {
        param($Scope)
        $definition = 'function ' + $Scope + ':Get-ChildItem { param($PATH) }' + "`nGet-ChildItem -PATH 'x'"
        Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'recognizes a nested <Scope> declaration outside the defining function' -TestCases @(
        @{ Scope = 'script' }, @{ Scope = 'global' }
    ) {
        param($Scope)
        $definition = 'function Outer { function ' + $Scope + ':Get-ChildItem { param($PATH) } }' + "`nOuter`nGet-ChildItem -PATH 'x'"
        Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'keeps a nested local declaration inside its containing function' {
        $definition = @'
function Outer {
    function local:Get-ChildItem { param($PATH) }
    Get-ChildItem -PATH 'inside'
}
Get-ChildItem -PATH 'outside'
'@
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].Extent.Text | Should -BeExactly '-PATH'
        $diagnostics[0].Extent.StartLineNumber | Should -Be 5
        $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
    }

    It 'does not inherit a private declaration into a child function' {
        $definition = @'
function private:Get-ChildItem { param($PATH) }
Get-ChildItem -PATH 'same'
function Child { Get-ChildItem -PATH 'child' }
'@
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].Extent.StartLineNumber | Should -Be 3
        $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
    }

    It 'keeps a nested private declaration in its exact defining scope' {
        $definition = @'
function Outer {
    function private:Get-ChildItem { param($PATH) }
    Get-ChildItem -PATH 'same'
    function Child { Get-ChildItem -PATH 'child' }
}
Get-ChildItem -PATH 'outside'
'@
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings)
        $diagnostics.Count | Should -Be 2
        @($diagnostics.Extent.StartLineNumber | Sort-Object) | Should -Be @(4, 6)
    }

    It 'preserves casing when formatting a scoped local definition' {
        $definition = "function ScRiPt:get-childitem { param(`$PATH) }`nget-childitem -PATH 'x'"
        Invoke-Formatter -ScriptDefinition $definition -Settings $casingSettings | Should -BeExactly $definition
    }

    It 'does not borrow mandatory parameters from a cmdlet shadowed by a scoped function' {
        Invoke-ScriptAnalyzer -ScriptDefinition "function script:Write-Warning { }`nWrite-Warning" -IncludeRule PSUseCmdletCorrectly | Should -BeNullOrEmpty
    }

    It 'does not inherit a private declaration into a directly invoked child block' {
        $definition = "function private:Get-ChildItem { param(`$PATH) }`nGet-ChildItem -PATH 'same'`n& { Get-ChildItem -PATH 'child' }"
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].Extent.StartLineNumber | Should -Be 3
    }

    It 'sees a private declaration from a dot-sourced block in the same scope' {
        $definition = "function private:Get-ChildItem { param(`$PATH) }`n. { Get-ChildItem -PATH 'same' }"
        Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'sees a private declaration from a ForEach-Object block in the same scope' {
        $definition = "function private:Get-ChildItem { param(`$PATH) }`n1 | ForEach-Object { Get-ChildItem -PATH 'same' }"
        Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'keeps an ampersand command argument block in the caller scope' {
        $definition = "function private:Get-ChildItem { param(`$PATH) }`n1 | & ForEach-Object { Get-ChildItem -PATH 'same' }"
        Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings | Should -BeNullOrEmpty
    }

    It 'still validates a module-qualified cmdlet despite a scoped local declaration' {
        $definition = "function script:Get-ChildItem { param(`$PATH) }`nMicrosoft.PowerShell.Management\Get-ChildItem -PATH 'x'"
        $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $definition -Settings $casingSettings)
        $diagnostics.Count | Should -Be 1
        $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
    }
}
