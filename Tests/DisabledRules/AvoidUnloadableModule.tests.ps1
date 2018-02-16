Import-Module PSScriptAnalyzer 
$unloadableMessage = [regex]::Escape("Cannot load the module TestBadModule that the file TestBadModule.psd1 is in.")
$unloadableName = "PSAvoidUnloadableModule"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\TestBadModule\TestBadModule.psd1 | Where-Object {$_.RuleName -eq $unloadableName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1 | Where-Object {$_.RuleName -eq $unloadableName}

Describe "AvoidUnloadableModule" {
    Context "When there are violations" {
        It "has 1 unloadable module violation" {
            $violations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $violations.Message | Should -Match $unloadableMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}