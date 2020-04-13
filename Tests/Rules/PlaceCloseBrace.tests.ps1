# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $ruleConfiguration = @{
        Enable             = $true
        NoEmptyLineBefore  = $true
        IgnoreOneLineBlock = $true
        NewLineAfter       = $true
    }

    $settings = @{
        IncludeRules = @("PSPlaceCloseBrace")
        Rules        = @{
            PSPlaceCloseBrace = $ruleConfiguration
        }
    }
}

Describe "PlaceCloseBrace" {
    Context "When a close brace is not on a new line" {
        BeforeAll {
            $def = @'
function foo {
    Write-Output "close brace not on a new line"}
'@
            $ruleConfiguration.'NoEmptyLineBefore' = $false
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $ruleConfiguration.'NewLineAfter' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }

        It "Should mark the right extent" {
            $violations[0].Extent.Text | Should -Be "}"
        }
    }

    Context "When there is an extra new line before a close brace" {
        BeforeAll {
            $def = @'
function foo {
    Write-Output "close brace not on a new line"

}
'@
            $ruleConfiguration.'NoEmptyLineBefore' = $true
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $ruleConfiguration.'NewLineAfter' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should -Be 1
        }

        It "Should mark the right extent" {
            $violations[0].Extent.Text | Should -Be "}"
        }
    }

    Context "When there is a one line hashtable" {
        BeforeAll {
            $def = @'
$hashtable = @{a = 1; b = 2}
'@
            $ruleConfiguration.'NoEmptyLineBefore' = $false
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $ruleConfiguration.'NewLineAfter' = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violation" {
            $violations.Count | Should -Be 0
        }
    }

    Context "When there is a one line hashtable inside a one line statement" {
        BeforeAll {
            $scriptDefinition = 'if ($true) { $test = @{ } }'
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
        }

        It "Should not find a violation" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "When there is a multi-line hashtable" {
        BeforeAll {

            $ruleConfiguration.'NoEmptyLineBefore' = $false
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $ruleConfiguration.'NewLineAfter' = $false
        }

        It "Should find a violation" {
            $def = @'
            $hashtable = @{
                a = 1
                b = 2}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Should not crash when hashtable is defined on first token" {
            $def = "@{ `n Key = value }"

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings -ErrorAction Stop
            $violations.Count | Should -Be 1
            Invoke-Formatter -ScriptDefinition $def -ErrorAction Stop
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
            $violations.Count | Should -Be 0
        }

        It "Should ignore violations for one line if statement" {
            $def = @'
$x = if ($true) { "blah" } else { "blah blah" }
'@
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }

        It "Should ignore violations for one line if statement even if NewLineAfter is true" {
            $def = @'
$x = if ($true) { "blah" } else { "blah blah" }
'@
            $ruleConfiguration.'IgnoreOneLineBlock' = $true
            $ruleConfiguration.'NewLineAfter' = $true
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "When a close brace should be followed by a new line" {
        BeforeAll {
            $ruleConfiguration.'NoEmptyLineBefore' = $false
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $ruleConfiguration.'NewLineAfter' = $true
        }

        It "Should find a violation for a close brace followed by else" {
            $def = @'
if (Test-Path "blah") {
    "blah"
} else {
    "blah blah"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = '}'
                CorrectionText   = '}' + [System.Environment]::NewLine
            }
            Test-CorrectionExtentFromContent @params
        }

        It "Should find a violation for a close brace followed by elseif" {
            $def = @'
if (Test-Path "blah") {
    "blah"
} elseif (Test-Path "blah blah") {
    "blah blah"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = '}'
                CorrectionText   = '}' + [System.Environment]::NewLine
            }
            Test-CorrectionExtentFromContent @params
        }

        It "Should find a violation for a close brace followed by a catch statement" {
            $def = @'
try {
    "try"
} catch {
    "catch"
}

'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = '}'
                CorrectionText   = '}' + [System.Environment]::NewLine
            }
            Test-CorrectionExtentFromContent @params

        }
        It "Should not find a violation for a close brace followed by a comma in an array expression" {
            $def = @'
Some-Command -Param1 @{
     key1="value1"
    },@{
     key2="value2"
    }
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }

        It "Should not find a violation for a close brace followed by parameter in a command expression" {
            $def = @'
Some-Command -Param1 @{
    key="value"
} -Param2
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "When a close brace should not be followed by a new line" {
        BeforeAll {
            $ruleConfiguration.'NoEmptyLineBefore' = $false
            $ruleConfiguration.'IgnoreOneLineBlock' = $false
            $ruleConfiguration.'NewLineAfter' = $false
        }

        It "Should find a violation if a branching statement is on a new line if open brance is on same line" {
            $def = @'
if ($true) {
    $true
}
else {
    $false
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Should correct violation by cuddling the else branch statement" {
            $def = @'
if ($true) {
    $true
}
else {
    $false
}
'@
            $expected = @'
if ($true) {
    $true
} else {
    $false
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should correct violation if the close brace and following keyword are apart by less than a space" {
            $def = @'
if ($true) {
    $true
}else {
    $false
}
'@
            $expected = @'
if ($true) {
    $true
} else {
    $false
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should correct violation if the close brace and following keyword are apart by more than a space" {
            $def = @'
if ($true) {
    $true
}     else {
    $false
}
'@
            $expected = @'
if ($true) {
    $true
} else {
    $false
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should correct violations in an if-else-elseif block" {
            $def = @'
$x = 1
if ($x -eq 1) {
    "1"
}
elseif ($x -eq 2) {
    "2"
}
else {
    "3"
}
'@
            $expected = @'
$x = 1
if ($x -eq 1) {
    "1"
} elseif ($x -eq 2) {
    "2"
} else {
    "3"
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should correct violations in a try-catch-finally block" {
            $def = @'
try {
    "try"
}
catch {
    "catch"
}
finally {
    "finally"
}
'@
            $expected = @'
try {
    "try"
} catch {
    "catch"
} finally {
    "finally"
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should not find violations when a script has two close braces in succession" {
            $def = @'
function foo {
if ($true) {
"true"
} else {
"false"
}
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }
}
