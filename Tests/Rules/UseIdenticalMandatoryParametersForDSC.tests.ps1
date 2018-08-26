$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$ruleName = 'PSDSCUseIdenticalMandatoryParametersForDSC'
$resourceBasepath = "$directory\DSCResourceModule\DSCResources"
$badResourceFilepath = [System.IO.Path]::Combine(
    $resourceBasepath,
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter',
    'MSFT_WaitForAnyNoIdenticalMandatoryParameter.psm1');
$goodResourceFilepath = [System.IO.Path]::Combine($resourceBasepath,'MSFT_WaitForAny','MSFT_WaitForAny.psm1');

$noparentClassFilepath = [System.IO.Path]::Combine($resourceBasepath,'ClassWithNoParent','ClassWithNoParent.psm1');

Describe "UseIdenticalMandatoryParametersForDSC" {
    Context "When a mandatory parameters are not present" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -Path $badResourceFilepath -IncludeRule $ruleName
        }

        It "Should find a violations" -skip:($IsLinux -or $IsMacOS) {
            $violations.Count | Should -Be 5
        }

        It "Should mark only the function name" -skip:($IsLinux -or $IsMacOS) {
            $violations[0].Extent.Text | Should -Be 'Get-TargetResource'
        }
    }

    Context "When all mandatory parameters are present" {
        BeforeAll {
            $violations = Invoke-ScriptAnalyzer -Path $goodResourceFilepath -IncludeRule $ruleName
        }

        # todo add a test to check one violation per function
        It "Should find a violations" -pending {
            $violations.Count | Should -Be 0
        }
    }

    Context "When a CIM class has no parent" {
        # regression test for #982 - just check no uncaught exception
        It "Should find no violations, and throw no exceptions" {
            $violations = Invoke-ScriptAnalyzer -Path $noParentClassFilepath -IncludeRule $ruleName
            $violations.Count | Should -Be 0
        }
    }
}
