$testRootDirectory = Split-Path -Parent $PSScriptRoot
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$AvoidGlobalAliasesError = "Avoid creating aliases with a Global scope."
$violationName = "PSAvoidGlobalAliases"
$violations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalAliases.psm1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\AvoidGlobalAliasesNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}
$IsV3OrV4 = (Test-PSVersionV3) -or (Test-PSVersionV4)

Describe "$violationName " {
    Context "When there are violations" {
        It "Has 4 avoid global alias violations" -Skip:$IsV3OrV4 {
            $violations.Count | Should -Be 4
        }

        It "Has the correct description message" -Skip:$IsV3OrV4 {
            $violations[0].Message | Should -Match $AvoidGlobalAliasesError
        }
    }

    Context "When there are no violations" {
        It "Returns no violations" -Skip:$IsV3OrV4 {
            $noViolations.Count | Should -Be 0
        }
    }
}
