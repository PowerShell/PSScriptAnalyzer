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

        It "Should track the range when an edit deletes its last line" {
            $text = New-Object -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.EditableText" -ArgumentList "first`n`nlast"
            $correctionType = "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent" -as [type]
            $correction = [Activator]::CreateInstance(
                $correctionType,
                [object[]]@(2, 2, 1, 1, [string[]]@(), $null, $null))
            $correctionsType = [System.Collections.Generic.List``1].MakeGenericType($correctionType)
            $corrections = [Activator]::CreateInstance($correctionsType)
            $corrections.Add($correction)
            $range = New-Object -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.Range" -ArgumentList 1, 1, 2, 1
            $arguments = [object[]]@(
                $text.PSObject.BaseObject,
                $corrections.PSObject.BaseObject,
                0,
                $range.PSObject.BaseObject)
            $fixMethod = ("Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer" -as [type]).GetMethod(
                'Fix',
                [Reflection.BindingFlags]'NonPublic, Static')

            $fixMethod.Invoke($null, $arguments) | Out-Null

            $text.ToString() | Should -Be "first`nlast"
            $arguments[3].End.Line | Should -Be 1
            $arguments[3].End.Column | Should -Be 6
        }

        It "Should exhaust the range when an edit deletes all selected lines" {
            $text = New-Object -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.EditableText" -ArgumentList "first`nlast"
            $correctionType = "Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.CorrectionExtent" -as [type]
            $correction = [Activator]::CreateInstance(
                $correctionType,
                [object[]]@(2, 2, 1, 5, [string[]]@(), $null, $null))
            $correctionsType = [System.Collections.Generic.List``1].MakeGenericType($correctionType)
            $corrections = [Activator]::CreateInstance($correctionsType)
            $corrections.Add($correction)
            $range = New-Object -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.Range" -ArgumentList 2, 1, 2, 5
            $arguments = [object[]]@(
                $text.PSObject.BaseObject,
                $corrections.PSObject.BaseObject,
                0,
                $range.PSObject.BaseObject)
            $fixMethod = ("Microsoft.Windows.PowerShell.ScriptAnalyzer.ScriptAnalyzer" -as [type]).GetMethod(
                'Fix',
                [Reflection.BindingFlags]'NonPublic, Static')

            $fixMethod.Invoke($null, $arguments) | Out-Null
            $text.ToString() | Should -Be "first"
            $arguments[3] | Should -BeNullOrEmpty
        }

        It "Should apply all corrections when earlier fixes expand the selected line" {
            $def = "if(`$a){'x'}`nif(`$b){'y'}"
            $expected = "if(`$a){'x'}`nif (`$b) { 'y' }"

            Invoke-Formatter -ScriptDefinition $def -Range @(2, 1, 2, 12) | Should -Be $expected
        }

        It "Should track the selected lines when fixes <Name>" -TestCases @(
            @{
                Name = 'expand the last line'
                Selected = "if(`$b){'y'}"
                Expected = "if (`$b) { 'y' }"
            }
            @{
                Name = 'shorten the last line'
                Selected = "if  (`$b)  {  'y'  }"
                Expected = "if (`$b) { 'y' }"
            }
            @{
                Name = 'expand multiple lines'
                Selected = "if(`$b){'y'}`nif(`$c){'z'}"
                Expected = "if (`$b) { 'y' }`nif (`$c) { 'z' }"
            }
            @{
                Name = 'remove a line'
                Selected = "if(`$b)`n{`n'y'`n}"
                Expected = "if (`$b) {`n    'y'`n}"
            }
            @{
                Name = 'add a line'
                Selected = "if(`$b) {`n'y'`n}"
                Expected = "if (`$b)`n{`n    'y'`n}"
                Settings = 'CodeFormattingAllman'
            }
        ) {
            param($Selected, $Expected, $Settings = 'CodeFormatting')

            $def = "if(`$a){'x'}`n$Selected`nif(`$d){'w'}"
            $lines = $Selected -split "`n"
            $range = @(2, 1, (1 + $lines.Count), (1 + $lines[-1].Length))

            Invoke-Formatter -ScriptDefinition $def -Range $range -Settings $Settings |
                Should -Be "if(`$a){'x'}`n$Expected`nif(`$d){'w'}"
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
