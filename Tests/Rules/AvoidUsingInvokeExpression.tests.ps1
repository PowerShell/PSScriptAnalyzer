Import-Module PSScriptAnalyzer
$violationMessage = "Invoke-Expression is used. Please remove Invoke-Expression from script and find other options instead."
$violationName = "PSAvoidUsingInvokeExpression"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingInvokeExpression.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidConvertToSecureStringWithPlainTextNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingInvokeExpression" {
    Context "When there are violations" {
        It "has 2 Avoid Using Invoke-Expression violations" {
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