# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $settings = @{
        IncludeRules = @("PSUseSupportsShouldProcess")
        Rules = @{
            PSUseSupportsShouldProcess = @{
                Enable = $true
            }
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
            $s = " "
            $expectedCorrection = @"
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
$s$s$s$s$s$s$s$s
    )
}
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Should return valid correction text if whatif is the first parameter" {
            $def = @'
function foo {
    param(
        [bool] $whatif,
        $param1
    )
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Should return valid correction text if whatif is in the middle" {
            $def = @'
function foo {
    param(
        $param1,
        [bool] $whatif,
        $param2
    )
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1,
        $param2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }


        It "Should return valid correction text if whatif the last parameter" {
            $def = @'
function foo {
    param(
        $param1,
        [bool] $whatif
    )
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection

        }

        It "Should find violation if only Confirm is added" {
            $def = @'
function foo {
    param(
        [bool] $confirm
    )
}
'@

            #  We use this odd construct because the editor auto-removes the trailing whitespaces.
            $s = " "
            $expectedCorrection = @"
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
$s$s$s$s$s$s$s$s
    )
}
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
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
            $s = " "
            $expectedCorrection = @"
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
$s$s$s$s$s$s$s$s
    )
}
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
            # TODO Make test-correction extent take more than 1 corrections
            # or modify the rule such that it outputs only 1 correction.
            # Test-CorrectionExtentFromContent $def $violation 2 $expectedViolationText ''
        }

        It "Suggests adding SupportsShouldProcess attribute, when no argument is present" {
            $def = @'
function foo {
    [CmdletBinding()]
    param(
        [bool] $confirm,
        [bool] $whatif
    )
}
'@
            $s = " "
            $expectedCorrection = @"
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
$s$s$s$s$s$s$s$s
    )
}
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Suggests adding SupportsShouldProcess attribute, when arguments are present" {
            $def = @'
function foo {
    [CmdletBinding(ConfirmImpact="High")]
    param(
        [bool] $confirm,
        [bool] $whatif
    )
}
'@
            $s = " "
            $expectedCorrection = @"
function foo {
    [CmdletBinding(SupportsShouldProcess, ConfirmImpact="High")]
    param(
$s$s$s$s$s$s$s$s
    )
}
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Suggests replacing function parameter declaration with a param block" {
            $def = @'
function foo ($param1, $whatif, $param2) {
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1,
        $param2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Suggests replacing function parameter declaration with whatif and confirm with a param block" {
            $def = @'
function foo ($param1, $whatif, $confirm, $param2) {
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1,
        $param2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Suggests replacing function parameter declaration with non-contiguous whatif and confirm with a param block" {
            $def = @'
function foo ($param1, $whatif, $param2, $confirm) {
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess)]
    param(
        $param1,
        $param2
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }

        It "Suggests setting SupportsShouldProcess to `$true" {
            $def = @'
function foo {
    [CmdletBinding(SupportsShouldProcess=$false)]
    param($whatif)
}
'@
            $s = " "
            $expectedCorrection = @'
function foo {
    [CmdletBinding(SupportsShouldProcess=$true)]
    param()
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].SuggestedCorrections[0].Text | Should -Be $expectedCorrection
        }




    }

}
