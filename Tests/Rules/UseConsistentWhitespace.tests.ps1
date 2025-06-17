# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $ruleName = "PSUseConsistentWhitespace"
    $ruleConfiguration = @{
        Enable          = $true
        CheckInnerBrace = $false
        CheckOpenBrace  = $false
        CheckOpenParen  = $false
        CheckOperator   = $false
        CheckPipe       = $false
        CheckSeparator  = $false
        CheckParameter  = $false
    }

    $settings = @{
        IncludeRules = @($ruleName)
        Rules        = @{
            PSUseConsistentWhitespace = $ruleConfiguration
        }
    }
}


Describe "UseWhitespace" {
    Context "When an open brace follows a keyword" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $true
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if an open brace does not follow whitespace" {
            $def = 'if ($true){}'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find violation if an open brace follows a whitespace" {
            $def = 'if($true) {}'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find violation if an open brace follows a foreach member invocation" {
            $def = '(1..5).foreach{$_}'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find violation if an open brace follows a where member invocation" {
            $def = '(1..5).where{$_}'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find violation if an open brace is on the next line" {
            $def = @'
if ($true)
{
    foo
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It 'Should not find a violation if an open paren is before an opening brace' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$ast.Find({ $oneAst -is [TypeExpressionAst] })' -Settings $settings |
                Should -BeNullOrEmpty
        }

        It 'Should not find a violation if an open paren is preceded by a Dot token' {
            Invoke-ScriptAnalyzer -ScriptDefinition '$foo.{bar}' -Settings $settings |
                Should -BeNullOrEmpty
        }
    }

    Context "When a parenthesis follows a keyword" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $true
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find violation in an if statement" {
            $def = 'if($true) {}'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation in a function definition" {
            $def = @'
function foo($param1) {

}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation in a param block" {
            $def = @'
