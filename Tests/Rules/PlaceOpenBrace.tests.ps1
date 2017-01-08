Import-Module PSScriptAnalyzer
$settings = @{
    IncludeRules = @("PSPlaceOpenBrace")
    Rules = @{
        PSPlaceOpenBrace = @{
            Enable = $true
            OnSameLine = $true
        }
    }
}


Describe "PlaceOpenBrace on same line" {
    Context "When an open brace must be on the same line" {
        BeforeAll{
            $def = @'
function foo ($param1)
{

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should Be '{'
        }
    }

    Context "When an open brace must be on a new line" {
        BeforeAll{
            $def = @'
function foo ($param1) {

}
'@

            $settingsNewLine = $settings.Clone()
            $settingsNewLine["Rules"]["PSPlaceOpenBrace"]["OnSameLine"] = $false
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settingsNewLine
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should Be '{'
        }
    }
}
