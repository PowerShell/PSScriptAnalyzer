# Check if PSScriptAnalyzer is already loaded so we don't
# overwrite a test version of Invoke-ScriptAnalyzer by
# accident
if (!(Get-Module PSScriptAnalyzer) -and !$testingLibraryUsage)
{
	Import-Module -Verbose PSScriptAnalyzer
}

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$violations = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1"
$violationsUsingScriptDefinition = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$directory\GlobalSuppression.ps1")
$suppression = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Profile "$directory\Profile.ps1"
$suppressionUsingScriptDefinition = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$directory\GlobalSuppression.ps1") -Profile "$directory\Profile.ps1"

Describe "GlobalSuppression" {
    Context "Exclude Rule" {
        It "Raises 1 violation for uninitialized variable and 1 for cmdlet alias" {
            $withoutProfile = $violations | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" -or $_.RuleName -eq "PSAvoidUninitializedVariable" }
            $withoutProfile.Count | Should Be 1
            $withoutProfile = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" -or $_.RuleName -eq "PSAvoidUninitializedVariable" }
            $withoutProfile.Count | Should Be 1
        }

        It "Does not raise any violations for uninitialized variable and cmdlet alias with profile" {
            $withProfile = $suppression | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" -or $_.RuleName -eq "PSAvoidUninitializedVariable" }
            $withProfile.Count | Should be 0
            $withProfile = $suppressionUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" -or $_.RuleName -eq "PSAvoidUninitializedVariable" }
            $withProfile.Count | Should be 0
        }
    }

    Context "Include Rule" {
        It "Raises 1 violation for computername hard-coded" {
            $withoutProfile = $violations | Where-Object { $_.RuleName -eq "PSAvoidUsingComputerNameHardcoded" }
            $withoutProfile.Count | Should Be 1
            $withoutProfile = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingComputerNameHardcoded" }
            $withoutProfile.Count | Should Be 1
        }

        It "Does not raise any violations for computername hard-coded" {
            $withProfile = $suppression | Where-Object { $_.RuleName -eq "PSAvoidUsingComputerNameHardcoded" }
            $withProfile.Count | Should be 0
            $withProfile = $suppressionUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingComputerNameHardcoded" }
            $withProfile.Count | Should be 0
        }
    }

    Context "Severity" {
        It "Raises 1 violation for internal url without profile" {
            $withoutProfile = $violations | Where-Object { $_.RuleName -eq "PSAvoidUsingInternalURLs" }
            $withoutProfile.Count | Should Be 1
            $withoutProfile = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingInternalURLs" }
            $withoutProfile.Count | Should Be 1
        }

        It "Does not raise any violations for internal urls with profile" {
            $withProfile = $suppression | Where-Object { $_.RuleName -eq "PSAvoidUsingInternalURLs" }
            $withProfile.Count | Should be 0
            $withProfile = $suppressionUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingInternalURLs" }
            $withProfile.Count | Should be 0
        }
    }
}