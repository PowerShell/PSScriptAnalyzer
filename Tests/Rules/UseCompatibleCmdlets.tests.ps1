$ruleName = "PSUseCompatibleCmdlets"
$directory = Split-Path $MyInvocation.MyCommand.Path -Parent
$testRootDirectory = Split-Path -Parent $directory
$ruleTestDirectory = Join-Path $directory 'UseCompatibleCmdlets'

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

Describe "UseCompatibleCmdlets" {
    Context "script has violation" {
        It "detects violation" {
            $violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
            $settingsFilePath =  [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettings.psd1');
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -IncludeRule $ruleName -Settings $settingsFilePath
            $diagnosticRecords.Count | Should Be 1
        }
    }

    Function Test-Command
    {
        param (
            [Parameter(ValueFromPipeline)]
            $command,
            $settings,
            $expectedViolations
        )
        process
        {
            It ("found {0} violations for '{1}'" -f $expectedViolations, $command) {
                Invoke-ScriptAnalyzer -ScriptDefinition $command -IncludeRule $ruleName -Settings $settings | `
                    Get-Count | `
                    Should Be $expectedViolations
            }
        }
    }

    $settings = @{rules=@{PSUseCompatibleCmdlets=@{compatibility=@("core-6.0.0-alpha-windows")}}}

    Context "Microsoft.PowerShell.Core" {
         @('Enter-PSSession', 'Foreach-Object', 'Get-Command') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Non-builtin commands" {
        @('get-foo', 'get-bar', 'get-baz') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Aliases" {
        @('where', 'select', 'cd') | `
            Test-Command -Settings $settings -ExpectedViolations 0
    }

    Context "Commands present in reference platform but not in target platform" {
        @("Start-VM", "New-SmbShare", "Get-Disk") | `
            Test-Command -Settings $settings -ExpectedViolations 1
    }
}
