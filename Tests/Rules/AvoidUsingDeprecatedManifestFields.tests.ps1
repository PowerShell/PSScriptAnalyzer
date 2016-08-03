Import-Module PSScriptAnalyzer
$violationName = "PSAvoidUsingDeprecatedManifestFields"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer "$directory\TestBadModule\TestDeprecatedManifestFields.psd1" | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer "$directory\TestGoodModule\TestGoodModule.psd1" | Where-Object {$_.RuleName -eq $violationName}
$noViolations2 = Invoke-ScriptAnalyzer "$directory\TestGoodModule\TestDeprecatedManifestFieldsWithVersion2.psd1" | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingDeprecatedManifestFields" {
    Context "When there are violations" {
        It "has 1 violations" {
            $violations.Count | Should Be 1
        }
    }

    Context "When there are no violations" {
        It "returns no violations if no deprecated fields are used" {
            $noViolations.Count | Should Be 0
        }

        It "returns no violations if deprecated fields are used but psVersion is less than 3.0" {
            $noViolations.Count | Should Be 0
        }
    }

    Context "When given a non module manifest file" {
        It "does not flag a PowerShell data file" {
            Invoke-ScriptAnalyzer `
                -Path "$directory/TestManifest/PowerShellDataFile.psd1" `
                -IncludeRule "PSAvoidUsingDeprecatedManifestFields" `
                -OutVariable ruleViolation
            $ruleViolation.Count | Should Be 0
        }
    }
}