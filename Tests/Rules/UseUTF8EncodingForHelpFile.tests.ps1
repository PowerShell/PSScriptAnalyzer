Import-Module PSScriptAnalyzer
$violationMessage = "File about_utf16.help.txt has to use UTF8 instead of System.Text.UTF32Encoding encoding because it is a powershell help file."
$violationName = "PSUseUTF8EncodingForHelpFile"
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer $directory\about_utf16.help.txt | Where-Object {$_.RuleName -eq $violationName}
$noViolations = Invoke-ScriptAnalyzer $directory\about_utf8.help.txt | Where-Object {$_.RuleName -eq $violationName}
$notHelpFileViolations = Invoke-ScriptAnalyzer $directory\utf16.txt | Where-Object {$_.RuleName -eq $violationName}

Describe "UseUTF8EncodingForHelpFile" {
    Context "When there are violations" {
        It "has 1 avoid use utf8 encoding violation" {
            $violations.Count | Should Be 1
        }

        It "has the correct description message" {
            $violations[0].Message | Should Match $violationMessage
        }

    }

    Context "When there are no violations" {
        It "returns no violations for correct utf8 help file" {
            $noViolations.Count | Should Be 0
        }

        It "returns no violations for utf16 file that is not a help file" {
            $notHelpFileViolations.Count | Should Be 0
        }
    }
}