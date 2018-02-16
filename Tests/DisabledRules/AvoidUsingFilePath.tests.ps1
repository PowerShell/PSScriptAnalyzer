Import-Module PSScriptAnalyzer
$violationMessage = @'
The file path "D:\\Code" of AvoidUsingFilePath.ps1 is rooted. This should be avoided if AvoidUsingFilePath.ps1 is published online
'@
$violationUNCMessage = @'
The file path "\\\\scratch2\\scratch\\" of AvoidUsingFilePath.ps1 is rooted. This should be avoided if AvoidUsingFilePath.ps1 is published online.
'@

$violationName = "PSAvoidUsingFilePath"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\AvoidUsingFilePath.ps1 | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\AvoidUsingFilePathNoViolations.ps1 | Where-Object {$_.RuleName -eq $violationName}

Describe "AvoidUsingFilePath" {
    Context "When there are violations" {
        It "has 4 avoid using file path violations" {
            $violations.Count | Should -Be 4
        }

        It "has the correct description message with drive name" {
            $violations[0].Message | Should -Match $violationMessage
        }

        It "has the correct description message (UNC)" {
            $violations[2].Message | Should -Match $violationUNCMessage
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}