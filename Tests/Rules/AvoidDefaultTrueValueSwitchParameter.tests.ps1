Import-Module PSScriptAnalyzer
$violationMessage = "File 'AvoidDefaultTrueValueSwitchParameter.ps1' has a switch parameter default to true."
$violationName = "PSAvoidDefaultValueSwitchParameter"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidDefaultTrueValueSwitchParameter.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidDefaultTrueValueSwitchParameterNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidDefaultTrueValueSwitchParameter" {
    Context "When there are violations" {
        It "has 2 avoid using switch parameter default to true violation" {
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