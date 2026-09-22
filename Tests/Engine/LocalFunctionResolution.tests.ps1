# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Commands defined by the analyzed script resolve locally" {
    BeforeAll {
        $casingSettings = @{
            IncludeRules = @('PSUseCorrectCasing')
            Rules = @{ PSUseCorrectCasing = @{ Enable = $true; CheckKeyword = $false; CheckOperator = $false } }
        }
    }

    Context "Parameter metadata" {
        It "validates against the real cmdlet when the script does not define it" {
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition "Get-ChildItem -PATH 'x'" -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "does not validate against a cmdlet the script shadows" {
            $scriptDefinition = @'
function Get-ChildItem { param($PATH) }
Get-ChildItem -PATH 'x'
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "does not validate against a cmdlet shadowed by a nested function" {
            $scriptDefinition = @'
function Outer {
    function Get-ChildItem { param($PATH) }
    Get-ChildItem -PATH 'x'
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "still validates a call outside the function that a nested definition shadows" {
            # Regression test: a function nested inside Outer is only in scope for calls made from
            # within Outer's body. A call at the top level must still resolve against the real cmdlet.
            $scriptDefinition = @'
function Outer {
    function Get-ChildItem { param($PATH) }
}
Get-ChildItem -PATH 'x'
'@
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }

        It "still validates a call in a sibling function that a nested definition shadows" {
            $scriptDefinition = @'
function Outer {
    function Get-ChildItem { param($PATH) }
}
function Sibling {
    Get-ChildItem -PATH 'x'
}
'@
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Path'
        }
    }

    Context "Command name casing" {
        It "corrects a cmdlet the script does not define" {
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition 'get-childitem' -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Get-ChildItem'
        }

        It "leaves a command the script defines itself alone" {
            $scriptDefinition = @'
function get-childitem { }
get-childitem
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings | Should -BeNullOrEmpty
        }

        It "does not reformat a command the script defines itself" {
            $scriptDefinition = "function get-childitem { }`nget-childitem"
            Invoke-Formatter $scriptDefinition | Should -BeExactly $scriptDefinition
        }
    }

    Context "Mandatory parameters" {
        It "reports a missing mandatory parameter on the real cmdlet" {
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition 'Write-Warning' -IncludeRule PSUseCmdletCorrectly)
            $diagnostics.Count | Should -Be 1
        }

        It "does not report mandatory parameters of a cmdlet the script shadows" {
            $scriptDefinition = @'
function Write-Warning { }
Write-Warning
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule PSUseCmdletCorrectly |
                Should -BeNullOrEmpty
        }
    }

    Context "Isolation" {
        It "does not suppress lookups for commands the script does not define" {
            $scriptDefinition = @'
function Get-MyThing { }
get-childitem
'@
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Get-ChildItem'
        }

        It "does not leak the definitions of one analysis into the next" {
            $null = Invoke-ScriptAnalyzer -ScriptDefinition "function get-childitem { }`nget-childitem" -Settings $casingSettings
            $diagnostics = @(Invoke-ScriptAnalyzer -ScriptDefinition 'get-childitem' -Settings $casingSettings)
            $diagnostics.Count | Should -Be 1
            $diagnostics[0].SuggestedCorrections[0].Text | Should -BeExactly 'Get-ChildItem'
        }
    }
}
