# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "The Test and Set-TargetResource functions of DSC Resource must have the same parameters."
    $violationName = "PSDSCUseIdenticalParametersForDSC"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}

    if ($PSVersionTable.PSVersion -ge [Version]'5.0.0')
    {
        $noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    }
}


Describe "UseIdenticalParametersDSC" {
    Context "When there are violations" {
        It "has 1 Use Identical Parameters For DSC violations" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        if ($PSVersionTable.PSVersion -ge [Version]'5.0.0')
        {

            It "returns no violations for DSC Classes" {
                $noClassViolations.Count | Should -Be 0
            }
        }
    }
}
