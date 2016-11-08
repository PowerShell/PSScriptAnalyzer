$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$violationMessage = "The variable 'declaredVar2' is assigned but never used."
$violationName = "PSUseDeclaredVarsMoreThanAssignments"
$violations = Invoke-ScriptAnalyzer $directory\UseDeclaredVarsMoreThanAssignments.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\UseDeclaredVarsMoreThanAssignmentsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "UseDeclaredVarsMoreThanAssignments" {
    Context "When there are violations" {
        It "has 2 use declared vars more than assignments violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should Match $violationMessage
        }

        It "flags the variable in the correct scope" {
            $target = @'
function MyFunc1() {
    $a = 1
    $b = 1
    $a + $b
}

function MyFunc2() {
    $a = 1
    $b = 1
    $a + $a
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $target -IncludeRule $violationName | `
            Get-Count | `
            Should Be 1
        }

        It "does not flag `$InformationPreference variable" {
            Invoke-ScriptAnalyzer -ScriptDefinition '$InformationPreference=Stop' -IncludeRule $violationName  | `
            Get-Count | `
            Should Be 0
        }

        It "does not flag `$PSModuleAutoLoadingPreference variable" {
            Invoke-ScriptAnalyzer -ScriptDefinition '$PSModuleAutoLoadingPreference=None' -IncludeRule $violationName | `
            Get-Count | `
            Should Be 0
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}