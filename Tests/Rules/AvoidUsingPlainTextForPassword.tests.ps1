Import-Module PSScriptAnalyzer

$violationMessage = [regex]::Escape("Parameter '`$password' should use SecureString, otherwise this will expose sensitive information. See ConvertTo-SecureString for more information.")
$violationName = "PSAvoidUsingPlainTextForPassword"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violationsFilepath = Join-Path $directory 'AvoidUsingPlainTextForPassword.ps1'
$violations = Invoke-ScriptAnalyzer $violationsFilepath | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingPlainTextForPasswordNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingPlainTextForPassword" {
    Context "When there are violations" {
        It "has 3 avoid using plain text for password violations" {
            $violations.Count | Should Be 4
        }

	It "suggests corrections" {
	   Import-Module .\PSScriptAnalyzerTestHelper.psm1
	    Function Test-Extent($idx, $violationText, $correctionText)
	    {
		$violation = $violations[$idx]
	    	$violation.SuggestedCorrections.Count | Should Be 1
	    	Get-ExtentText $violation.SuggestedCorrections[0] $violationsFilepath | Should Be $violationText
	    	$violation.SuggestedCorrections[0].Text | Should Be $correctionText
	    }

	    Test-Extent 0 '$passphrases' '[SecureString] $passphrases'
	    Test-Extent 1 '$passwordparam' '[SecureString] $passwordparam'
	    Test-Extent 2 '$credential' '[SecureString] $credential'
	    Test-Extent 3 '$password' '[SecureString] $password'
	}

        It "has the correct violation message" {
            $violations[3].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}