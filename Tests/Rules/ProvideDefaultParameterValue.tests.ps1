Import-Module PSScriptAnalyzer
$violationName = "PSProvideDefaultParameterValue"
$violationMessage = "Parameter 'Param1' is not initialized. Non-global variables must be initialized. To fix a violation of this rule, please initialize non-global variables."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\ProvideDefaultParameterValue.ps1 | Where-Object {$_.RuleName -match $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\ProvideDefaultParameterValueNoViolations.ps1

Describe "ProvideDefaultParameters" {
    Context "When there are violations" {
        It "has 1 provide default parameter value violation" {
            $violations.Count | Should Be 1
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