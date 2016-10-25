
# Check if PSScriptAnalyzer is already loaded so we don't
# overwrite a test version of Invoke-ScriptAnalyzer by
# accident
if (!(Get-Module PSScriptAnalyzer) -and !$testingLibraryUsage)
{
	Import-Module PSScriptAnalyzer
}

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

if (-not (Test-PSEditionCoreCLR))
{
	# Force Get-Help not to prompt for interactive input to download help using Update-Help
	# By adding this registry key we turn off Get-Help interactivity logic during ScriptRule parsing
	$null,"Wow6432Node" | ForEach-Object {
		try
		{
			Set-ItemProperty -Name "DisablePromptToUpdateHelp" -Path "HKLM:\SOFTWARE\$($_)\Microsoft\PowerShell" -Value 1 -Force
		}
		catch
		{
			# Ignore for cases when tests are running in non-elevated more or registry key does not exist or not accessible
		}
	}
}


$message = "this is help"
$measure = "Measure-RequiresRunAsAdministrator"

Describe "Test importing customized rules with null return results" {
    Context "Test Get-ScriptAnalyzer with customized rules" {
        It "will not terminate the engine" {
            $customizedRulePath = Get-ScriptAnalyzerRule -CustomizedRulePath $directory\samplerule\SampleRulesWithErrors.psm1 | Where-Object {$_.RuleName -eq $measure}
            $customizedRulePath.Count | Should Be 1
        }

    }

    Context "Test Invoke-ScriptAnalyzer with customized rules" {
        It "will not terminate the engine" {
            $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\SampleRulesWithErrors.psm1 | Where-Object {$_.RuleName -eq $measure}
            $customizedRulePath.Count | Should Be 0
        }
    }

}

