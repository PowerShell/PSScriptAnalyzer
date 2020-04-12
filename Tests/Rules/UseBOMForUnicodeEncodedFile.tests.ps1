# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessageOne = "Missing BOM encoding for non-ASCII encoded file 'BOMAbsent_UTF16EncodedScript.ps1'"
    $violationMessageTwo = "Missing BOM encoding for non-ASCII encoded file 'BOMAbsent_UnknownEncodedScript.ps1'"
    $violationName = "PSUseBOMForUnicodeEncodedFile"
    $violationsOne = Invoke-ScriptAnalyzer "$PSScriptRoot\TestFiles\BOMAbsent_UTF16EncodedScript.ps1" | Where-Object {$_.RuleName -eq $violationName}
    $violationsTwo = Invoke-ScriptAnalyzer "$PSScriptRoot\TestFiles\BOMAbsent_UnknownEncodedScript.ps1" | Where-Object {$_.RuleName -eq $violationName}
    $noViolationsOne = Invoke-ScriptAnalyzer "$PSScriptRoot\TestFiles\BOMPresent_UTF16EncodedScript.ps1" | Where-Object {$_.RuleName -eq $violationName}
    $noViolationsTwo = Invoke-ScriptAnalyzer "$PSScriptRoot\TestFiles\BOMAbsent_ASCIIEncodedScript.ps1" | Where-Object {$_.RuleName -eq $violationName}
}

Describe "UseBOMForUnicodeEncodedFile" {
    Context "When there are violations" {
        It "has 1 rule violation for BOM Absent - UTF16 Encoded file" {
            $violationsOne.Count | Should -Be 1
        }

        It "has the correct description message for BOM Absent - UTF16 Encoded file" {
            $violationsOne[0].Message | Should -Match $violationMessageOne
        }

        It "has 1 rule violation for BOM Absent - Unknown Encoded file" {
            $violationsTwo.Count | Should -Be 1
        }

        It "has the correct description message for BOM Absent - Unknown Encoded file" {
            $violationsTwo[0].Message | Should -Match $violationMessageTwo
        }

    }

    Context "When there are no violations" {
        It "returns no violations for BOM Present - UTF16 Encoded File" {
            $noViolationsOne.Count | Should -Be 0
        }

        It "returns no violations for BOM Absent - ASCII Encoded File" {
            $noViolationsTwo.Count | Should -Be 0
        }
    }
}
