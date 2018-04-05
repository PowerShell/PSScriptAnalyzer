$ruleName = 'PSAvoidDefaultValueForMandatoryParameter'

Describe "AvoidDefaultValueForMandatoryParameter" {
    Context "When there are violations" {
        It "has 1 provide default value for mandatory parameter violation with CmdletBinding" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'Function foo{ [CmdletBinding()]Param([Parameter(Mandatory)]$Param1=''defaultValue'') }' |
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }

        It "has 1 provide default value for mandatory=$true parameter violation without CmdletBinding" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'Function foo{ Param([Parameter(Mandatory=$true)]$Param1=''defaultValue'') }' |
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 1
        }
    }

    Context "When there are no violations" {
        It "has 1 provide default value for mandatory parameter violation" {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition 'Function foo{ Param([Parameter(Mandatory=$false)]$Param1=''val1'', [Parameter(Mandatory)]$Param2=''val2'', $Param3=''val3'') }' |
                Where-Object { $_.RuleName -eq $ruleName }
            $violations.Count | Should -Be 0
        }
    }
}
