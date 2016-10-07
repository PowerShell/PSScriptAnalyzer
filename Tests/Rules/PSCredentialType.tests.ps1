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

        It "detects attributes on the same line" {
            $scriptDef = @'
function Get-Credential
{
    param(
    [PSCredential][System.Management.Automation.Credential()]
    $Credential
    )
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule $violationName
            $violations.Count | Should Be 0
        }

    }

    $expectedViolationCount = 0
    if ($PSVersionTable.PSVersion -lt [Version]'5.0.0')
    {
        $expectedViolationCount = 1
    }
    Context ("When there are {0} violations" -f $expectedViolationCount) {
        It ("returns {0} violations" -f $expectedViolationCount) {
            $noViolations.Count | Should Be $expectedViolationCount
        }
    }
}