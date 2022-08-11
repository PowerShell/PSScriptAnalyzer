# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $settings = @{
        IncludeRules = @('PSAvoidUsingBrokenHashAlgorithms')
        Rules        = @{
            PSAvoidUsingBrokenHashAlgorithms = @{
                Enable = $true
            }
        }
    }

    $violationMessageMD5 = [regex]::Escape("The Algorithm parameter of cmdlet 'Get-FileHash' was used with the broken algorithm 'MD5'.")
    $violationMessageSHA1 = [regex]::Escape("The Algorithm parameter of cmdlet 'Get-FileHash' was used with the broken algorithm 'SHA1'.")
}

Describe "AvoidUsingBrokenHashAlgorithms" {
    Context "When there are violations" {
        It "detects broken hash algorithms violations" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm MD5' -Settings $settings).Count | Should -Be 1
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm SHA1' -Settings $settings).Count | Should -Be 1
        }

        It "has the correct description message for MD5" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm MD5' -Settings $settings).Message | Should -Match $violationMessageMD5
        }

        It "has the correct description message for SHA-1" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm SHA1' -Settings $settings).Message | Should -Match $violationMessageSHA1
        }

        It "detects arbitrary cmdlets" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Fake-Cmdlet foo -Algorithm MD5' -Settings $settings).Count | Should -Be 1
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Fake-Cmdlet foo -Algorithm SHA1' -Settings $settings).Count | Should -Be 1
        }

    }

    Context "When there are no violations" {
        It "does not flag default algorithm" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo' -Settings $settings).Count | Should -Be 0
        }
        
        It "does not flag safe algorithms" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm SHA256' -Settings $settings).Count | Should -Be 0
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm SHA384' -Settings $settings).Count | Should -Be 0
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Get-FileHash foo -Algorithm SHA512' -Settings $settings).Count | Should -Be 0
        }
    }
}