Import-Module PSScriptAnalyzer 
$violationMessage = "The Credential parameter in 'Credential' must be of the type PSCredential with CredentialAttribute where PSCredential comes before CredentialAttribute."
$violationName = "PSUsePSCredentialType"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\PSCredentialType.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\PSCredentialTypeNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "PSCredentialType" {
    Context "When there are violations" {
        It "has 2 PSCredential type violation" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}