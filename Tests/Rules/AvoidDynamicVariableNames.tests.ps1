# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSAvoidDynamicVariableNames"
    $ruleMessage = "'{0}' is a dynamic variable name. Please, avoid creating variables with a dynamic name"
}

Describe "AvoidDynamicVariableNames" {
    Context "Violates" {
        It "Basic dynamic variable name" {
            $scriptDefinition = { New-Variable -Name $Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Variable -Name $Test}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f '$Test')
        }

        It "Common dynamic variable iteration" {
            $scriptDefinition = {
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    New-Variable -Name "My$_" -Value ($i++)
                }
                $MyTwo # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Variable -Name "My$_" -Value ($i++)}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f 'My$_')
        }

        It "Set-Variable by positional parameter" {
            $scriptDefinition = {
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    New-Variable "My$_" ($i++)
                }
                $MyTwo # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Warning
            $violations.Extent.Text | Should -Be {New-Variable "My$_" ($i++)}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f 'My$_')
        }
    }

    Context "Compliant" {
        It "Common hash table population" {
            $scriptDefinition = {
                $My = @{}
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    $My[$_] = $i++
                }
                $My.Two # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Scoped hash table population" {
            $scriptDefinition = {
                New-Variable -Name My -Value @{} -Option ReadOnly -Scope Script
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    $Script:My[$_] = $i++
                }
                $Script:My.Two # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Verbatim (single quoted) name with dollar sign" {
            $scriptDefinition = {
                New-Variable -Name '$Sign'
                Set-Variable -Name '$Sign' -Value 'Dollar'
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppressed" {
        It "Basic dynamic variable name" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidDynamicVariableNames', '$Test', Justification = 'Test')]
                Param()
                New-Variable -Name $Test
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
        It "Common dynamic variable iteration" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidDynamicVariableNames', 'My$_', Justification = 'Test')]
                Param()
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    New-Variable -Name "My$_" -Value ($i++)
                }
                $MyTwo # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }
}