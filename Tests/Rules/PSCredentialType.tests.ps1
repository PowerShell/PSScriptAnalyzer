Import-Module PSScriptAnalyzer 
$violationMessage = "The Credential parameter in 'Credential' must be of type PSCredential. For PowerShell 4.0 and earlier, please define a credential transformation attribute, e.g. [System.Management.Automation.Credential()], after the PSCredential type attribute."
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
            $violations[0].Message | Should Be $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}