Import-Module PSScriptAnalyzer
$ruleName = "PSUseSupportsShouldProcess"
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
            $violation = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violation.Count | Should Be 1
        }

        It "Should find violation if only Confirm is added" {
            $def = @'
function foo {
    param(
        [bool] $confirm
    )
}
'@
            $violation = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violation.Count | Should Be 1
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
            $violation = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violation.Count | Should Be 1
        }

        It "Suggests adding SupportsShouldProcess attribute" {

        }

        It "Suggests removing the manually added parameters" {

        }
    }
}