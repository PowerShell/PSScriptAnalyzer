Import-Module PSScriptAnalyzer
$ruleName = "PSUseWhitespace"
$ruleConfiguration = @{
    Enable = $true
    CheckOpenBrace = $false
    CheckOpenParen = $false
    CheckOperator = $false
}

$settings = @{
    IncludeRules = @($ruleName)
    Rules = @{
        PSUseWhitespace = $ruleConfiguration
    }
}

Describe "UseWhitespace" {
    Context "When an open brace follows a keyword" {
        BeforeAll {

            $ruleConfiguration.CheckOpenBrace = $true
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOperator = $false
        }

        It "Should find a violation if an open brace does not follow whitespace" {
            $def = @'
if ($true){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should find no violation if an open brace follows a whitespace" {
            $def = @'
if($true) {}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0

        }

    }

    Context "When a parenthesis follows a keyword" {
        BeforeAll {
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOpenParen = $true
            $ruleConfiguration.CheckOperator = $false
        }

        It "Should find no violation if an open brace follows a whitespace" {
            $def = @'
if($true) {}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should not find a violation if no whitespace is present before open paren of a function definition" {
            $def = @'
function foo($param1) {

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0
        }

        It "Should not find a violation if no whitespace is present before open paren of a param block" {
            $def = @'
function foo() {
    param( )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0
        }

        It "Should not find a violation if no whitespace is present in a nested open paren" {
            $def = @'
function foo($param) {
    ((Get-Process))
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0
        }
    }

    Context "When there is whitespace around assignment and binary operators" {
        BeforeAll {
            $ruleConfiguration.CheckOpenParen = $false
            $ruleConfiguration.CheckOpenBrace = $false
            $ruleConfiguration.CheckOperator = $true
        }

        It "Should find a violation if no whitespace around an assignment operator" {
            $def = @'
$x=1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should find a violation if no whitespace before an assignment operator" {
            $def = @'
$x= 1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should find a violation if no whitespace after an assignment operator" {
            $def = @'
$x =1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should find a violation if there is a whitespaces not of size 1 around an assignment operator" {
            $def = @'
$x  =  1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 1
        }

        It "Should find no violation if there are whitespaces of size 1 around an assignment operator" {
            $def = @'
$x = 1
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should Be 0
        }
    }
}