if (!(Get-Module PSScriptAnalyzer)) {
    Import-Module PSScriptAnalyzer
}

$directory = Split-Path $MyInvocation.MyCommand.Path
$settingsTestDirectory = [System.IO.Path]::Combine($directory, "SettingsTest")
$project1Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project1")
$project2Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project2")
$settingsTypeName = 'Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings'

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
            $settings = New-Object -TypeName $settingsTypeName -ArgumentList @{}
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
            $settings = New-Object -TypeName $settingsTypeName -ArgumentList @{ IncludeRules = $ruleName }
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
            $settingsHashtable = @{
                Rules = @{
                    PSAvoidUsingCmdletAliases = @{
                        WhiteList = @("cd", "cp")
                    }
                }
            }
            $settings = New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable
        }

        It "Should return the rule arguments" {
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"].Count | Should Be 2
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][0] | Should Be "cd"
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][1] | Should Be "cp"
        }

        It "Should be case insensitive" {
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"].Count | Should Be 2
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][0] | Should Be "cd"
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][1] | Should Be "cp"
        }
    }

    Context "When a settings file path is provided" {
        BeforeAll {
            $settings = New-Object -TypeName $settingsTypeName `
                              -ArgumentList ([System.IO.Path]::Combine($project1Root, "ExplicitSettings.psd1"))
        }

        It "Should return 2 IncludeRules" {
            $settings.IncludeRules.Count | Should Be 3
        }

        It "Should return 2 ExcludeRules" {
            $settings.ExcludeRules.Count | Should Be 3
        }

        It "Should return 1 rule argument" {
            $settings.RuleArguments.Count | Should Be 3
        }

        It "Should parse boolean type argument" {
            $settings.RuleArguments["PSUseConsistentIndentation"]["Enable"] | Should Be $true
        }

        It "Should parse int type argument" {
            $settings.RuleArguments["PSUseConsistentIndentation"]["IndentationSize"] | Should Be 4
        }

        It "Should parse string literal" {
            $settings.RuleArguments["PSProvideCommentHelp"]["Placement"] | Should Be 'end'
        }
    }

    Context "When CustomRulePath parameter is provided" {
        It "Should return an array of 1 item when only 1 path is given in a hashtable" {
            $rulePath = "C:\rules\module1"
            $settingsHashtable = @{
                CustomRulePath = $rulePath
            }

            $settings = New-Object -TypeName $settingsTypeName  -ArgumentList $settingsHashtable
            $settings.CustomRulePath.Count | Should Be 1
            $settings.CustomRulePath[0] | Should be $rulePath
        }

        It "Should return an array of n items when n items are given in a hashtable" {
            $rulePaths = @("C:\rules\module1", "C:\rules\module2")
            $settingsHashtable = @{
                CustomRulePath = $rulePaths
            }

            $settings = New-Object -TypeName $settingsTypeName  -ArgumentList $settingsHashtable
            $settings.CustomRulePath.Count | Should Be $rulePaths.Count
            0..($rulePaths.Count - 1) | ForEach-Object { $settings.CustomRulePath[$_] | Should be $rulePaths[$_] }
        }
    }

    Context "When IncludeDefaultRules parameter is provided" {
        It "Should correctly set the value if a boolean is given - true" {
            $settingsHashtable = @{
                IncludeDefaultRules = $true
            }

            $settings = New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable
            $settings.IncludeDefaultRules | Should Be $true
        }

        It "Should correctly set the value if a boolean is given - false" {
            $settingsHashtable = @{
                IncludeDefaultRules = $false
            }

            $settings = New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable
            $settings.IncludeDefaultRules | Should Be $false
        }

        It "Should throw if a non-boolean value is given" {
            $settingsHashtable = @{
                IncludeDefaultRules = "some random string"
            }

            { New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable } | Should Throw
        }
    }
}
