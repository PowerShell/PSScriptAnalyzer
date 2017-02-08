Import-Module PSScriptAnalyzer
$ruleConfiguration = @{
    Enable = $true
    NoEmptyLineBefore = $true
    IgnoreOneLineBlock = $true
}

$settings = @{
    IncludeRules = @("PSPlaceCloseBrace")
    Rules = @{
        PSPlaceCloseBrace = $ruleConfiguration
    }
}

Describe "PlaceCloseBrace" {
    Context "When a close brace is not on a new line" {
        BeforeAll {
            $def = @'
function foo {
    Write-Output "close brace not on a new line"}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }

        It "Should mark the right extent" {
            $violations[0].Extent.Text | Should Be "}"
        }
    }

    Context "When there is an extra new line before a close brace" {
        BeforeAll {
            $def = @'
function foo {
    Write-Output "close brace not on a new line"

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }

        It "Should mark the right extent" {
            $violations[0].Extent.Text | Should Be "}"
        }
    }

    Context "When there is a one line hashtable" {
        BeforeAll {
            $def = @'
$hashtable = @{a = 1; b = 2}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violation" {
            $violations.Count | Should Be 0
        }
    }

    Context "When there is a multi-line hashtable" {
        BeforeAll {
            $def = @'
$hashtable = @{
    a = 1
    b = 2}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }
    }

    Context "When a close brace is on the same line as its open brace" {
        BeforeAll {
            $def = @'
Get-Process * | % { "blah" }
'@
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violation" {
            $violations.Count | Should Be 0
        }

        It "Should ignore violations for one line if statement" {
            $def = @'
$x = if ($true) { "blah" } else { "blah blah" }
'@
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0
        }
    }
}