Describe "Test importing correct customized rules" {

	if(-not (Test-PSEditionCoreCLR))
	{
		Context "Test Get-Help functionality in ScriptRule parsing logic" {
			It "ScriptRule help section must be correctly processed when Get-Help is called for the first time" {

				# Force Get-Help to prompt for interactive input to download help using Update-Help
				# By removing this registry key we force to turn on Get-Help interactivity logic during ScriptRule parsing
				$null,"Wow6432Node" | ForEach-Object {
					try
					{
						Remove-ItemProperty -Name "DisablePromptToUpdateHelp" -Path "HKLM:\SOFTWARE\$($_)\Microsoft\PowerShell" -ErrorAction Stop
					} catch {
						#Ignore for cases when tests are running in non-elevated more or registry key does not exist or not accessible
					}
				}

				$customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\samplerule.psm1 | Where-Object {$_.Message -eq $message}
				$customizedRulePath.Count | Should Be 1

				# Force Get-Help not to prompt for interactive input to download help using Update-Help
				# By adding this registry key we turn off Get-Help interactivity logic during ScriptRule parsing
				$null,"Wow6432Node" | ForEach-Object {
					try
					{
						Set-ItemProperty -Name "DisablePromptToUpdateHelp" -Path "HKLM:\SOFTWARE\$($_)\Microsoft\PowerShell" -Value 1 -Force
					}
					catch
					{
						# Ignore for cases when tests are running in non-elevated more or registry key does not exist or not accessible
					}
				}
			}
		}
	}

    Context "Test Get-ScriptAnalyzer with customized rules" {
        It "will show the custom rule" {
            $customizedRulePath = Get-ScriptAnalyzerRule  -CustomizedRulePath $directory\samplerule\samplerule.psm1 | Where-Object {$_.RuleName -eq $measure}
            $customizedRulePath.Count | Should Be 1
        }

		It "will show the custom rule when given a rule folder path" {
			$customizedRulePath = Get-ScriptAnalyzerRule  -CustomizedRulePath $directory\samplerule | Where-Object {$_.RuleName -eq $measure}
		    $customizedRulePath.Count | Should Be 1
		}

        It "will show the custom rule when given a rule folder path with trailing backslash" {
			# needs fixing for linux
			if (-not (Test-PSEditionCoreCLRLinux))
			{
				$customizedRulePath = Get-ScriptAnalyzerRule  -CustomizedRulePath $directory/samplerule/ | Where-Object {$_.RuleName -eq $measure}
				$customizedRulePath.Count | Should Be 1
			}
		}

		It "will show the custom rules when given a glob" {
			# needs fixing for Linux
			$expectedNumRules = 4
			if (Test-PSEditionCoreCLRLinux)
			{
				$expectedNumRules = 3
			}
			$customizedRulePath = Get-ScriptAnalyzerRule  -CustomizedRulePath $directory\samplerule\samplerule* | Where-Object {$_.RuleName -match $measure}
			$customizedRulePath.Count | Should be $expectedNumRules
		}

		It "will show the custom rules when given recurse switch" {
			$customizedRulePath = Get-ScriptAnalyzerRule  -RecurseCustomRulePath -CustomizedRulePath "$directory\samplerule", "$directory\samplerule\samplerule2" | Where-Object {$_.RuleName -eq $measure}
			$customizedRulePath.Count | Should be 5
		}

		It "will show the custom rules when given glob with recurse switch" {
			# needs fixing for Linux
			$expectedNumRules = 5
			if (Test-PSEditionCoreCLRLinux)
			{
				$expectedNumRules = 4
			}
			$customizedRulePath = Get-ScriptAnalyzerRule  -RecurseCustomRulePath -CustomizedRulePath $directory\samplerule\samplerule* | Where-Object {$_.RuleName -eq $measure}
			$customizedRulePath.Count | Should be $expectedNumRules
		}

		It "will show the custom rules when given glob with recurse switch" {
			$customizedRulePath = Get-ScriptAnalyzerRule  -RecurseCustomRulePath -CustomizedRulePath $directory\samplerule* | Where-Object {$_.RuleName -eq $measure}
			$customizedRulePath.Count | Should be 3
		}
    }

    Context "Test Invoke-ScriptAnalyzer with customized rules" {
        It "will show the custom rule in the results" {
            $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\samplerule.psm1 | Where-Object {$_.Message -eq $message}
            $customizedRulePath.Count | Should Be 1
        }

		It "will show the custom rule in the results when given a rule folder path" {
            $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule | Where-Object {$_.Message -eq $message}
            $customizedRulePath.Count | Should Be 1
        }

		It "will set ScriptName property to the target file name" {
			$violations = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule
			$violations[0].ScriptName | Should Be 'TestScript.ps1'
		}

		It "will set ScriptPath property to the target file path" {
			$violations = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule
			$expectedScriptPath = Join-Path $directory 'TestScript.ps1'
			$violations[0].ScriptPath | Should Be $expectedScriptPath
		}

        if (!$testingLibraryUsage)
		{
            It "will show the custom rule in the results when given a rule folder path with trailing backslash" {
				# needs fixing for Linux
				if (-not (Test-PSEditionCoreCLRLinux))
				{
					$customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\ | Where-Object {$_.Message -eq $message}
					$customizedRulePath.Count | Should Be 1
				}
		    }

		    It "will show the custom rules when given a glob" {
			    $customizedRulePath = Invoke-ScriptAnalyzer  $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\samplerule* | Where-Object {$_.Message -eq $message}
			    $customizedRulePath.Count | Should be 3
		    }

		    It "will show the custom rules when given recurse switch" {
			    $customizedRulePath = Invoke-ScriptAnalyzer  $directory\TestScript.ps1 -RecurseCustomRulePath -CustomizedRulePath $directory\samplerule | Where-Object {$_.Message -eq $message}
			    $customizedRulePath.Count | Should be 3
		    }

		    It "will show the custom rules when given glob with recurse switch" {
			    $customizedRulePath = Invoke-ScriptAnalyzer  $directory\TestScript.ps1 -RecurseCustomRulePath -CustomizedRulePath $directory\samplerule\samplerule* | Where-Object {$_.Message -eq $message}
			    $customizedRulePath.Count | Should be 4
		    }

		    It "will show the custom rules when given glob with recurse switch" {
			    $customizedRulePath = Invoke-ScriptAnalyzer  $directory\TestScript.ps1 -RecurseCustomRulePath -CustomizedRulePath $directory\samplerule* | Where-Object {$_.Message -eq $message}
			    $customizedRulePath.Count | Should be 3
		    }

            It "Using IncludeDefaultRules Switch with CustomRulePath" {
                $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomRulePath $directory\samplerule\samplerule.psm1 -IncludeDefaultRules
                $customizedRulePath.Count | Should Be 2
            }

            It "Using IncludeDefaultRules Switch without CustomRulePath" {
                $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -IncludeDefaultRules
                $customizedRulePath.Count | Should Be 1
            }

            It "Not Using IncludeDefaultRules Switch and without CustomRulePath" {
                $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1
                $customizedRulePath.Count | Should Be 1
            }

			It "loads custom rules that contain version in their path" -Skip:($PSVersionTable.PSVersion -lt [Version]'5.0.0') {
			$customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomRulePath $directory\VersionedSampleRule\SampleRuleWithVersion
			$customizedRulePath.Count | Should Be 1

			$customizedRulePath = Get-ScriptAnalyzerRule -CustomRulePath $directory\VersionedSampleRule\SampleRuleWithVersion
			$customizedRulePath.Count | Should Be 1
			}

			It "loads custom rules that contain version in their path with the RecurseCustomRule switch" -Skip:($PSVersionTable.PSVersion -lt [Version]'5.0.0') {
			$customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomRulePath $directory\VersionedSampleRule -RecurseCustomRulePath
			$customizedRulePath.Count | Should Be 1

			$customizedRulePath = Get-ScriptAnalyzerRule -CustomRulePath $directory\VersionedSampleRule -RecurseCustomRulePath
			$customizedRulePath.Count | Should Be 1
		}
        }

    }
}

