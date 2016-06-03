Import-Module PSScriptAnalyzer
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testManifestPath = Join-Path $directory "TestManifest"
$testManifestBadFunctionsWildcardPath = "ManifestBadFunctionsWildcard.psd1"
$testManifestBadFunctionsWildcardInArrayPath = "ManifestBadFunctionsWildcardInArray.psd1"
$testManifestBadFunctionsNullPath = "ManifestBadFunctionsNull.psd1"
$testManifestBadCmdletsWildcardPath = "ManifestBadCmdletsWildcard.psd1"
$testManifestBadAliasesWildcardPath = "ManifestBadAliasesWildcard.psd1"
$testManifestBadVariablesWildcardPath = "ManifestBadVariablesWildcard.psd1"
$testManifestBadAllPath = "ManifestBadAll.psd1"
$testManifestGoodPath = "ManifestGood.psd1"
$testManifestInvalidPath = "ManifestInvalid.psd1"
Import-Module (Join-Path $directory "PSScriptAnalyzerTestHelper.psm1")

Function Run-PSScriptAnalyzerRule 
{
    Param(
        [Parameter(Mandatory)]
        [String] $ManifestPath
    )

    Invoke-ScriptAnalyzer -Path (Resolve-Path (Join-Path $testManifestPath $ManifestPath))`
                            -IncludeRule PSUseToExportFieldsInManifest
}

Describe "UseManifestExportFields" {

    Context "Invalid manifest file" {
        It "does not process the manifest" {
            $results = Run-PSScriptAnalyzerRule $testManifestInvalidPath
            $results | Should BeNullOrEmpty            
        }        
    }

    Context "Manifest contains violations" {

        It "detects FunctionsToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be "'*'"
        }

	It "suggests corrections for FunctionsToExport with wildcard" {
	    $violations = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardPath
	    $violationFilepath = Join-path $testManifestPath $testManifestBadFunctionsWildcardPath
	    Test-CorrectionExtent $violationFilepath $violations[0] 1 "'*'" "@('Get-Foo', 'Get-Bar')"
	    $violations[0].SuggestedCorrections[0].Description | Should Be "Replace '*' with @('Get-Foo', 'Get-Bar')"
	}

        It "detects FunctionsToExport with null" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsNullPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be '$null'
        }

	It "suggests corrections for FunctionsToExport with null and line wrapping" {
	    $violations = Run-PSScriptAnalyzerRule $testManifestBadFunctionsNullPath
	    $violationFilepath = Join-path $testManifestPath $testManifestBadFunctionsNullPath
	    Test-CorrectionExtent $violationFilepath $violations[0] 1  '$null' "@('Get-Foo1', 'Get-Foo2', 'Get-Foo3', 'Get-Foo4', 'Get-Foo5', 'Get-Foo6', `r`n`t`t'Get-Foo7', 'Get-Foo8', 'Get-Foo9', 'Get-Foo10', 'Get-Foo11', `r`n`t`t'Get-Foo12')"
	}

        It "detects array element containing wildcard" {
	    # if more than two elements contain wildcard we can show only the first one as of now.
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardInArrayPath
            $results.Count | Should be 2
			($results | Where-Object {$_.Message -match "FunctionsToExport"}).Extent.Text | Should be "'Get-*'"
            ($results | Where-Object {$_.Message -match "CmdletsToExport"}).Extent.Text | Should be "'Update-*'"

        }


        It "detects CmdletsToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadCmdletsWildcardPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be "'*'"
        }

        It "detects AliasesToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadAliasesWildcardPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be "'*'"
        }

	It "suggests corrections for AliasesToExport with wildcard" {
	    $violations = Run-PSScriptAnalyzerRule $testManifestBadAliasesWildcardPath
	    $violationFilepath = Join-path $testManifestPath $testManifestBadAliasesWildcardPath
	    Test-CorrectionExtent $violationFilepath $violations[0] 1  "'*'" "@('gfoo', 'gbar')"
	}

        It "detects all the *ToExport violations" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadAllPath
            $results.Count | Should be 3
        }
    }

    Context "Manifest contains no violations" {
        It "detects all the *ToExport fields explicitly stating lists" {
            $results = Run-PSScriptAnalyzerRule $testManifestGoodPath
            $results.Count | Should be 0
        }        
    }
}


