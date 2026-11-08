# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = [regex]::Escape("Parameter '`$password' should not use String type but either ")
    $violationName = "PSAvoidUsingPlainTextForPassword"
    $violationFilepath = Join-Path $PSScriptRoot 'AvoidUsingPlainTextForPassword.ps1'
    $violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object { $_.RuleName -eq $violationName }
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUsingPlainTextForPasswordNoViolations.ps1 | Where-Object { $_.RuleName -eq $violationName }
    Import-Module (Join-Path $PSScriptRoot "PSScriptAnalyzerTestHelper.psm1")
}

Describe "AvoidUsingPlainTextForPassword" {
    Context "When there are violations" {
        It "recognizes credential abbreviations in parameter names" {
            $scriptDefinition = 'param([string]$cred, [string]$DbCreds, [string]$DBCred, [string]$CREDFile, [string]$db_cred)'
            @(Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName).Count | Should -Be 5
        }

        It "has expected violations count" {
            $violations.Count | Should -Be 7
        }

        It "suggests corrections" {
            Test-CorrectionExtent $violationFilepath $violations[0] 2 '$passphrases' '[SecureString] $passphrases'
            $violations[0].SuggestedCorrections[0].Description | Should -Be 'Set $passphrases type to SecureString'

            Test-CorrectionExtent $violationFilepath $violations[1] 2 '$passwordparam' '[SecureString] $passwordparam'
            Test-CorrectionExtent $violationFilepath $violations[2] 2 '$credential' '[SecureString] $credential'
            Test-CorrectionExtent $violationFilepath $violations[3] 2 '$password' '[SecureString] $password'
            Test-CorrectionExtent $violationFilepath $violations[4] 2 '[string]' '[SecureString]'
            Test-CorrectionExtent $violationFilepath $violations[5] 2 '[string[]]' '[SecureString[]]'
            Test-CorrectionExtent $violationFilepath $violations[6] 2 '[string]' '[SecureString]'
        }

        It "has the correct violation message" {
            $violations[3].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "ignores names containing an unrelated cred substring" {
            $scriptDefinition = 'param([string]$creditor, [string]$CREDIT, [string]$Accreditation, [string]$Credibility, [string]$creditCardNumber)'
            Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -IncludeRule $violationName | Should -BeNullOrEmpty
        }

        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
