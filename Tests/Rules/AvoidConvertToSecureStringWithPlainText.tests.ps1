Import-Module PSScriptAnalyzer
Set-Alias ctss ConvertTo-SecureString
$violationMessage = "File 'AvoidConvertToSecureStringWithPlainText.ps1' uses ConvertTo-SecureString with plaintext. This will expose secure information. Encrypted standard strings should be used instead."
$violationName = "PSAvoidUsingConvertToSecureStringWithPlainText"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidConvertToSecureStringWithPlainText.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidConvertToSecureStringWithPlainTextNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidConvertToSecureStringWithPlainText" {
    Context "When there are violations" {
        It "has 3 ConvertTo-SecureString violations" {
            $violations.Count | Should Be 3
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