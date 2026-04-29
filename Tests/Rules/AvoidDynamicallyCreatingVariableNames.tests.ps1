# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
[Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidUsingCmdletAliases', 'nv', Justification = 'For test purposes')]
param()

BeforeAll {
    $ruleName = "PSAvoidDynamicallyCreatingVariableNames"
    $ruleMessage = "'{0}' is a dynamic variable name. Please, avoid creating variables with a dynamic name"
}

Describe "AvoidDynamicallyCreatingVariableNames" {
    Context "Violates" {
        It "Basic dynamic variable name" {
            $scriptDefinition = { New-Variable -Name $Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Information
            $violations.Extent.Text | Should -Be {New-Variable -Name $Test}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f '$Test')
        }

        It "Using alias" {
            $scriptDefinition = { nv -Name $Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Information
            $violations.Extent.Text | Should -Be {nv -Name $Test}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f '$Test')
        }

        It "Using uppercase" {
            $scriptDefinition = { NEW-VARIABLE -Name $Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Information
            $violations.Extent.Text | Should -Be {NEW-VARIABLE -Name $Test}.ToString()
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
            $violations.Severity    | Should -Be Information
            $violations.Extent.Text | Should -Be {New-Variable -Name "My$_" -Value ($i++)}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f 'My$_')
        }

        It "Unquoted positional binding" {
            $scriptDefinition = {
                $myVarName = 'foo'
                New-Variable $myVarName
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Information
            $violations.Extent.Text | Should -Be {New-Variable $myVarName}.ToString()
            $violations.Message     | Should -Be ($ruleMessage -f '$myVarName')
        }

        It "Quoted positional binding" {
            $scriptDefinition = {
                'One', 'Two', 'Three' | ForEach-Object -Begin { $i = 1 } -Process {
                    New-Variable "My$_" ($i++)
                }
                $MyTwo # returns 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count       | Should -Be 1
            $violations.Severity    | Should -Be Information
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
                New-Variable -Name '$Sign1'
                New-Variable -Name '$Sign2' -Value 'Dollar'
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppressed" {
        It "Basic dynamic variable name" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidDynamicallyCreatingVariableNames', '$Test', Justification = 'Test')]
                Param()
                New-Variable -Name $Test
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
        It "Common dynamic variable iteration" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidDynamicallyCreatingVariableNames', 'My$_', Justification = 'Test')]
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