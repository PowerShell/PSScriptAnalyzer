# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $violationMessage = "Missing 'Get-TargetResource' function. DSC Resource must implement Get, Set and Test-TargetResource functions."
    $classViolationMessage = "Missing 'Set' function. DSC Class must implement Get, Set and Test functions."
    $violationName = "PSDSCStandardDSCFunctionsInResource"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAll\MSFT_WaitForAll.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\DSCResourceModule\DSCResources\MSFT_WaitForAny\MSFT_WaitForAny.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $classViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\BadDscResource\BadDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    $noClassViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
}


Describe "StandardDSCFunctionsInResource" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
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
    }
}

Describe "StandardDSCFunctionsInClass" {
    Context "When there are violations" {
        It "has 1 missing standard DSC functions violation" {
            $classViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $classViolations[0].Message | Should -Match $classViolationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noClassViolations.Count | Should -Be 0
        }
    }

    Context "When a class-based DSC resource is also in DSC resource module layout" {
        It "does not duplicate the unapplied suppression error" {
            $resourceRoot = Join-Path $TestDrive 'DSCResources'
            $resourceDir = Join-Path $resourceRoot 'MyRes'
            $resourcePath = Join-Path $resourceDir 'MyRes.psm1'
            $schemaPath = Join-Path $resourceDir 'MyRes.schema.mof'

            New-Item -ItemType Directory -Path $resourceDir -Force | Out-Null
            [System.IO.File]::WriteAllText($resourcePath, @'
[System.Diagnostics.CodeAnalysis.SuppressMessage('PSDSCStandardDSCFunctionsInResource', 'BadDscId', Scope='Class', Target='MyRes')]
[DscResource()]
class MyRes {
    [DscProperty(Key)] [string] $Name
    [MyRes] Get() { return $this }
}
'@.TrimStart() + "`n")
            Set-Content -Path $schemaPath -Value ''

            Invoke-ScriptAnalyzer `
                -Path $resourcePath `
                -ErrorVariable dscErr `
                -ErrorAction SilentlyContinue |
                Out-Null

            $dscErr | Should -HaveCount 1
            $dscErr[0].TargetObject.RuleName | Should -BeExactly $violationName
            $dscErr[0].TargetObject.RuleSuppressionID | Should -BeExactly 'BadDscId'
        }
    }
}
