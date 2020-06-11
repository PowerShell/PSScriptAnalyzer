# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "Function 'TestFunction1' has both Username and Password parameters. Either set the type of the Password parameter to SecureString or replace the Username and Password parameters with a Credential parameter of type PSCredential. If using a Credential parameter in PowerShell 4.0 or earlier, please define a credential transformation attribute after the PSCredential type attribute."
    $violationName = "PSAvoidUsingUserNameAndPasswordParams"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUserNameAndPasswordParams.ps1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidUserNameAndPasswordParamsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "AvoidUserNameAndPasswordParams" {
    Context "When there are violations" {
        It "has 1 avoid username and password parameter violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct violation message" {
            $violations[0].Message | Should -Be $violationMessage
        }

	It "has correct extent" {
	   $expectedExtent = '$password, $username'
	   $violations[0].Extent.Text | Should -Be $expectedExtent

	   $expectedExtent = '$username, $password'
	   $violations[1].Extent.Text | Should -Be $expectedExtent
	}
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
