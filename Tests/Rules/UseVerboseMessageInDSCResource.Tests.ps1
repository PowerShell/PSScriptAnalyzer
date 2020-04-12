# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "There is no call to Write-Verbose in DSC function 'Test-TargetResource'. If you are using Write-Verbose in a helper function, suppress this rule application."
    $violationName = "PSDSCUseVerboseMessageInDSCResource"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
}

Describe "UseVerboseMessageInDSCResource" {
    Context "When there are violations" {
        It "has 2 Verbose Message violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
