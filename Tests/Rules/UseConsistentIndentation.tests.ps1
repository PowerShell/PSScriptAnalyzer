Import-Module PSScriptAnalyzer
$settings = @{
    IncludeRules = @("PSUseConsistentIndentation")
    Rules = @{
        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
        }
    }
}


Describe "UseConsistentIndentation" {
    Context "When top level indentation is not consistent" {
        BeforeAll {
            $def = @'
 function foo ($param1)
{

}
'@

            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should detect a violation" {
            $violations.Count | Should Be 1
        }
    }

    Context "When nested indenation of is not consistent" {
        BeforeAll {
            $def = @'
function foo ($param1)
{
"abc"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should detect a violation" {
            $violations.Count | Should Be 1
        }
    }
}
