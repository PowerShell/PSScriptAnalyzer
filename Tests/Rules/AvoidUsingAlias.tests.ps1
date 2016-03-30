Import-Module PSScriptAnalyzer
$violationMessage = "'cls' is an alias of 'Clear-Host'. Alias can introduce possible problems and make scripts hard to maintain. Please consider changing alias to its full content."
$violationName = "PSAvoidUsingCmdletAliases"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violationsFilepath = Join-Path $directory 'AvoidUsingAlias.ps1'
$violations = Invoke-ScriptAnalyzer $violationsFilepath | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingAliasNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingAlias" {
    Context "When there are violations" {
        It "has 2 Avoid Using Alias Cmdlets violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should Match $violationMessage
        }

	It "suggests correction" {
	   Import-Module .\PSScriptAnalyzerTestHelper.psm1
	   $violations[0].SuggestedCorrections.Count | Should Be 1
	   $violations[0].SuggestedCorrections.Text | Should Be 'Invoke-Expression'
	   Get-ExtentText $violations[0].SuggestedCorrections[0] $violationsFilepath | Should Be 'iex'

	   $violations[1].SuggestedCorrections.Count | Should Be 1
	   $violations[1].SuggestedCorrections.Text | Should Be 'Clear-Host'
	   Get-ExtentText $violations[1].SuggestedCorrections[0] $violationsFilepath | Should Be 'cls'
	}
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}