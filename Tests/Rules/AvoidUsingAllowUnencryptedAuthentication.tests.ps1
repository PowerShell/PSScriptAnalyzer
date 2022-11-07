# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $settings = @{
        IncludeRules = @('PSAvoidUsingAllowUnencryptedAuthentication')
        Rules        = @{
            PSAvoidUsingAllowUnencryptedAuthentication = @{
                Enable = $true
            }
        }
    }
}

Describe "AvoidUsingAllowUnencryptedAuthentication" {
    Context "When there are violations" {
        It "detects unencrypted authentication violations" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Invoke-WebRequest foo -AllowUnencryptedAuthentication' -Settings $settings).Count | Should -Be 1
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Invoke-RestMethod foo -AllowUnencryptedAuthentication' -Settings $settings).Count | Should -Be 1
            (Invoke-ScriptAnalyzer -ScriptDefinition 'iwr foo -AllowUnencryptedAuthentication' -Settings $settings).Count | Should -Be 1
        }

        It "detects arbitrary cmdlets" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Invoke-CustomWebRequest foo -AllowUnencryptedAuthentication' -Settings $settings).Count | Should -Be 1
        }

    }

    Context "When there are no violations" {
        It "does not flag safe usage" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Invoke-WebRequest foo' -Settings $settings).Count | Should -Be 0
        }

        It "does not flag cases with unrelated parameters" {
            (Invoke-ScriptAnalyzer -ScriptDefinition 'Invoke-WebRequest foo -Method Get' -Settings $settings).Count | Should -Be 0
        }
    }
}