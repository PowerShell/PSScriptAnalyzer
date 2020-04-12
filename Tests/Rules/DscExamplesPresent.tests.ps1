# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSDSCDscExamplesPresent"
}

 Describe "DscExamplesPresent rule in class based resource" -Skip:($PSVersionTable.PSVersion -lt [Version]'5.0.0')  {
    BeforeAll {
        $examplesPath = "$PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\Examples"
        $classResourcePath = "$PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1"
    }

    Context "When examples absent" {

        $violations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $classResourcePath | Where-Object {$_.RuleName -eq $ruleName}
        $violationMessage = "No examples found for resource 'FileResource'"

        It "has 1 missing examples violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When examples present" {
        New-Item -Path $examplesPath -ItemType Directory -force
        New-Item -Path "$examplesPath\FileResource_Example.psm1" -ItemType File -force

        $noViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $classResourcePath | Where-Object {$_.RuleName -eq $ruleName}

        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        Remove-Item -Path $examplesPath -Recurse -Force
    }
}

Describe "DscExamplesPresent rule in regular (non-class) based resource" {
    BeforeAll {
        $examplesPath = "$PSScriptRoot\DSCResourceModule\Examples"
        $resourcePath = "$PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1"
    }

    Context "When examples absent" {

        $violations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $resourcePath | Where-Object {$_.RuleName -eq $ruleName}
        $violationMessage = "No examples found for resource 'MSFT_WaitForAll'"

        It "has 1 missing examples violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Match $violationMessage
        }
    }

    Context "When examples present" {
        BeforeAll {
            New-Item -Path $examplesPath -ItemType Directory -force
            New-Item -Path "$examplesPath\MSFT_WaitForAll_Example.psm1" -ItemType File -force
            $noViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $resourcePath | Where-Object {$_.RuleName -eq $ruleName}
        }

        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        Remove-Item -Path $examplesPath -Recurse -Force
    }
}
