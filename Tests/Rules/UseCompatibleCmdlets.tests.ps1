Import-Module PSScriptAnalyzer
$ruleName = "UseCompatibleCmdlets"
$directory = Split-Path $MyInvocation.MyCommand.Path -Parent
$ruleTestDirectory = Join-Path $directory 'UseCompatibleCmdlets'


Describe "UseCompatibleCmdlets" {
    Context "script has violation" {
        It "detects violation" {
            $violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
            $settingsFilePath =  Join-Path $ruleTestDirectory 'PSScriptAnalyzerSettings.psd1'
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings
            $diagnosticRecords.Count | Should Be 1
        }
    }
}
