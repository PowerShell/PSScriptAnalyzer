Import-Module PSScriptAnalyzer 
$missingMessage = "The member 'ModuleVersion' is not present in the module manifest."
$missingName = "PSMissingModuleManifestField"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\TestBadModule\TestBadModule.psd1 | Where-Object {$_.RuleName -eq $missingName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1 | Where-Object {$_.RuleName -eq $missingName}

Describe "MissingRequiredFieldModuleManifest" {
    Context "When there are violations" {
        It "has 1 missing required field module manifest violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations.Message | Should Match $missingMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}

