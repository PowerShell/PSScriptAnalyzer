Import-Module PSScriptAnalyzer
$ruleName = "PSPlaceOpenBrace"

Describe "PlaceOpenBrace on same line" {
    Context "When a open brace is on a new line" {
        BeforeAll{
            $def = @'
function foo ($param1)
{

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -IncludeRule $ruleName
        }

        It "Should find a violation" {
            $violations.Count | Should Be 1
        }

        It "Should mark only the open brace" {
            $violations[0].Extent.Text | Should Be '{'
        }
    }
}
