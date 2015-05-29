Import-Module PSScriptAnalyzer
$violationName = "PSAvoidUsingDeprecatedManifestFields"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\TestBadModule\TestDeprecatedManifestFields.psd1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingDeprecatedManifestFields" {
    Context "When there are violations" {
        It "has 1 violations" {
            $violations.Count | Should Be 1
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}