$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')
Import-Module PSScriptAnalyzer

$AvoidGlobalAliasesError = "Avoid creating aliases with a Global scope."
$violationName = "PSAvoidGlobalAliases"
$violations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliases.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidGlobalAliasesNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
$IsV3OrV4 = (Test-PSVersionV3) -or (Test-PSVersionV4)

Describe "$violationName " {
    Context "When there are violations" {
        It "Has 4 avoid global alias violations" -Skip:$IsV3OrV4 {
            $violations.Count | Should Be 4
        }

        It "Has the correct description message" -Skip:$IsV3OrV4 {
            $violations[0].Message | Should Match $AvoidGlobalAliasesError
        }
    }

    Context "When there are no violations" {
        It "Returns no violations" -Skip:$IsV3OrV4 {
            $noViolations.Count | Should Be 0
        }
    }
}
