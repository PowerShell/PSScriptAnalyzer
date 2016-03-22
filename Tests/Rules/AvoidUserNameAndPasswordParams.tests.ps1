Import-Module PSScriptAnalyzer

$violationMessage = "Function 'TestFunction' has both Username and Password parameters. Either set the type of the Password parameter to SecureString or replace the Username and Password parameters with a Credential parameter of type PSCredential. If using a Credential parameter in PowerShell 4.0 or earlier, please define a credential transformation attribute after the PSCredential type attribute."
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
            $violations[0].Message | Should Be $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}