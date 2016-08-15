$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$missingMessage = "The member 'ModuleVersion' is not present in the module manifest."
$missingMemberRuleName = "PSMissingModuleManifestField"
$violationFilepath = Join-Path $directory "TestBadModule\TestBadModule.psd1"
$violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object {$_.RuleName -eq $missingMemberRuleName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1 | Where-Object {$_.RuleName -eq $missingMemberRuleName}
$noHashtableFilepath = Join-Path $directory "TestBadModule\NoHashtable.psd1"

Describe "MissingRequiredFieldModuleManifest" {
    BeforeAll {
        Import-Module (Join-Path $directory "PSScriptAnalyzerTestHelper.psm1") -Force
    }

    AfterAll{
        Remove-Module PSScriptAnalyzerTestHelper
    }

    Context "When there are violations" {
        It "has 1 missing required field module manifest violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations.Message | Should Match $missingMessage
        }

        $numExpectedCorrections = 1
        It "has $numExpectedCorrections suggested corrections" {
            $violations.SuggestedCorrections.Count | Should Be $numExpectedCorrections
        }

    # On Linux, mismatch in line endings cause this to fail
	It "has the right suggested correction" -Skip:(Test-PSEditionCoreCLRLinux) {
	   $expectedText = @'
# Version number of this module.
ModuleVersion = '1.0.0.0'
'@
            $violations[0].SuggestedCorrections[0].Text | Should Match $expectedText
            Get-ExtentText $violations[0].SuggestedCorrections[0] $violationFilepath | Should Match ""
    }
}

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }

    Context "When an .psd1 file doesn't contain a hashtable" {
        It "does not throw exception" {
            {Invoke-ScriptAnalyzer -Path $noHashtableFilepath -IncludeRule $missingMemberRuleName} | Should Not Throw
        }
    }

    Context "Validate the contents of a .psd1 file" {
        It "detects a valid module manifest file" {
            $filepath = Join-Path $directory "TestManifest/ManifestGood.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should Be $true
        }

        It "detects a .psd1 file which is not module manifest" {
            $filepath = Join-Path $directory "TestManifest/PowerShellDataFile.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should Be $false
        }

        It "detects valid module manifest file for PSv5" {
            $filepath = Join-Path $directory "TestManifest/ManifestGoodPsv5.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"5.0.0") | Should Be $true
        }

        It "does not validate PSv5 module manifest file for PSv3 check" {
            $filepath = Join-Path $directory "TestManifest/ManifestGoodPsv5.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"3.0.0") | Should Be $false
        }

        It "detects valid module manifest file for PSv4" {
            $filepath = Join-Path $directory "TestManifest/ManifestGoodPsv4.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"4.0.0") | Should Be $true
        }

        It "detects valid module manifest file for PSv3" {
            $filepath = Join-Path $directory "TestManifest/ManifestGoodPsv3.psd1"
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Helper]::IsModuleManifest($filepath, [version]"3.0.0") | Should Be $true
        }
    }

    Context "When given a non module manifest file" {
        It "does not flag a PowerShell data file" {
            Invoke-ScriptAnalyzer `
                -Path "$directory/TestManifest/PowerShellDataFile.psd1" `
                -IncludeRule "PSMissingModuleManifestField" `
                -OutVariable ruleViolation
            $ruleViolation.Count | Should Be 0
        }
    }
}

