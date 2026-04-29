# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSInvalidMultiDotValue"
    $ruleMessage = "The unquoted '{0}' expression is not a valid syntax. Types with multiple dots need to be constructed from either a quoted string or individual components."
    $correctionDescription = 'Quote the value that contains multiple dots'
}

Describe "InvalidMultiDotValue" {
    Context "Violates" {
        It "3 version components" {
            $scriptDefinition = { $version = 1.2.3 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Error
            $violations.Extent.Text                      | Should -Be '1.2.3'
            $violations.Message                          | Should -Be ($ruleMessage -f '1.2.3')
            $violations.RuleSuppressionID                | Should -Be '1.2.3'
            $violations.SuggestedCorrections.Text        | Should -Be "'1.2.3'"
            $violations.SuggestedCorrections.Description | Should -Be $correctionDescription
        }

        It "4 version components" {
            $scriptDefinition = { $version = 1.2.3.4 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Error
            $violations.Extent.Text                      | Should -Be '1.2.3.4'
            $violations.Message                          | Should -Be ($ruleMessage -f '1.2.3.4')
            $violations.RuleSuppressionID                | Should -Be '1.2.3.4'
            $violations.SuggestedCorrections.Text        | Should -Be "'1.2.3.4'"
            $violations.SuggestedCorrections.Description | Should -Be $correctionDescription
        }


        It "With class initializer" {
            $scriptDefinition = { $version = [Version]1.2.3 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Error
            $violations.Extent.Text                      | Should -Be '1.2.3'
            $violations.Message                          | Should -Be ($ruleMessage -f '1.2.3')
            $violations.RuleSuppressionID                | Should -Be '1.2.3'
            $violations.SuggestedCorrections.Text        | Should -Be "'1.2.3'"
            $violations.SuggestedCorrections.Description | Should -Be $correctionDescription
        }

        It "As parameter" {
            $scriptDefinition = {
                param(
                    [Version]$version = 1.2.3
                )
                Write-Verbose $version
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Error
            $violations.Extent.Text                      | Should -Be '1.2.3'
            $violations.Message                          | Should -Be ($ruleMessage -f '1.2.3')
            $violations.RuleSuppressionID                | Should -Be '1.2.3'
            $violations.SuggestedCorrections.Text        | Should -Be "'1.2.3'"
            $violations.SuggestedCorrections.Description | Should -Be $correctionDescription
        }

        # Even an IP address is apparently expect below.
        # The violation message and description presumes a version
        # is expected because this is more common used type.
        It "IP Address" {
            $scriptDefinition = { $IP = [System.Net.IPAddress]127.0.0.1 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Error
            $violations.Extent.Text                      | Should -Be '127.0.0.1'
            $violations.Message                          | Should -Be ($ruleMessage -f '127.0.0.1')
            $violations.RuleSuppressionID                | Should -Be '127.0.0.1'
            $violations.SuggestedCorrections.Text        | Should -Be "'127.0.0.1'"
            $violations.SuggestedCorrections.Description | Should -Be $correctionDescription
        }
    }

    Context "Compliant" {
        It "From string" {
            $scriptDefinition = { $Version = [Version]'1.2.3' }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "From version components" {
            $scriptDefinition = { $Version = [Version]::new(1, 2, 3, 4) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "From (bare) double" {
            $scriptDefinition = { $Version = [Version]1.2 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }


        It "Dot notation" { #PowerShell:27356
            $scriptDefinition = {
                $1.2.3.4
                $intKeys = @{ 1 = @{ 2 = @{ 3 = @{ 4 = 'test' } } } }
                $intKeys.1.2.3.4
             }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppressed" {
        It "All" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSInvalidMultiDotValue', '', Justification = 'Test')]
                param()
                $version = 1.2.3
                $IP = [System.Net.IPAddress]127.0.0.1
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "1.2.3" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSInvalidMultiDotValue', '1.2.3', Justification = 'Test')]
                param()
                $version = 1.2.3
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "127.0.0.1" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSInvalidMultiDotValue', '127.0.0.1', Justification = 'Test')]
                param()
                $IP = [System.Net.IPAddress]127.0.0.1
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Fixing" {

        BeforeAll { # See request: #1938
            $tempFile = Join-Path $TestDrive 'TestScript.ps1'
        }

        It "Version" {
            Set-Content -LiteralPath $tempFile -Value {$version = 1.2.3}.ToString() -NoNewLine
            $violations = Invoke-ScriptAnalyzer -Path $tempFile -fix
            Get-Content -LiteralPath $tempFile -Raw | Should -Be {$version = '1.2.3'}.ToString()
        }

        It "IP Address" {
            Set-Content -LiteralPath $tempFile -Value {$IP = [System.Net.IPAddress]127.0.0.1}.ToString() -NoNewLine
            $violations = Invoke-ScriptAnalyzer -Path $tempFile -fix
            Get-Content -LiteralPath $tempFile -Raw | Should -Be {$IP = [System.Net.IPAddress]'127.0.0.1'}.ToString()
        }
    }
}