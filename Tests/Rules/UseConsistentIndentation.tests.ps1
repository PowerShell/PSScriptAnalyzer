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

    Context "When nested indenation is not consistent" {
        BeforeAll {
            $def = @'
function foo ($param1)
{
"abc"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }
    }

    Context "When a multi-line hashtable is provided" {
        BeforeAll {
            $def = @'
$hashtable = @{
a = 1
b = 2
    c = 3
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should Be 2
        }
    }

    Context "When a multi-line array is provided" {
        BeforeAll {
            $def = @'
$array = @(
1,
    2,
3)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should Be 2
        }
    }

    Context "When a param block is provided" {
        BeforeAll {
            $def = @'
param(
            [string] $param1,

[string]
    $param2,

        [string]
$param3
)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find violations" {
            $violations.Count | Should Be 4
        }
    }

    Context "When a sub-expression is provided" {
        BeforeAll {
            $def = @'
function foo {
    $x = $("abc")
    $x
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should not find a violations" {
            $violations.Count | Should Be 0
        }
    }

    Context "When a multi-line command is given" {
        It "Should find a violation if a pipleline element is not indented correctly" {
            $def = @'
get-process |
where-object {$_.Name -match 'powershell'}
'@
          $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
          $violations.Count | Should Be 1
        }

        It "Should not find a violation if a pipleline element is indented correctly" {
            $def = @'
get-process |
    where-object {$_.Name -match 'powershell'}
'@
          $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
          $violations.Count | Should Be 0
        }

        It "Should ignore comment in the pipleline" {
            $def = @'
  get-process |
    where-object Name -match 'powershell' | # only this is indented correctly
select Name,Id |
       format-list
'@
          $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
          $violations.Count | Should Be 3
        }
    }
}
