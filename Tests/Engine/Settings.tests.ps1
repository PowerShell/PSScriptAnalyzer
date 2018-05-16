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
            $violations.Count | Should -Be 2
        }
    }

    Context "settings file is implicit" {
        It "runs rules from the implicit setting file" {
            $violations = Invoke-ScriptAnalyzer -Path $project1Root -Recurse
            $violations.Count | Should -Be 1
            $violations[0].RuleName | Should -Be "PSAvoidUsingCmdletAliases"
        }

        It "cannot find file if not named PSScriptAnalyzerSettings.psd1" {
            $violations = Invoke-ScriptAnalyzer -Path $project2Root -Recurse
            $violations.Count | Should -Be 2
        }
    }
}

Describe "Settings Class" {
    Context "When an empty hashtable is provided" {
        BeforeAll {
            $settings = New-Object -TypeName $settingsTypeName -ArgumentList @{}
        }

        It "Should return empty <name> property" -TestCases @(
            @{ Name = "IncludeRules" }
            @{ Name = "ExcludeRules" }
            @{ Name = "Severity" }
            @{ Name = "RuleArguments" }
        ) {
            Param($Name)

            ${settings}.${Name}.Count | Should -Be 0
        }
    }

    Context "When a string is provided for IncludeRules in a hashtable" {
        BeforeAll {
            $ruleName = "PSAvoidCmdletAliases"
            $settings = New-Object -TypeName $settingsTypeName -ArgumentList @{ IncludeRules = $ruleName }
        }

        It "Should return an IncludeRules array with 1 element" {
            $settings.IncludeRules.Count | Should -Be 1
        }

        It "Should return the rule in the IncludeRules array" {
            $settings.IncludeRules[0] | Should -Be $ruleName
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
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"].Count | Should -Be 2
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][0] | Should -Be "cd"
            $settings.RuleArguments["PSAvoidUsingCmdletAliases"]["WhiteList"][1] | Should -Be "cp"
        }

        It "Should Be case insensitive" {
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"].Count | Should -Be 2
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][0] | Should -Be "cd"
            $settings.RuleArguments["psAvoidUsingCmdletAliases"]["whiteList"][1] | Should -Be "cp"
        }
    }

    Context "When a settings file path is provided" {
        BeforeAll {
            $settings = New-Object -TypeName $settingsTypeName `
                              -ArgumentList ([System.IO.Path]::Combine($project1Root, "ExplicitSettings.psd1"))
        }

        $expectedNumberOfIncludeRules = 3
        It "Should return $expectedNumberOfIncludeRules IncludeRules" {
            $settings.IncludeRules.Count | Should -Be $expectedNumberOfIncludeRules
        }
        
        $expectedNumberOfExcludeRules = 3
        It "Should return $expectedNumberOfExcludeRules ExcludeRules" {
            $settings.ExcludeRules.Count | Should -Be $expectedNumberOfExcludeRules
        }

        $expectedNumberOfRuleArguments = 3
        It "Should return $expectedNumberOfRuleArguments rule argument" {
            $settings.RuleArguments.Count | Should -Be 3
        }

        It "Should parse boolean type argument" {
            $settings.RuleArguments["PSUseConsistentIndentation"]["Enable"] | Should -BeTrue
        }

        It "Should parse int type argument" {
            $settings.RuleArguments["PSUseConsistentIndentation"]["IndentationSize"] | Should -Be 4
        }

        It "Should parse string literal" {
            $settings.RuleArguments["PSProvideCommentHelp"]["Placement"] | Should -Be 'end'
        }
    }

    Context "When CustomRulePath parameter is provided" {
        It "Should return an array of 1 item when only 1 path is given in a hashtable" {
            $rulePath = "C:\rules\module1"
            $settingsHashtable = @{
                CustomRulePath = $rulePath
            }

            $settings = New-Object -TypeName $settingsTypeName  -ArgumentList $settingsHashtable
            $settings.CustomRulePath.Count | Should -Be 1
            $settings.CustomRulePath[0] | Should -Be $rulePath
        }

        It "Should return an array of n items when n items are given in a hashtable" {
            $rulePaths = @("C:\rules\module1", "C:\rules\module2")
            $settingsHashtable = @{
                CustomRulePath = $rulePaths
            }

            $settings = New-Object -TypeName $settingsTypeName  -ArgumentList $settingsHashtable
            $settings.CustomRulePath.Count | Should -Be $rulePaths.Count
            0..($rulePaths.Count - 1) | ForEach-Object { $settings.CustomRulePath[$_] | Should -Be $rulePaths[$_] }

        }

        It "Should detect the parameter in a settings file" {
            $settings = New-Object -TypeName $settingsTypeName `
                              -ArgumentList ([System.IO.Path]::Combine($project1Root, "CustomRulePathSettings.psd1"))
            $settings.CustomRulePath.Count | Should -Be 2
        }
    }

    @("IncludeDefaultRules", "RecurseCustomRulePath") | ForEach-Object {
        $paramName = $_
        Context "When $paramName parameter is provided" {
            It "Should correctly set the value if a boolean is given - true" {
                $settingsHashtable = @{}
                $settingsHashtable.Add($paramName, $true)

                $settings = New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable
                $settings."$paramName" | Should -BeTrue
            }

            It "Should correctly set the value if a boolean is given - false" {
                $settingsHashtable = @{}
                $settingsHashtable.Add($paramName, $false)

                $settings = New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable
                $settings."$paramName" | Should -BeFalse
            }

            It "Should throw if a non-boolean value is given" {
                $settingsHashtable = @{}
                $settingsHashtable.Add($paramName, "some random string")

                { New-Object -TypeName $settingsTypeName -ArgumentList $settingsHashtable } | Should -Throw
            }

            It "Should detect the parameter in a settings file" {
                $settings = New-Object -TypeName $settingsTypeName `
                    -ArgumentList ([System.IO.Path]::Combine($project1Root, "CustomRulePathSettings.psd1"))
                $settings."$paramName" | Should -BeTrue
            }
        }
    }
}
