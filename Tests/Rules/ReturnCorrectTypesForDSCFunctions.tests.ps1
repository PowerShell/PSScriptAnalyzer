# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessageDSCResource = "Test-TargetResource function in DSC Resource should return object of type System.Boolean instead of System.Collections.Hashtable"
    $violationMessageDSCClass = "Get function in DSC Class FileResource should return object of type FileResource instead of type System.Collections.Hashtable"
    $violationName = "PSDSCReturnCorrectTypesForDSCFunctions"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}

    if ($PSVersionTable.PSVersion -ge [Version]'5.0.0')
    {
        $classViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\BadDscResource\BadDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
        $noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    }
}

Describe "ReturnCorrectTypesForDSCFunctions" {
    Context "When there are violations" {
        It "has 5 return correct types for DSC functions violations" {
            $violations.Count | Should -Be 5
        }

        It "has the correct description message" {
            $violations[2].Message | Should -Match $violationMessageDSCResource
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}

 Describe "StandardDSCFunctionsInClass" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
    Context "When there are violations" {
        It "has 4 return correct types for DSC functions violations" {
            $classViolations.Count | Should -Be 4
        }

        It "has the correct description message" {
            $classViolations[3].Message | Should -Match $violationMessageDSCClass
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noClassViolations.Count | Should -Be 0
        }
    }
}
