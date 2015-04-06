Import-Module ScriptAnalyzer
$violationMessage = "Function Verb-Noun in file AvoidShouldContinueShouldProcessWithoutForce.ps1 uses ShouldContinue or ShouldProcess but does not have a boolean force parameter. The force parameter will allow users of the script to bypass ShouldContinue prompt"
$violationName = "PSAvoidShouldContinueShouldProcessWithoutForce"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidShouldContinueShouldProcessWithoutForce.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidShouldContinueShouldProcessWithoutForce" {
    Context "When there are violations" {
        It "has 4 avoid ShouldContinue or ShouldProcess without boolean Force parameter violations" {
            $violations.Count | Should Be 4
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
