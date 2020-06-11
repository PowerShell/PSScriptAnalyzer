# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

    $testManifestPath = Join-Path $PSScriptRoot "TestManifest"
    $testManifestBadFunctionsWildcardPath = "ManifestBadFunctionsWildcard.psd1"
    $testManifestBadFunctionsWildcardInArrayPath = "ManifestBadFunctionsWildcardInArray.psd1"
    $testManifestBadFunctionsNullPath = "ManifestBadFunctionsNull.psd1"
    $testManifestBadCmdletsWildcardPath = "ManifestBadCmdletsWildcard.psd1"
    $testManifestBadAliasesWildcardPath = "ManifestBadAliasesWildcard.psd1"
    $testManifestBadVariablesWildcardPath = "ManifestBadVariablesWildcard.psd1"
    $testManifestBadAllPath = "ManifestBadAll.psd1"
    $testManifestGoodPath = "ManifestGood.psd1"
    $testManifestInvalidPath = "ManifestInvalid.psd1"
    Import-Module (Join-Path $PSScriptRoot "PSScriptAnalyzerTestHelper.psm1")

    Function Run-PSScriptAnalyzerRule
    {
        Param(
            [Parameter(Mandatory)]
            [String] $ManifestPath
        )

        Invoke-ScriptAnalyzer -Path (Resolve-Path (Join-Path $testManifestPath $ManifestPath))`
                                -IncludeRule PSUseToExportFieldsInManifest
    }
}


Describe "UseManifestExportFields" {

    Context "Invalid manifest file" {
        It "does not process the manifest" {
            $results = Run-PSScriptAnalyzerRule $testManifestInvalidPath
            $results | Should -BeNullOrEmpty
        }
    }

    Context "Manifest contains violations" {
        It "detects FunctionsToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardPath
            $results.Count | Should -Be 1
            $results[0].Extent.Text | Should -Be "'*'"
        }

        It "suggests corrections for FunctionsToExport with wildcard" {
            $violations = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardPath
            $violationFilepath = Join-path $testManifestPath $testManifestBadFunctionsWildcardPath
            Test-CorrectionExtent $violationFilepath $violations[0] 1 "'*'" "@('Get-Bar', 'Get-Foo')"
            $violations[0].SuggestedCorrections[0].Description | Should -Be "Replace '*' with @('Get-Bar', 'Get-Foo')"
        }

        It "detects FunctionsToExport with null" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsNullPath
            $results.Count | Should -Be 1
            $results[0].Extent.Text | Should -Be '$null'
        }

        It "suggests corrections for FunctionsToExport with null and line wrapping" {
            $violations = Run-PSScriptAnalyzerRule $testManifestBadFunctionsNullPath
            $violationFilepath = Join-path $testManifestPath $testManifestBadFunctionsNullPath
            $expectedCorrectionExtent = "@('Get-Foo1', 'Get-Foo10', 'Get-Foo11', 'Get-Foo12', 'Get-Foo2', 'Get-Foo3', {0}`t`t'Get-Foo4', 'Get-Foo5', 'Get-Foo6', 'Get-Foo7', 'Get-Foo8', {0}`t`t'Get-Foo9')" -f [System.Environment]::NewLine
            Test-CorrectionExtent $violationFilepath $violations[0] 1  '$null' $expectedCorrectionExtent
        }

        It "detects array element containing wildcard" {
	    # if more than two elements contain wildcard we can show only the first one as of now.
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardInArrayPath
            $results.Count | Should -Be 2
			($results | Where-Object {$_.Message -match "FunctionsToExport"}).Extent.Text | Should -Be "'Get-*'"
            ($results | Where-Object {$_.Message -match "CmdletsToExport"}).Extent.Text | Should -Be "'Update-*'"

        }

        It "detects CmdletsToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadCmdletsWildcardPath
            $results.Count | Should -Be 1
            $results[0].Extent.Text | Should -Be "'*'"
        }

        It "detects AliasesToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadAliasesWildcardPath
            $results.Count | Should -Be 1
            $results[0].Extent.Text | Should -Be "'*'"
        }

        It "suggests corrections for AliasesToExport with wildcard" -pending:($IsCoreClr) {
            $violations = Run-PSScriptAnalyzerRule $testManifestBadAliasesWildcardPath
            $violationFilepath = Join-path $testManifestPath $testManifestBadAliasesWildcardPath
            Test-CorrectionExtent $violationFilepath $violations[0] 1  "'*'" "@('gbar', 'gfoo')"
        }

        It "detects all the *ToExport violations" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadAllPath
            $results.Count | Should -Be 3
        }
    }

    Context "Manifest contains no violations" {
        It "detects all the *ToExport fields explicitly stating lists" {
            $results = Run-PSScriptAnalyzerRule $testManifestGoodPath
            $results.Count | Should -Be 0
        }
    }

    Context "When given a non module manifest file" {
        It "does not flag a PowerShell data file" {
            Invoke-ScriptAnalyzer `
                -Path "$PSScriptRoot/TestManifest/PowerShellDataFile.psd1" `
                -IncludeRule "PSUseToExportFieldsInManifest" `
                -OutVariable ruleViolation
            $ruleViolation.Count | Should -Be 0
        }
    }
}
