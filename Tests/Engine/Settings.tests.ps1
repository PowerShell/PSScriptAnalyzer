if (!(Get-Module PSScriptAnalyzer))
{
	Import-Module PSScriptAnalyzer
}

$directory = Split-Path $MyInvocation.MyCommand.Path
$settingsTestDirectory = [System.IO.Path]::Combine($directory, "SettingsTest")
$project1Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project1")
$project2Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project2")

Describe "Settings Precedence" {
    Context "settings object is explicit" {
        It "runs rules from the explicit setting file" {
              $settingsFilepath = [System.IO.Path]::Combine($project1Root, "ExplicitSettings.psd1")
              $violations = Invoke-ScriptAnalyzer -Path $project1Root -Settings $settingsFilepath -Recurse
              $violations.Count | Should Be 2
        }
	}

	Context "settings file is implicit" {
        It "runs rules from the implicit setting file" {
              $violations = Invoke-ScriptAnalyzer -Path $project1Root -Recurse
              $violations.Count | Should Be 1
              $violations[0].RuleName | Should Be "PSAvoidUsingCmdletAliases"
        }

        It "cannot find file if not named PSScriptAnalyzerSettings.psd1" {
              $violations = Invoke-ScriptAnalyzer -Path $project2Root -Recurse
              $violations.Count | Should Be 2
        }
	}
}

Describe "Settings Class" {
      Context "When an empty hashtable is provided" {
            BeforeAll {
                  $settings = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.Settings]::new(@{})
            }

            'IncludeRules', 'ExcludeRules', 'Severity', 'RuleArguments' | ForEach-Object {
                  It ("Should return empty {0} property" -f $_) {
                        $settings.${$_}.Count | Should Be 0
                  }
            }
      }

      Context "When a string is provided for IncludeRules in a hashtable" {
            BeforeAll {
                  $ruleName = "PSAvoidCmdletAliases"
                  $settings = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.Settings]::new(
                        @{
                              IncludeRules = $ruleName
                        }
                  )
            }

            It "Should return an IncludeRules array with 1 element" {
                  $settings.IncludeRules.Count | Should Be 1
            }

            It "Should return the rule in the IncludeRules array" {
                  $settings.IncludeRules[0] | Should Be $ruleName
            }
      }

      Context "When rule arguments are provided in a hashtable" {
            BeforeAll {
                  $settings = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.Settings]::new(
                        @{
                              Rules = @{
                                    PSAvoidUsingCmdletAliases = @{
                                          WhiteList = @("cd", "cp")
                                    }
                              }
                        }
                  )
            }

            It "Should return the rule arguments" {
                  $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"].Count | Should Be 2
                  $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][0] | Should Be "cd"
                  $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][1] | Should Be "cp"
            }

            It "Should be case insesitive" {
                  $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"].Count | Should Be 2
                  $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][0] | Should Be "cd"
                  $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][1] | Should Be "cp"
            }
      }

      Context "When a settings file path is provided" {
            BeforeAll {
                  $type = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.Settings]
                  $settings = $type::new([System.IO.Path]::Combine($project1Root, "ExplicitSettings.psd1"))
            }

            It "Should return 2 IncludeRules" {
                  $settings.IncludeRules.Count | Should Be 2
            }

            It "Should return 2 ExcludeRules" {
                  $settings.ExcludeRules.Count | Should Be 3
            }

            It "Should return 1 rule argument" {
                  $settings.RuleArguments.Count | Should Be 1
            }
      }
}