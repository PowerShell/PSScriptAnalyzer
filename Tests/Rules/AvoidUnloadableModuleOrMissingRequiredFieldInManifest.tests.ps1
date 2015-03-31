Import-Module ScriptAnalyzer 
$missingMessage = "The member 'ModuleVersion' is not present in the module manifest."
$missingName = "PSMissingModuleManifestField"
$unloadableMessage = [regex]::Escape("Cannot load the module TestBadModule that the file TestBadModule.psd1 is in.")
$unloadableName = "PSAvoidUnloadableModule"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\TestBadModule\TestBadModule.psd1
$missingViolations = $violations | Where-Object {$_.RuleName -eq $missingName}
$unloadableViolations = $violations | Where-Object {$_.RuleName -eq $unloadableName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1
$noMissingViolations = $noViolations | Where-Object {$_.RuleName -eq $missingName}
$noUnloadableViolations = $noViolations | Where-Object {$_.RuleName -eq $unloadableName}

Describe "AvoidUnloadableModule" {
    Context "When there are violations" {
        It "has 1 unloadable module violation" {
            $unloadableViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $unloadableViolations.Message | Should Match $unloadableMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noUnloadableViolations.Count | Should Be 0
        }
    }
}

Describe "MissingRequiredFieldModuleManifest" {
    Context "When there are violations" {
        It "has 1 missing required field module manifest violation" {
            $missingViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $missingViolations.Message | Should Match $missingMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noMissingViolations.Count | Should Be 0
        }
    }
}

