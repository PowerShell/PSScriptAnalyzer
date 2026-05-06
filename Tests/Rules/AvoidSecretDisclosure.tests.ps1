# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSAvoidSecretDisclosure"
    $ruleMessage = "Avoid disclosing a secret"
}

Describe "AvoidSecretDisclosure" {

    # Secret disclosure examples we like to discourage:
    # https://stackoverflow.com/questions/28352141/convert-a-secure-string-to-plain-text
    # https://stackoverflow.com/questions/7468389/powershell-decode-system-security-securestring-to-readable-password

    Context "Violates" {
        It "ConvertFrom-SecureString -AsPlainText" {
            $scriptDefinition = {
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $Null = $SecureString | ConvertFrom-SecureString -AsPlainText
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be {ConvertFrom-SecureString -AsPlainText}.ToString()
            $violations.Message           | Should -Be $ruleMessage
            $violations.RuleSuppressionID | Should -Be 'AsPlainText'
        }

        It "SecureStringToBSTR()" {
            $scriptDefinition = {
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
                $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
                [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be {[System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)}.ToString()
            $violations.Message           | Should -Be $ruleMessage
            $violations.RuleSuppressionID | Should -Be 'SecureStringToBSTR'
        }

        It "SecureStringToCoTaskMemUnicode()" {
            $scriptDefinition = {
                $password = ConvertTo-SecureString 'P@ssw0rd' -AsPlainText -Force
                $Ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password)
                $result = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($Ptr)
                [System.Runtime.InteropServices.Marshal]::ZeroFreeCoTaskMemUnicode($Ptr)
                $result
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be {[System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password)}.ToString()
            $violations.Message           | Should -Be $ruleMessage
            $violations.RuleSuppressionID | Should -Be 'SecureStringToCoTaskMemUnicode'
        }

        It "GetNetworkCredential().Password" {
            $scriptDefinition = {
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $PSCredential = [PSCredential]::new(0, $SecureString)
                $Credential = $PSCredential.GetNetworkCredential()
                $Password = $Credential.Password
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be {$Credential.Password}.ToString()
            $violations.Message           | Should -Be $ruleMessage
            $violations.RuleSuppressionID | Should -Be 'Password'
        }

        It "Custom password property" {
            $scriptDefinition = {
                $Cred = ConvertFrom-Json '
                {
                    "Account": {
                        "password": "Welcome123!",
                        "username": "JohnDoe"
                    }
                }'
                schtasks /change /s $env:COMPUTERNAME /tn $myTask  /ru $Cred.username /rp $Cred.password
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations.Count             | Should -Be 1
            $violations.Severity          | Should -Be Warning
            $violations.Extent.Text       | Should -Be {$Cred.password}.ToString()
            $violations.Message           | Should -Be $ruleMessage
            $violations.RuleSuppressionID | Should -Be 'Password'
        }
    }

    Context "Compliant" {
        It "Correct" {
            $scriptDefinition = {
                $credential = Get-Credential
                $url = "https://server.contoso.com:8089/services/search/jobs/export"
                $body = @{
                    search = "search index=_internal | reverse | table index,host,source,sourceType,_raw"
                    output_mode = "csv"
                    earliest_time = "-2d@d"
                    latest_time = "-1d@d"
                }
                Invoke-RestMethod -Method 'Post' -Uri $url -Credential $credential -Body $body -OutFile output.csv
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Write-Host" {
            $scriptDefinition = {
                Write-Host AsPlainText SecureStringToBSTR Password
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppressed" {
        It "AsPlainText" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidSecretDisclosure', 'AsPlainText', Justification = 'Test')]
                Param()
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $Null = $SecureString | ConvertFrom-SecureString -AsPlainText
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "SecureStringToBSTR" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidSecretDisclosure', 'SecureStringToBSTR', Justification = 'Test')]
                Param()
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
                $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
                [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "SecureStringToCoTaskMemUnicode" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidSecretDisclosure', 'SecureStringToCoTaskMemUnicode', Justification = 'Test')]
                Param()
                $password = ConvertTo-SecureString 'P@ssw0rd' -AsPlainText -Force
                $Ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password)
                $result = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($Ptr)
                [System.Runtime.InteropServices.Marshal]::ZeroFreeCoTaskMemUnicode($Ptr)
                $result
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "Password" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidSecretDisclosure', 'Password', Justification = 'Test')]
                Param()
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $PSCredential = [PSCredential]::new(0, $SecureString)
                $Password = $PSCredential.GetNetworkCredential().Password
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }

        It "All" {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidSecretDisclosure', '', Justification = 'Test')]
                Param()
                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $Null = $SecureString | ConvertFrom-SecureString -AsPlainText

                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
                $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
                [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)

                $password = ConvertTo-SecureString 'P@ssw0rd' -AsPlainText -Force
                $Ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password)
                $result = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($Ptr)
                [System.Runtime.InteropServices.Marshal]::ZeroFreeCoTaskMemUnicode($Ptr)
                $result

                $SecureString = ConvertTo-SecureString 'P@ssW0rd' -AsPlainText
                $PSCredential = [PSCredential]::new(0, $SecureString)
                $Password = $PSCredential.GetNetworkCredential().Password
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule @($ruleName)
            $violations | Should -BeNullOrEmpty
        }
    }
}