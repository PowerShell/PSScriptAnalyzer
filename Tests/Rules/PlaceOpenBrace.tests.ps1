# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleConfiguration = @{
        Enable          = $true
        OnSameLine      = $true
        NewLineAfter    = $true
        IgnoreOneLineIf = $true
    }

    $settings = @{
        IncludeRules = @("PSPlaceOpenBrace")
        Rules        = @{
            PSPlaceOpenBrace = $ruleConfiguration
        }
    }
}


Describe "PlaceOpenBrace" {
    Context "When an open brace must be on the same line" {
        BeforeAll {
            $def = @'
function foo ($param1)
{

}
'@
            $ruleConfiguration.'OnSameLine' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should -Be '{'
        }
    }

    Context "Handling of comments when using Invoke-Formatter" {
        It "Should correct violation when brace should be on the same line" {
            $scriptDefinition = @'
foreach ($x in $y)
{
    Get-Something
}
'@
            $expected = @'
foreach ($x in $y) {
    Get-Something
}
'@
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingStroustrup' | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingOTBS' | Should -Be $expected
        }

        It "Should correct violation when brace should be on the same line and take comment into account" {
            $scriptDefinition = @'
foreach ($x in $y) # useful comment
{
    Get-Something
}
'@
            $expected = @'
foreach ($x in $y) { # useful comment
    Get-Something
}
'@
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingStroustrup' | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingOTBS' | Should -Be $expected
        }

        It "Should correct violation when the brace should be on the next line and take comment into account" {
            $scriptDefinition = @'
foreach ($x in $y) # useful comment
{
    Get-Something
}
'@
            $expected = @'
foreach ($x in $y) { # useful comment
    Get-Something
}
'@
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingStroustrup' | Should -Be $expected
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings 'CodeFormattingOTBS' | Should -Be $expected
        }
    }

    Context "When an open brace must be on the same line in a switch statement" {
        BeforeAll {
            $def = @'
switch ($x) {
    {"b"} {"b"; break;}
    {"a"} {"a"; break;}
}
'@
            $ruleConfiguration.'OnSameLine' = $true
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violation" {
            $violations.Count | Should -Be 0
        }
    }

    Context "When an open brace must be on a new line" {
        BeforeAll {
            $def = @'
function foo ($param1) {

}
'@
            $ruleConfiguration.'OnSameLine' = $false
            $ruleConfiguration.'NewLineAfter' = $true
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $defShouldIgnore = @'
Get-Process | % { "blah" }
'@
            $violationsShouldIgnore = Invoke-ScriptAnalyzer -ScriptDefinition $defShouldIgnore -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should -Be '{'
        }

        It "Should ignore violations for a command element" {
            $violationsShouldIgnore.Count | Should -Be 0
        }

        It "Should ignore violations for one line if statement" {
            $def = @'
$x = if ($true) { "blah" } else { "blah blah" }
'@
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "When a new line should follow an open brace" {
        BeforeAll {
            $def = @'
function foo { }
'@
            $ruleConfiguration.'OnSameLine' = $true
            $ruleConfiguration.'NewLineAfter' = $true
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should -Be '{'
        }
    }
}
