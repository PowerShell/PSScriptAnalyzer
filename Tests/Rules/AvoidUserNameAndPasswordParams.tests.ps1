Import-Module PSScriptAnalyzer

$violationMessage = "Function 'TestFunction' has both username and password parameters. A credential parameter of type PSCredential with a CredentialAttribute where PSCredential comes before CredentialAttribute should be used."
$violationName = "PSAvoidUsingUserNameAndPasswordParams"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUserNameAndPasswordParams.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUserNameAndPasswordParamsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUserNameAndPasswordParams" {
    Context "When there are violations" {
        It "has 1 avoid username and password parameter violations" {
            $violations.Count | Should Be 1
        }

        It "has the correct violation message" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}