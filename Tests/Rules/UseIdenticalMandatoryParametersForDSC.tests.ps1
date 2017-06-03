Import-Module PSScriptAnalyzer
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$ruleName = 'PSDSCUseIdenticalMandatoryParametersForDSC'
$resourceFilepath = [System.IO.Path]::Combine(
    $directory,
    'DSCResourceModule',
    'DSCResources',
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter',
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter.psm1');

Describe "UseIdenticalMandatoryParametersForDSC" {
    Context "When a mandatory parameter is not present" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -Path $resourceFilepath -IncludeRule $ruleName
        }

        # todo add a test to check one violation per function
        It "Should find a violations" {
            $violations.Count | Should Be 1
        }

        It "Should mark only the function name" {
            $violations[0].Extent.Text | Should Be 'Get-TargetResource'
        }
    }
}
