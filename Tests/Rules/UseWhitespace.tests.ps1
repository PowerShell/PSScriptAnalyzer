Import-Module PSScriptAnalyzer
$ruleName = "PSUseWhitespace"
$ruleConfiguration = @{
    Enable = $true
    CheckOpenBrace = $true
}

$settings = @{
    IncludeRules = @($ruleName)
    Rules = @{
        PSUseWhitespace = $ruleConfiguration
    }
}

Describe "UseWhitespace" {
    Context "When no whitespace is present before if block open brace" {
        BeforeAll {
            $def = @'
if ($true){}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }
    }

    Context "When no whitespace is present before open paren of an if block" {
        BeforeAll {
            $def = @'
if($true) {}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }
    }

    Context "When no whitespace is present before open paren of a function definition" {
        BeforeAll {
            $def = @'
function foo($param1) {

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

                It "Should not find a violation" {
            $violations.Count | Should Be 0
        }
    }

    Context "When no whitespace is present before open of a param block" {
        BeforeAll {
            $def = @'
function foo() {
    param( )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

                It "Should not find a violation" {
            $violations.Count | Should Be 0
        }
    }

        Context "When no whitespace is present in a nested open paren" {
        BeforeAll {
            $def = @'
function foo($param) {
    ((Get-Process))
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

                It "Should not find a violation" {
            $violations.Count | Should Be 0
        }
    }
}
