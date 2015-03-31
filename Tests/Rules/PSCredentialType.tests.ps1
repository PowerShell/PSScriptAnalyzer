Import-Module ScriptAnalyzer 
$violationMessage = "The Credential parameter in Credential must be of the type PSCredential."
$violationName = "PSUsePSCredentialType"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\PSCredentialType.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\PSCredentialTypeNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "PSCredentialType" {
    Context "When there are violations" {
        It "has 1 PSCredential type violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations.Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}