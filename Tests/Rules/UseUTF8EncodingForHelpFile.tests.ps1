# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "UseUTF8EncodingForHelpFile" {
    BeforeAll {
        $violationMessage = "File about_utf16.help.txt has to use UTF8 instead of System.Text.UTF32Encoding encoding because it is a powershell help file."
        $violationName = "PSUseUTF8EncodingForHelpFile"
        $violations = Invoke-ScriptAnalyzer $PSScriptRoot\about_utf16.help.txt | Where-Object {$_.RuleName -eq $violationName}
        $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\about_utf8.help.txt | Where-Object {$_.RuleName -eq $violationName}
        $notHelpFileViolations = Invoke-ScriptAnalyzer $PSScriptRoot\utf16.txt | Where-Object {$_.RuleName -eq $violationName}
    }

    Context "When there are violations" {
        It "has 1 avoid use utf8 encoding violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations for correct utf8 help file" {
            $noViolations.Count | Should -Be 0
        }

        It "returns no violations for utf16 file that is not a help file" {
            $notHelpFileViolations.Count | Should -Be 0
        }
    }
}
