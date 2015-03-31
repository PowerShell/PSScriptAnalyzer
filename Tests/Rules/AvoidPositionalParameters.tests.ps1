Import-Module ScriptAnalyzer
$violationMessage = "Cmdlet Get-Content has positional parameter. Please use named parameters instead of positional parameters when calling a command."
$violationName = "PSAvoidUsingPositionalParameters"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidPositionalParameters.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidPositionalParametersNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolationsDSC = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\serviceconfigdisabled.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidPositionalParameters" {
    Context "When there are violations" {
        It "has 4 avoid positional parameters violation" {
            $violations.Count | Should Be 5
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
        
        It "returns no violations for DSC configuration" {
            $noViolationsDSC.Count | Should Be 0
        }
    }
}