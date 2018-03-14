Import-Module PSScriptAnalyzer
$ruleName = "PSAvoidPlusEqualsOperatorOnArraysOrStrings"

Describe "AvoidPlusEqualsOperatorOnArraysOrStrings" {
    Context "When there are violations" {
        It "assignment inside if statement causes warning" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$array=@(); $list | ForEach-Object { $array += $stuff }' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        It "assignment inside if statement causes warning when when wrapped in command expression" {
            $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$array=@(); $list | Where-Object { if($_ -eq $true){ $array += 4 }}' | Where-Object {$_.RuleName -eq $ruleName}
            $warnings.Count | Should -Be 1
        }

        Context "When there are no violations" {
            It "returns no violations when there is no equality operator" {
                $warnings = Invoke-ScriptAnalyzer -ScriptDefinition '$array = Get-UnknownObject; $array += $stuff' | Where-Object {$_.RuleName -eq $ruleName}
                $warnings.Count | Should -Be 0
            }
        }
    }
}
