Import-Module PSScriptAnalyzer 
$missingMessage = "The member 'ModuleVersion' is not present in the module manifest."
$missingName = "PSMissingModuleManifestField"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violationFilepath = Join-Path $directory "TestBadModule\TestBadModule.psd1"
$violations = Invoke-ScriptAnalyzer $violationFilepath | Where-Object {$_.RuleName -eq $missingName}
$noViolations = Invoke-ScriptAnalyzer $directory\TestGoodModule\TestGoodModule.psd1 | Where-Object {$_.RuleName -eq $missingName}

Describe "MissingRequiredFieldModuleManifest" {
    BeforeAll {
        Import-Module (Join-Path $directory "PSScriptAnalyzerTestHelper.psm1")
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
	

	It "has the right suggested correction" {	
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
}

