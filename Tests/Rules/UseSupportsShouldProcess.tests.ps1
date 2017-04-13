$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$settings = @{
    IncludeRules = @("PSUseSupportsShouldProcess")
    Rules = @{
        PSUseSupportsShouldProcess = @{
            Enable = $true
        }
    }
}

Describe "UseSupportsShouldProcess" {
    Context "When a function manually adds WhatIf and Confirm parameters" {
        It "Should find violation if only WhatIf is added" {
            $def = @'
function foo {
    param(
        [bool] $whatif
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
            Test-CorrectionExtentFromContent $def $violations 1 '[bool] $whatif' ''
        }

        It "Should find violation if only Confirm is added" {
            $def = @'
function foo {
    param(
        [bool] $confirm
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
            Test-CorrectionExtentFromContent $def $violations 1 '[bool] $confirm' ''
        }

        It "Should find violation if both Whatif and Confirm are added" {
            $def = @'
function foo {
    param(
        [bool] $confirm,
        [bool] $whatif
    )
}
'@
            $expectedViolationText = @'
        [bool] $confirm,
        [bool] $whatif
'@
            $violation = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violation.Count | Should Be 1
            $violation[0].SuggestedCorrections.Count | Should Be 2
            # TODO Make test-correction extent take more than 1 corrections
            # or modify the rule such that it outputs only 1 correction.
            # Test-CorrectionExtentFromContent $def $violation 2 $expectedViolationText ''
        }

        It "Suggests adding SupportsShouldProcess attribute" {

        }

        It "Suggests removing the manually added parameters" {

        }
    }
}
