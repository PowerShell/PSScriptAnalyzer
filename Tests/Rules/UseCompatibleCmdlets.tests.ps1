Import-Module PSScriptAnalyzer
$ruleName = "PSUseCompatibleCmdlets"
$directory = Split-Path $MyInvocation.MyCommand.Path -Parent
$ruleTestDirectory = Join-Path $directory 'UseCompatibleCmdlets'

Describe "UseCompatibleCmdlets" {
    Context "script has violation" {
        It "detects violation" {
            $violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
            $settingsFilePath =  [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettings.psd1');
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings $settingsFilePath
            $diagnosticRecords.Count | Should Be 1
        }
    }
}
