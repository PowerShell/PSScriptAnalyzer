Import-Module PSScriptAnalyzer
$violationName = "PSAvoidDefaultValueForMandatoryParameter"
$violationMessage = "Mandatory Parameter 'Param1' is initialized in the Param block. To fix a violation of this rule, please leave it unintialized."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer "$directory\AvoidDefaultValueForMandatoryParameter.ps1" | Where-Object {$_.RuleName -match $violationName}
$noViolations = Invoke-ScriptAnalyzer "$directory\AvoidDefaultValueForMandatoryParameterNoViolations.ps1"

Describe "AvoidDefaultValueForMandatoryParameter" {
    Context "When there are violations" {
        It "has 1 provide default value for mandatory parameter violation" {
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