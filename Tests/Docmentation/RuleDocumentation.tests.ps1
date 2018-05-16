$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
$repoRootDirectory = Split-Path -Parent $testRootDirectory
$ruleDocDirectory = Join-Path $repoRootDirectory RuleDocumentation

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

Describe "Validate rule documentation files" {
    BeforeAll {
        $docs = Get-ChildItem $ruleDocDirectory/*.md -Exclude README.md |
            ForEach-Object { "PS" + $_.BaseName} | Sort-Object
        $rules = Get-ScriptAnalyzerRule | ForEach-Object RuleName | Sort-Object
        $res = Compare-Object -ReferenceObject $rules -DifferenceObject $docs
    }
    It "Every rule must have a rule documentation file" {
        Write-Host $res
        $res | Where-Object SideIndicator -eq "<=" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }
    It "Every rule documentation file must have a rule" {
        $res | Where-Object SideIndicator -eq "=>" | Foreach-Object InputObject | Should -BeNullOrEmpty
    }
}
