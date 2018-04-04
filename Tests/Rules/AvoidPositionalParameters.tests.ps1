Import-Module PSScriptAnalyzer

Describe "AvoidPositionalParameters" {
    BeforeAll {
        $directory = $PSScriptRoot
        $violationName = "PSAvoidUsingPositionalParameters"
        $violation = Invoke-ScriptAnalyzer -ScriptDefinition 'Get-Command "abc" 4 4.3'
        $noViolations = Invoke-ScriptAnalyzer $directory\AvoidPositionalParametersNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
        $noViolationsDSC = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\serviceconfigdisabled.ps1 | Where-Object {$_.RuleName -eq $violationName}
    }
    Context "When there are violations" {
        It "has 1 avoid positional parameters violation" {
            @($violation).Count | Should -Be 1
            $violation.RuleName | Should -Be $violationName
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
        
        It "returns no violations for DSC configuration" {
            $noViolationsDSC.Count | Should -Be 0
        }
    }
}