function foo() {
    param( )
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation in a nested open paren" {
            $def = @'
function foo($param) {
    ((Get-Process))
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation on a method call" {
            $def = '$x.foo("bar")'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }
    }

    Context "When there is whitespace around assignment and binary operators" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
            $ruleConfiguration.IgnoreAssignmentOperatorInsideHashTable = $false
        }

        It "Should find a violation if no whitespace around an assignment operator" {
            $def = '$x=1'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '=' ' = '
        }

        It "Should find a violation if no whitespace before an assignment operator" {
            $def = '$x= 1'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if no whitespace after an assignment operator" {
            $def = '$x =1'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is a whitespaces not of size 1 around an assignment operator" {
            $def = '$x  =  1'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  =  ' ' = '
        }

        It "Should not find violation if there are whitespaces of size 1 around an assignment operator" {
            $def = @'
$x = @"
"abc"
"@
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find violation if there are whitespaces of size 1 around an assignment operator for here string" {
            $def = '$x = 1'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find violation if there are no whitespaces around DotDot operator" {
            $def = '1..5'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find violation if a binary operator is followed by new line" {
            $def = @'
$x = $true -and
            $false
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should find violation if not asked to ignore assignment operator in hash table" {
            $def = @'
$ht = @{
    variable = 3
    other    = 4
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '    ' ' '
        }
    }

    Context "When asked to ignore assignment operator inside hash table" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
            $ruleConfiguration.IgnoreAssignmentOperatorInsideHashTable = $true
        }

        It "Should not find violation if assignment operator is in multi-line hash table and a using statement is present" {
            $def = @'
using namespace System.IO

$ht = @{
    variable = 3
    other    = 4
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find violation if assignment operator is in multi-line hash table" {
            $def = @'
$ht = @{
    variable = 3
    other    = 4
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should find violation if assignment operator has extra space in single-line hash table" {
            $def = @'
$h = @{
    ht = @{a = 3; b   = 4}
    eb = 33
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '   ' ' '
        }

        It "Should find violation for extra space around non-assignment operator inside hash table" {
            $def = @'
$ht = @{
    variable = 3
    other    = 4 +  7
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  ' ' '
        }
    }

    Context "When a comma is not followed by a space" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $true
        }

        It "Should find a violation" {
            $def = '$x = @(1,2)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if a space follows a comma" {
            $def = '$x = @(1, 2)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }
    }

    Context "When a semi-colon is not followed by a space" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $true
        }

        It "Should find a violation" {
            $def = '$x = @{a=1;b=2}'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if a space follows a semi-colon" {
            $def = '$x = @{a=1; b=2}'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a new-line follows a semi-colon" {
            $def = @'
$x = @{
    a=1;
    b=2
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a end of input follows a semi-colon" {
            $def = @'
$x = "abc";
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }
    }


    Context "CheckPipe" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $true
            $ruleConfiguration.CheckPipeForRedundantWhitespace = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if there is no space after pipe" {
            $def = 'Get-Item |foo'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if there is no space before pipe" {
            $def = 'Get-Item| foo'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if there is one space too much before pipe" {
            $def = 'Get-Item  | foo'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should find a violation if there is one space too much after pipe" {
            $def = 'Get-Item |  foo'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find a violation if there is 1 space before and after a pipe" {
            $def = 'Get-Item | foo'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a backtick is before the pipe" {
            $def = @'
Get-Item `
| foo
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a new-line is after the pipe" {
            $def = @'
Get-Item |
    foo
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a backtick is after the pipe" {
            $def = @'
Get-Item |`
foo
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }
    }

    Context "CheckPipeForRedundantWhitespace" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckPipeForRedundantWhitespace = $true
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should not find a violation if there is no space around pipe" {
            $def = 'foo|bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find a violation if there is exactly one space around pipe" {
            $def = 'foo | bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should find a violation if there is one space too much before pipe" {
            $def = 'foo  | bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  ' ' '
        }

        It "Should find a violation if there is two spaces too much before pipe" {
            $def = 'foo   | bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '   ' ' '
        }

        It "Should find a violation if there is one space too much after pipe" {
            $def = 'foo |  bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  ' ' '
        }

        It "Should find a violation if there is two spaces too much after pipe" {
            $def = 'foo |   bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '   ' ' '
        }
    }

    Context "CheckInnerBrace" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $true
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if there is no space after opening brace" {
            $def = 'if ($true) {Get-Item }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space after opening brace when there are 2 braces" {
            $def = 'if ($true) {{ Get-Item } }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space before closing brace" {
            $def = 'if ($true) { Get-Item}'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space before closing brace when there are 2 braces" {
            $def = 'if ($true) { { Get-Item }}'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is more than 1 space after opening brace" {
            $def = 'if($true) {  Get-Item }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  ' ' '
        }

        It "Should find a violation if there is more than 1 space before closing brace" {
            $def = 'if($true) { Get-Item  }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  ' ' '
        }

        It "Should not find a violation if there is 1 space after the opening brace and 1 before the closing brace" {
            $def = 'if($true) { Get-Item }'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is 1 space inside empty curly braces" {
            $def = 'if($true) { }'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation for an empty hashtable" {
            $def = '$hashtable = @{}'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a new-line is after the opening brace" {
            $def = @'
if ($true) {
    Get-Item }
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a backtick is after the opening brace" {
            $def = @'
if ($true) {`
    Get-Item }
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a new-line is before the closing brace" {
            $def = @'
if ($true) { Get-Item
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if a backtick is before the closing brace" {
            $def = @'
if ($true) { Get-Item `
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It 'Should not throw when analysing a line starting with a scriptblock' {
            { Invoke-ScriptAnalyzer -ScriptDefinition '{ }' -Settings $settings -ErrorAction Stop } | Should -Not -Throw
        }
    }

    Context "CheckSeparator" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $true
        }

        It "Should find a violation if there is no space after a comma" {
            $def = '$Array = @(1,2)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -HaveCount 1
        }

        It "Should not find a violation if there is a space after a comma" {
            $def = '$Array = @(1, 2)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is a new-line after a comma" {
            $def = @'
$Array = @(
    1,
    2
)
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is a comment after the separator" {
            $def = @'
$Array = @(
    'foo',     # Comment Line 1
    'FizzBuzz' # Comment Line 2
)
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

    }


    Context "CheckParameter" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $true
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
            $ruleConfiguration.CheckParameter = $true
        }

        It "Should not find no violation when newlines are involved" {
            $def = {foo -a $b `
-c d -d $e -f g `
-h i |
bar -h i `
-switch}
            Invoke-ScriptAnalyzer -ScriptDefinition "$def" -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is always 1 space between parameters except when using colon syntax" {
            $def = 'foo -bar $baz @splattedVariable -bat -parameterName:$parameterValue'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation when redirect operators, spearated by 1 space, are used and not in stream order" {
            # Related to Issue #2000
            $def = 'foo 3>&1 1>$null 2>&1'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should find 1 violation if there is 1 space too much before a parameter" {
            $def = 'foo  -bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'foo'
            $violations[0].SuggestedCorrections[0].Text | Should -Be ([string]::Empty)
        }

        It "Should find 1 violation if there is 1 space too much before a parameter value" {
            $def = 'foo  $bar'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].Extent.Text | Should -Be 'foo'
            $violations[0].SuggestedCorrections[0].Text | Should -Be ([string]::Empty)
        }

        It "Should fix script to always have 1 space between parameters except when using colon syntax but not by default" {
            $def = 'foo  -bar   $baz  -ParameterName:  $ParameterValue "$PSScriptRoot\module.psd1"'
            Invoke-Formatter -ScriptDefinition $def |
                Should -BeExactly $def -Because 'CheckParameter configuration is not turned on by default (yet) as the setting is new'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings |
                Should -BeExactly 'foo -bar $baz -ParameterName:  $ParameterValue "$PSScriptRoot\module.psd1"'
        }

        It "Should fix script when newlines are involved" {
            $def = {foo  -a  $b `
-c  d -d  $e  -f  g `
-h  i |
bar  -h  i `
-switch}
            $expected = {foo -a $b `
-c d -d $e -f g `
-h i |
bar -h i `
-switch}
            Invoke-Formatter -ScriptDefinition "$def" -Settings $settings |
                Should -Be "$expected"
        }

        It "Should fix script when a parameter value is a script block spanning multiple lines" {
            $def = {foo {
    bar
}     -baz}

            $expected = {foo {
    bar
} -baz}
            Invoke-Formatter -ScriptDefinition "$def" -Settings $settings |
                Should -Be "$expected"
        }

        It "Should fix script when a parameter value is a hashtable spanning multiple lines" {
            $def = {foo @{
    a = 1
}     -baz}

            $expected = {foo @{
    a = 1
} -baz}
            Invoke-Formatter -ScriptDefinition "$def" -Settings $settings |
                Should -Be "$expected"
        }

        It "Should fix script when a parameter value is an array spanning multiple lines" {
            $def = {foo @(
    1
)     -baz}

            $expected = {foo @(
    1
) -baz}
            Invoke-Formatter -ScriptDefinition "$def" -Settings $settings |
                Should -Be "$expected"
        }

        It "Should fix script when redirects are involved and whitespace is not consistent" {
            # Related to Issue #2000
            $def = 'foo   3>&1  1>$null   2>&1'
            $expected = 'foo 3>&1 1>$null 2>&1'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings |
                Should -Be $expected
        }
    }

    Context "When keywords follow closing braces" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenBrace = $true
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if no space between } and while" {
            $def = 'do { "test" }while($true)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if no space between } and until" {
            $def = 'do { "test" }until($false)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if there is space between } and while" {
            $def = 'do { "test" } while($true)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find a violation if there is space between } and until" {
            $def = 'do { "test" } until($false)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }
    }

    Context "When checking unary operators" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if no space after -not operator" {
            $def = 'if (-not$true) { }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if no space after -bnot operator" {
            $def = '$x = -bnot$value'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if space after -not operator" {
            $def = 'if (-not $true) { }'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find a violation for unary operator in method call" {
            $def = '$foo.bar(-$value)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should not find a violation for unary operator in property access" {
            $def = '$object.Property(-$x)'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -BeNullOrEmpty
        }

        It "Should find a violation for unary operator not in method call context" {
            $def = 'if(-not$x) { }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Should handle multiple unary operators in same expression" {
            $def = 'while(-not$a -and -not$b) { }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 2
        }
    }

    Context "Invoke-Formatter validates do-while/do-until and unary operator fixes" {
        BeforeAll {
            $settings = @{
                Rules = @{
                    PSUseConsistentWhitespace = @{
                        Enable = $true
                        CheckOpenBrace = $true
                        CheckOpenParen = $true
                        CheckOperator = $true
                    }
                }
            }
        }

        It "Should format the original bug repro correctly" {
            $def = @'
if(-not$false) {
    do{
        "Hello!"
    }until(
        $True
    )
    do{
        "Oh, hi!"
    }while(
        -not$True
    )
    while(-not$True) {
        "This won't show up."
    }
}
'@
            $expected = @'
if (-not $false) {
    do {
        "Hello!"
    } until (
        $True
    )
    do {
        "Oh, hi!"
    } while (
        -not $True
    )
    while (-not $True) {
        "This won't show up."
    }
}
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should add space between } and while" {
            $def = 'do { Get-Process }while($true)'
            $expected = 'do { Get-Process } while ($true)'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should add space between } and until" {
            $def = 'do { Get-Process }until($false)'
            $expected = 'do { Get-Process } until ($false)'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should add space after -not operator" {
            $def = 'if (-not$variable) { "test" }'
            $expected = 'if (-not $variable) { "test" }'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should add space after -bnot operator" {
            $def = '$result = -bnot$value'
            $expected = '$result = -bnot $value'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should not add space after unary minus in method call" {
            $def = '$object.Method(-$value)'
            $expected = '$object.Method(-$value)'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }

        It "Should handle all unary operators correctly" {
            $def = 'if (-not$a -and -bnot$b -and $c.Method(-$d)) { }'
            $expected = 'if (-not $a -and -bnot $b -and $c.Method(-$d)) { }'
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -BeExactly $expected
        }
    }
}