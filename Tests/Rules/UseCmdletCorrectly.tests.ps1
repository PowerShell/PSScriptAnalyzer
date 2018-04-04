
Describe "UseCmdletCorrectly" {
    Context "When there are violations" {
        It "has 1 Use Cmdlet Correctly violation" {
            $violationName = "PSUseCmdletCorrectly"
            $violation = Invoke-ScriptAnalyzer -ScriptDefinition 'Write-Warning;Write-Warning -Message "a warning"'
            $violation.Count | Should -Be 1
            $violation.RuleName | Should -Be $violationName
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $directory = $PSScriptRoot
            $noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 
            $noViolations.Count | Should -Be 0
        }
    }
}
