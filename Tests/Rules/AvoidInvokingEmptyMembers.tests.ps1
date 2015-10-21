Import-Module PSScriptAnalyzer

$violationMessage = "() has non-constant members. Invoking non-constant members may cause bugs in the script."
$violationName = "PSAvoidInvokingEmptyMembers"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidInvokingEmptyMembers.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidInvokingEmptyMembersNonViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidInvokingEmptyMembers" {
    Context "When there are violations" {
        It "has one InvokeEmptyMember violations" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}