$ruleName = "PSUseCompatibleCmdlets"
$directory = Split-Path $MyInvocation.MyCommand.Path -Parent
$testRootDirectory = Split-Path -Parent $directory
$ruleTestDirectory = Join-Path $directory 'UseCompatible/Cmdlets/'
$violationFilePath = Join-Path $ruleTestDirectory 'ScriptWithViolation.ps1'
$settingsFilePath =  [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettings.psd1');
$settingsLinux = [System.IO.Path]::Combine($ruleTestDirectory, 'PSScriptAnalyzerSettingsLinux.psd1');

Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

Describe "UseCompatibleCmdlets" {
    Context "script has violation" {
        It "detects violation" { 
            $diagnosticRecords = Invoke-ScriptAnalyzer -Path $violationFilePath -Settings $settingsFilePath
            $diagnosticRecords.Count | Should Be 1
        }
    }

    Function Test-Command
    {
        param (
            [Parameter(ValueFromPipeline)]
            $command,
            $expectedViolations,
            $settingsFile
        )
        process
        {
            It ("found {0} violations for '{1}'" -f $expectedViolations, $command) {
                Invoke-ScriptAnalyzer -ScriptDefinition $command -IncludeRule $ruleName -Settings $settingsFile | `
                    Get-Count | `
                    Should -Be $expectedViolations
            }
        }
    }

    Context "Microsoft.PowerShell.Core" {
         @('Enter-PSSession', 'Foreach-Object', 'Get-Command') | `
            Test-Command -ExpectedViolations 0 -SettingsFile $settingsFilePath
    }

    Context "Non-builtin commands" {
        @('get-foo', 'get-bar', 'get-baz') | `
            Test-Command -ExpectedViolations 0 -SettingsFile $settingsFilePath
    }

    Context "Aliases" {
        @('where', 'select', 'cd') | `
            Test-Command -ExpectedViolations 0 -SettingsFile $settingsFilePath
    }

    Context "Commands present in reference platform but not in target platform" {
        @("Start-VM", "New-SmbShare", "Get-Disk") | `
            Test-Command -ExpectedViolations 1 -SettingsFile $settingsFilePath
    }

    Context "Known issue cmdlets" {
        @("Register-WmiEvent", "Remove-Event", "Unregister-Event") | `
            Test-Command -ExpectedViolations 1 -SettingsFile $settingsLinux 
    }
}
