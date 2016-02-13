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

    Context "invalid manifest file" {
        It "does not process the manifest" {
            $results = Run-PSScriptAnalyzerRule $testManifestInvalidPath
            $results | Should BeNullOrEmpty            
        }        
    }

    Context "manifest contains violations" {

        It "detects FunctionsToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be "'*'"
        }

        It "detects FunctionsToExport with null" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsNullPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be '$null'
        }

        It "detects array element containing wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadFunctionsWildcardInArrayPath
            $results.Count | Should be 3
            $results.Where({$_.Message -match "FunctionsToExport"}).Extent.Text | Should be "'Get-*'"
            $results.Where({$_.Message -match "CmdletsToExport"}).Extent.Text | Should be "'Update-*'"
            
            # if more than two elements contain wildcard we can show only the first one as of now.
            $results.Where({$_.Message -match "VariablesToExport"}).Extent.Text | Should be "'foo*'"
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

        It "detects VariablesToExport with wildcard" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadVariablesWildcardPath
            $results.Count | Should be 1
            $results[0].Extent.Text | Should be "'*'"
        }

        It "detects all the *ToExport violations" {
            $results = Run-PSScriptAnalyzerRule $testManifestBadAllPath
            $results.Count | Should be 4
        }
    }

    Context "manifest contains no violations" {
        It "detects all the *ToExport fields explicitly stating lists" {
            $results = Run-PSScriptAnalyzerRule $testManifestGoodPath
            $results.Count | Should be 0
        }        
    }
}


