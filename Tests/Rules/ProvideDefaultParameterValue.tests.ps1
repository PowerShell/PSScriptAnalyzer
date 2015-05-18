Import-Module PSScriptAnalyzer
$violationName = "PSProvideDefaultParameterValue"
$violationMessage = "Parameter 'Param2' is not initialized. Parameters must have a default value. To fix a violation of this rule, please specify a default value for all parameters"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\ProvideDefaultParameterValue.ps1 | Where-Object {$_.RuleName -match $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\ProvideDefaultParameterValueNoViolations.ps1

Describe "ProvideDefaultParameters" {
    Context "When there are violations" {
        It "has 2 provide default parameter value violation" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}