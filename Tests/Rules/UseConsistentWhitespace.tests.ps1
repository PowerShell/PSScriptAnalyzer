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

    Context "When there is whitespace around operators" {
        BeforeAll {
            $ruleConfiguration.CheckInnerBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
            $ruleConfiguration.CheckPipe = $false
            $ruleConfiguration.CheckSeparator = $false
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

        It "Should find a violation if there is no space around an arithmetic operator" {
            $def = '$z = 3+4'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '+' ' + '
        }

        It "Should find a violation if there is no space around a bitwise operator" {
            $def = '$value = 7-band3'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-band' ' -band '
        }

        It "Should find a violation if there is no space around an equality comparison operator" {
            $def = '$obviously = 3-lt4'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-lt' ' -lt '
        }

        It "Should find a violation if there is no space around a matching operator" {
            $def = '$shouldSend = $fromAddress-like"*@*.com"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-like' ' -like '
        }

        It "Should find a violation if there is no space around replace operator" {
            $def = '$a = "string"-replace"ing", "aight"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-replace' ' -replace '
        }

        It "Should find a violation if there is no space around a containment operator" {
            $def = 'if ("filename.txt"-in$FileList) { }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-in' ' -in '
        }

        It "Should find a violation if there is no space around a type operator" {
            $def = '$HoustonWeHaveAProblem = $a-isnot[System.Object]'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-isnot' ' -isnot '
        }

        It "Should find a violation if there is no space around a logical operator" {
            $def = '$a = $b-xor$c'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-xor' ' -xor '
        }

        It "Should find a violation if there is no space around logical not operator but only on one side" {
            $def = '$lie = (-not$true)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space around redirection operator" {
            $def = '"hi">>secretmessage.txt'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '>>' ' >> '
        }

        It "Should find a violation if there is no space around binary split operator" {
            $def = '$numbers = "one:two:three"-split":"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-split' ' -split '
        }

        It "Should find a violation if there is no space around unary join operator but only on one side" {
            $def = 'ConvertFrom-Json (-join(dotnet gitversion))'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space around format operator" {
            $def = '"{0:X}"-f88'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '-f' ' -f '
        }

        It "Should find violations if there is no space around ternary operator" -Skip:($PSVersionTable.PSVersion -lt '7.0') {
            $def = '($a -is [System.Object])?3:4'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Should -HaveCount 2
            Test-CorrectionExtentFromContent $def $violations[0] 1 '?' ' ? '
            Test-CorrectionExtentFromContent $def $violations[1] 1 ':' ' : '
        }

        It "Should not find a violation if there is no space around pipeline operator" {
            $def = 'Get-It|Forget-It'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should find a violation if there is no space around pipeline chain operator" -Skip:($PSVersionTable.PSVersion -lt '7.0') {
            $def = 'Start-Service $ServiceName||Write-Error "Could not start $ServiceName"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '||' ' || '
        }

        It "Should find a violation if there is no space around null coalescing operator" -Skip:($PSVersionTable.PSVersion -lt '7.0') {
            $def = '${a}??3'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '??' ' ?? '
        }

        It "Should find a violation if there is no space around call operator" {
            $def = '(&$ScriptFile)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space around background operator" -Skip:($PSVersionTable.PSVersion -lt '6.0') {
            $def = '(Get-LongThing&)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is no space around dot sourcing operator" {
            $def = '(.$ScriptFile)'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if there is no space around member access operator" {
            $def = '$PSVersionTable.PSVersion'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is no space around comma operator" {
            $def = '$somenumbers = 3,4,5'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is no space around prefix operator" {
            $def = '--$counter'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is no space around postfix operator" {
            $def = '$counter++'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
        }

        It "Should not find a violation if there is no space around exclaim operator" {
            $def = 'if(!$true){ "FALSE!@!!!!" }'
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should -Be $null
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

        It "Should not find no violation if there is always 1 space between parameters except when using colon syntax" {
            $def = 'foo -bar $baz @splattedVariable -bat -parameterName:$parameterValue'
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
    }
}
