Import-Module PSScriptAnalyzer
$AvoidUninitializedVariable = "PSAvoidUninitializedVariable"
$violationMessage = "Variable 'MyVerbosePreference' is not initialized. Non-global variables must be initialized. To fix a violation of this rule, please initialize non-global variables."
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingUninitializedVariable.ps1 -IncludeRule $AvoidUninitializedVariable
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingUninitializedVariableNoViolations.ps1 -IncludeRule $AvoidUninitializedVariable

Describe "AvoidUsingUninitializedVariable" {
    Context "Script uses uninitialized variables - Violation" {
        It "Have 3 rule violations" {
            $violations.Count | Should -Be 3
        }

        It "has the correct description message for UninitializedVariable rule violation" {
            $violations[0].Message | Should -Be $violationMessage            
        }
    }

    Context "Script uses initialized variables - No violation" {
        It "results in no rule violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}