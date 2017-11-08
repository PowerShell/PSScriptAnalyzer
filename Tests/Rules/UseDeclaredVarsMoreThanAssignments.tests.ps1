$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$violationMessage = "The variable 'declaredVar2' is assigned but never used."
$violationName = "PSUseDeclaredVarsMoreThanAssignments"
$violations = Invoke-ScriptAnalyzer $directory\UseDeclaredVarsMoreThanAssignments.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\UseDeclaredVarsMoreThanAssignmentsNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Function Test-UseDeclaredVarsMoreThanAssignments
{
    param(
        [string] $targetScript,
        [int] $expectedNumViolations
    )
            Invoke-ScriptAnalyzer -ScriptDefinition $targetScript -IncludeRule $violationName | `
            Get-Count | `
            Should Be $expectedNumViolations
}

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
            Test-UseDeclaredVarsMoreThanAssignments $target 1
        }

        It "flags variables assigned in downstream scopes" {
$target = @'
function Get-Directory() {
    $a = 1
    1..10 | ForEach-Object { $a = $_ }
}
'@
            Test-UseDeclaredVarsMoreThanAssignments $target 2
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }

        It "does not flag `$InformationPreference variable" {
            Test-UseDeclaredVarsMoreThanAssignments '$InformationPreference=Stop' 0
        }

        It "does not flag `$PSModuleAutoLoadingPreference variable" {
            Test-UseDeclaredVarsMoreThanAssignments '$PSModuleAutoLoadingPreference=None' 0
        }

        It "does not flags variables used in downstream scopes" {
$target = @'
function Get-Directory() {
    $a = 1
    1..10 | ForEach-Object { Write-Output $a }
}
'@
            Test-UseDeclaredVarsMoreThanAssignments $target 0
        }

        It "does not flag variables assigned in downstream scope but used in parent scope" {
$target = @'
function Get-Directory() {
    $a = 1
    1..10 | ForEach-Object { $a = $_ }
    $a
}
'@
            Test-UseDeclaredVarsMoreThanAssignments $target 0
        }
    }
}