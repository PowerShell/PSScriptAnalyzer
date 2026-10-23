# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
}

Describe "Invoke-Formatter Cmdlet" {
    Context "Cmdlet cleans up and has no knock on effect" {
        It "Invoke-Formatter has knock on effect on Invoke-ScriptAnalyzer" {
            Invoke-Formatter 'foo'
            Invoke-ScriptAnalyzer -ScriptDefinition 'gci' | Should -Not -BeNullOrEmpty
        }
    }

    Context 'Accept Value from Pipeline' {
        It 'Value from Pipeline' {
            'foo' | Invoke-Formatter | Should -Be 'foo'
        }
        It 'Value from Pipeline by Property Name with just the mandatory ScriptDefinition parameter' {
            [pscustomobject]@{ 'ScriptDefinition' = 'foo' } | Invoke-Formatter | Should -Be 'foo'
        }
        It 'Value from Pipeline by Property Name with all parameters' {
            [pscustomobject]@{ ScriptDefinition = 'foo'; Settings = @(); Range = 1, 1, 1, 4 } |
                Invoke-Formatter | Should -Be 'foo'
        }
    }

    Context 'Formatting files' {
        It 'Formats a file in place while preserving its encoding, line endings, and tab indentation' {
            $path = Join-Path $TestDrive 'script.ps1'
            $scriptDefinition = "function foo {`r`n`t`$x=1`r`n}"
            $expected = "function foo {`r`n`t`$x = 1`r`n}"
            $encoding = [System.Text.UTF8Encoding]::new($true)
            [System.IO.File]::WriteAllText($path, $scriptDefinition, $encoding)

            Invoke-Formatter -Path $path | Should -Be $expected

            [System.IO.File]::ReadAllText($path) | Should -Be $expected
            [System.IO.File]::ReadAllBytes($path)[0..2] | Should -Be @(239, 187, 191)
        }

        It 'Preserves a UTF-16 encoding and space indentation' {
            $path = Join-Path $TestDrive 'unicode.ps1'
            $scriptDefinition = "function foo {`n    `$x=1`n}"
            $expected = "function foo {`n    `$x = 1`n}"
            $encoding = [System.Text.UnicodeEncoding]::new($false, $true)
            [System.IO.File]::WriteAllText($path, $scriptDefinition, $encoding)

            Invoke-Formatter -Path $path | Should -Be $expected

            [System.IO.File]::ReadAllText($path) | Should -Be $expected
            [System.IO.File]::ReadAllBytes($path)[0..1] | Should -Be @(255, 254)
        }

        It 'Infers indentation from code instead of whitespace-only lines' {
            $path = Join-Path $TestDrive 'blank-lines.ps1'
            $scriptDefinition = "function foo {`n`t`$x=1`n    `n    `n    `n    `n`t`$y=2`n}"
            [System.IO.File]::WriteAllText($path, $scriptDefinition)

            $formatted = Invoke-Formatter -Path $path

            $formatted | Should -Match '(?m)^\t\$x = 1$'
            $formatted | Should -Match '(?m)^\t\$y = 2$'
        }

        It 'Infers indentation from code instead of multiline payload lines' {
            $path = Join-Path $TestDrive 'here-string.ps1'
            $scriptDefinition = "function foo {`n`t`$value = @'`n        first payload`n        second payload`n        third payload`n        fourth payload`n        fifth payload`n'@`n<#`n        comment payload`n        second comment payload`n        third comment payload`n        fourth comment payload`n        fifth comment payload`n#>`n`t`$x=1`n}"
            [System.IO.File]::WriteAllText($path, $scriptDefinition)

            $formatted = Invoke-Formatter -Path $path

            $formatted | Should -Match '(?m)^\t\$x = 1$'
            $formatted | Should -Match '(?m)^        first payload$'
            $formatted | Should -Match '(?m)^        comment payload$'
        }

        It 'Does not modify a file when WhatIf is specified' {
            $path = Join-Path $TestDrive 'whatif.ps1'
            $scriptDefinition = '$x=1'
            [System.IO.File]::WriteAllText($path, $scriptDefinition)

            Invoke-Formatter -Path $path -WhatIf 6>$null

            [System.IO.File]::ReadAllText($path) | Should -Be $scriptDefinition
        }
    }

    Context "When positional parameters are given" {
        It "Should use the positional parameters" {
            $def = @"
function foo {
"abc"
}
"@

            $expected = @"
function foo {
    "abc"
}
"@

            $settings = @{
                IncludeRules = @('PSUseConsistentIndentation')
                Rules        = @{
                    PSUseConsistentIndentation = @{
                        Enable = $true
                    }
                }
            }

            Invoke-Formatter $def $settings | Should -Be $expected
        }

        It "Should not expand unary operators when being used as a single negative argument" {
            $script = '$foo.bar(-$a)'
            Invoke-Formatter '$foo.bar(-$a)' -Settings CodeFormatting | Should -Be $script
        }

        It "Should expand unary operators when not being used as a single negative argument" {
            Invoke-Formatter '$foo.bar(-$a+$b+$c)' -Settings CodeFormatting | Should -Be '$foo.bar(-$a + $b + $c)'
        }
    }

    Context "When a range is given" {
        It "Should format only within the range when a range list is given" {
            $def = @"
function foo {
"xyz"
"abc"
}
"@

            $expected = @"
function foo {
"xyz"
    "abc"
}
"@

            Invoke-Formatter -ScriptDefinition $def -Range @(3, 1, 4, 1) | Should -Be $expected
        }
    }

    Context "When no settings are given" {
        It "Should format using default settings" {
            $def = @'
function foo
{
get-childitem
$x=1+2
$hashtable = @{
property1 = "value"
    anotherProperty = "another value"
}
}
'@
            $expected = @'
function foo {
    get-childitem
    $x = 1 + 2
    $hashtable = @{
        property1       = "value"
        anotherProperty = "another value"
    }
}
'@

            Invoke-Formatter -ScriptDefinition $def | Should -Be $expected
        }

        It "Does not throw when using turkish culture - https://github.com/PowerShell/PSScriptAnalyzer/issues/1095" {
            $initialCulture = [System.Threading.Thread]::CurrentThread.CurrentCulture
            try {
                [System.Threading.Thread]::CurrentThread.CurrentCulture = [cultureinfo]::CreateSpecificCulture('tr-TR')
                Invoke-Formatter ' foo' | Should -Be 'foo'
            }
            finally {
                [System.Threading.Thread]::CurrentThread.CurrentCulture = $initialCulture
            }
        }
    }

}
