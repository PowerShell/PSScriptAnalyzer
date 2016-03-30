Import-Module PSScriptAnalyzer
$writeHostName = "PSMisleadingBacktick"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violationsFilepath = Join-Path $directory 'MisleadingBacktick.ps1'
$violations = Invoke-ScriptAnalyzer $violationsFilepath | Where-Object {$_.RuleName -eq $writeHostName}
$noViolations = Invoke-ScriptAnalyzer $directory\NoMisleadingBacktick.ps1 | Where-Object {$_.RuleName -eq $clearHostName}

Describe "Avoid Misleading Backticks" {
    Context "When there are violations" {
        It "has 5 misleading backtick violations" {
	   Import-Module .\PSScriptAnalyzerTestHelper.psm1
            $violations.Count | Should Be 5

	    $idx = 0
	    $violations[$idx].SuggestedCorrections.Count | Should Be 1
	    $violations[$idx].SuggestedCorrections[0].Text | Should Be ''
	    Get-ExtentText $violations[$idx].SuggestedCorrections[0] $violationsFilepath | Should BeExactly ' '

	    $idx = 1
	    $violations[$idx].SuggestedCorrections.Count | Should Be 1
	    $violations[$idx].SuggestedCorrections[0].Text | Should Be ''
	    Get-ExtentText $violations[$idx].SuggestedCorrections[0] $violationsFilepath | Should BeExactly ' '

	    $idx = 2
	    $violations[$idx].SuggestedCorrections.Count | Should Be 1
	    $violations[$idx].SuggestedCorrections[0].Text | Should Be ''
	    Get-ExtentText $violations[$idx].SuggestedCorrections[0] $violationsFilepath | Should BeExactly ' '

	    $idx = 3
	    $violations[$idx].SuggestedCorrections.Count | Should Be 1
	    $violations[$idx].SuggestedCorrections[0].Text | Should Be ''
	    Get-ExtentText $violations[$idx].SuggestedCorrections[0] $violationsFilepath | Should BeExactly '                     '

	    $idx = 4
	    $violations[$idx].SuggestedCorrections.Count | Should Be 1
	    $violations[$idx].SuggestedCorrections[0].Text | Should Be ''
	    Get-ExtentText $violations[$idx].SuggestedCorrections[0] $violationsFilepath | Should BeExactly '      '

        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}