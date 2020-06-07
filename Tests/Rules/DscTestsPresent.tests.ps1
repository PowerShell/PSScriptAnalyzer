# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSDSCDscTestsPresent"
}

 Describe "DscTestsPresent rule in class based resource" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
    BeforeAll {
        $testsPath = "$PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\Tests"
        $classResourcePath = "$PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1"
    }

    Context "When tests absent" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $classResourcePath | Where-Object {$_.RuleName -eq $ruleName}
            $violationMessage = "No tests found for resource 'FileResource'"
        }

        It "has 1 missing test violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Be $violationMessage
        }
    }

    Context "When tests present" {
        BeforeAll {
            New-Item -Path $testsPath -ItemType Directory -force
            New-Item -Path "$testsPath\FileResource_Test.psm1" -ItemType File -force
            $noViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $classResourcePath | Where-Object {$_.RuleName -eq $ruleName}
        }

        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        AfterAll {
            Remove-Item -Path $testsPath -Recurse -Force
        }
    }
}

Describe "DscTestsPresent rule in regular (non-class) based resource" {
    BeforeAll {
        $resourcePath = "$PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1"
    }

    Context "When tests absent" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $resourcePath | Where-Object {$_.RuleName -eq $ruleName}
            $violationMessage = "No tests found for resource 'MSFT_WaitForAll'"
        }

        It "has 1 missing tests violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should -Be $violationMessage
        }
    }

    Context "When tests present" {
        BeforeAll {
            $testsPath = "$PSScriptRoot\DSCResourceModule\Tests"
            New-Item -Path $testsPath -ItemType Directory -force
            New-Item -Path "$testsPath\MSFT_WaitForAll_Test.psm1" -ItemType File -force
            $noViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $resourcePath | Where-Object {$_.RuleName -eq $ruleName}
        }

        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }

        AfterAll {
            Remove-Item -Path $testsPath -Recurse -Force
        }
    }
}
