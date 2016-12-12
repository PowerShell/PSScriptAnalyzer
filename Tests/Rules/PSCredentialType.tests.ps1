$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')
Import-Module PSScriptAnalyzer

$violationMessage = "The Credential parameter in 'Credential' must be of type PSCredential. For PowerShell 4.0 and earlier, please define a credential transformation attribute, e.g. [System.Management.Automation.Credential()], after the PSCredential type attribute."
$violationName = "PSUsePSCredentialType"
$violations = Invoke-ScriptAnalyzer $directory\PSCredentialType.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\PSCredentialTypeNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "PSCredentialType" {
    Context "When there are violations" {
        $expectedViolations = 1
        if ((Test-PSVersionV3) -or (Test-PSVersionV4)) {
            $expectedViolations = 2
        }
        It ("has {0} PSCredential type violation" -f $expectedViolations) {
            $violations.Count | Should Be $expectedViolations
        }

        It "has the correct description message" {
            $violations[0].Message | Should Be $violationMessage
        }

        It "detects attributes on the same line without space" {
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

    Context ("When there are no violations") {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}