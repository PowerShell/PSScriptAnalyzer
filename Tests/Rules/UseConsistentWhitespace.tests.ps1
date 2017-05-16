$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$ruleName = "PSUseConsistentWhitespace"
$ruleConfiguration = @{
    Enable = $true
    CheckOpenBrace = $false
    CheckOpenParen = $false
    CheckOperator = $false
    CheckSeparator = $false
}

$settings = @{
    IncludeRules = @($ruleName)
    Rules = @{
        PSUseConsistentWhitespace = $ruleConfiguration
    }
}

Describe "UseWhitespace" {
    Context "When an open brace follows a keyword" {
        BeforeAll {
            $ruleConfiguration.CheckOpenBrace = $true
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if an open brace does not follow whitespace" {
            $def = @'
if ($true){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find violation if an open brace follows a whitespace" {
            $def = @'
if($true) {}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find violation if an open brace follows a foreach member invocation" {
            $def = @'
(1..5).foreach{$_}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find violation if an open brace follows a where member invocation" {
            $def = @'
(1..5).where{$_}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

    }

    Context "When a parenthesis follows a keyword" {
        BeforeAll {
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $true
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find violation in an if statement" {
            $def = @'
if($true) {}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation in a function definition" {
            $def = @'
function foo($param1) {

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find a violation in a param block" {
            $def = @'
function foo() {
    param( )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find a violation in a nested open paren" {
            $def = @'
function foo($param) {
    ((Get-Process))
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find a violation on a method call" {
            $def = @'
$x.foo("bar")
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }
    }

    Context "When there is whitespace around assignment and binary operators" {
        BeforeAll {
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
            $ruleConfiguration.CheckSeparator = $false
        }

        It "Should find a violation if no whitespace around an assignment operator" {
            $def = @'
$x=1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '=' ' = '
        }

        It "Should find a violation if no whitespace before an assignment operator" {
            $def = @'
$x= 1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if no whitespace after an assignment operator" {
            $def = @'
$x =1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should find a violation if there is a whitespaces not of size 1 around an assignment operator" {
            $def = @'
$x  =  1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '  =  ' ' = '
        }

        It "Should not find violation if there are whitespaces of size 1 around an assignment operator" {
            $def = @'
$x = @"
"abc"
"@
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find violation if there are whitespaces of size 1 around an assignment operator for here string" {
            $def = @'
$x = 1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find violation if there are no whitespaces around DotDot operator" {
            $def = @'
1..5
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }
    }

    Context "When a comma is not followed by a space" {
        BeforeAll {
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckSeparator = $true
        }

        It "Should find a violation" {
            $def = @'
$x = @(1,2)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if a space follows a comma" {
            $def = @'
$x = @(1, 2)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }
    }

    Context "When a semi-colon is not followed by a space" {
        BeforeAll {
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
            $ruleConfiguration.CheckSeparator = $true
        }

        It "Should find a violation" {
            $def = @'
$x = @{a=1;b=2}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            Test-CorrectionExtentFromContent $def $violations 1 '' ' '
        }

        It "Should not find a violation if a space follows a semi-colon" {
            $def = @'
$x = @{a=1; b=2}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find a violation if a new-line follows a semi-colon" {
            $def = @'
$x = @{
    a=1;
    b=2
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }

        It "Should not find a violation if a end of input follows a semi-colon" {
            $def = @'
$x = "abc";
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Should Be $null
        }


    }
}
