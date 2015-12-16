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
        It "Raises 1 violation for cmdlet alias" {
            $withoutProfile = $violations | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases"}
            $withoutProfile.Count | Should Be 1
            $withoutProfile = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases"}
            $withoutProfile.Count | Should Be 1
        }

        It "Does not raise any violations for cmdlet alias with profile" {
            $withProfile = $suppression | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" }
            $withProfile.Count | Should be 0
            $withProfile = $suppressionUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases" }
            $withProfile.Count | Should be 0
        }

        It "Does not raise any violation for cmdlet alias using configuration hashtable" {
            $hashtableConfiguration = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Configuration @{"excluderules" = "PSAvoidUsingCmdletAliases"} |
                                        Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases"}
            $hashtableConfiguration.Count | Should Be 0

            $hashtableConfiguration = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$directory\GlobalSuppression.ps1") -Configuration @{"excluderules" = "PSAvoidUsingCmdletAliases"} |
                                        Where-Object { $_.RuleName -eq "PSAvoidUsingCmdletAliases"}
            $hashtableConfiguration.Count | Should Be 0
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
        
        It "Does not raise any violation for computername hard-coded using configuration hashtable" {
            $hashtableConfiguration = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Settings @{"includerules" = @("PSAvoidUsingCmdletAliases", "PSUseOutputTypeCorrectly")} |
                                        Where-Object { $_.RuleName -eq "PSAvoidUsingComputerNameHardcoded"}
            $hashtableConfiguration.Count | Should Be 0
        }
    }

    Context "Severity" {
        It "Raises 1 violation for use output type correctly without profile" {
            $withoutProfile = $violations | Where-Object { $_.RuleName -eq "PSUseOutputTypeCorrectly" }
            $withoutProfile.Count | Should Be 1
            $withoutProfile = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSUseOutputTypeCorrectly" }
            $withoutProfile.Count | Should Be 1
        }

        It "Does not raise any violations for use output type correctly with profile" {
            $withProfile = $suppression | Where-Object { $_.RuleName -eq "PSUseOutputTypeCorrectly" }
            $withProfile.Count | Should be 0
            $withProfile = $suppressionUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSUseOutputTypeCorrectly" }
            $withProfile.Count | Should be 0
        }

        It "Does not raise any violation for use output type correctly with configuration hashtable" {
            $hashtableConfiguration = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Settings @{"severity" = "warning"} |
                                    Where-Object {$_.RuleName -eq "PSUseOutputTypeCorrectly"}
            $hashtableConfiguration.Count | should be 0
        }
    }

    Context "Error Case" {
        It "Raises Error for file not found" {
            $invokeWithError = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Settings ".\ThisFileDoesNotExist.ps1" -ErrorAction SilentlyContinue
            $invokeWithError.Count | should be 0
            $Error[0].FullyQualifiedErrorId | should match "SettingsFileNotFound,Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands.InvokeScriptAnalyzerCommand"
        }

        It "Raises Error for file with no hash table" {
            $invokeWithError = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Settings "$directory\GlobalSuppression.ps1" -ErrorAction SilentlyContinue
            $invokeWithError.Count | should be 0
            $Error[0].FullyQualifiedErrorId | should match "SettingsFileHasNoHashTable,Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands.InvokeScriptAnalyzerCommand"
        }

        It "Raises Error for wrong profile" {
            $invokeWithError = Invoke-ScriptAnalyzer "$directory\GlobalSuppression.ps1" -Settings "$directory\WrongProfile.ps1" -ErrorAction SilentlyContinue
            $invokeWithError.Count | should be 0
            $Error[0].FullyQualifiedErrorId | should match "WrongSettingsKey,Microsoft.Windows.PowerShell.ScriptAnalyzer.Commands.InvokeScriptAnalyzerCommand"
        }
    }
}