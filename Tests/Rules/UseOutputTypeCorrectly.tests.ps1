# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "The cmdlet 'Verb-Files' returns an object of type 'System.Collections.Hashtable' but this type is not declared in the OutputType attribute."
    $violationName = "PSUseOutputTypeCorrectly"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
    if ($PSVersionTable.PSVersion -ge [Version]'5.0.0')
    {
        $dscViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    }
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}
}


Describe "UseOutputTypeCorrectly" {
    Context "When there are violations" {
        It "has 2 Use OutputType Correctly violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }

        if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
            It "Does not count violation in DSC class" {
                $dscViolations.Count | Should -Be 0
            }
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
