if (!(Get-Module PSScriptAnalyzer))
{
	Import-Module PSScriptAnalyzer
}

$directory = Split-Path $MyInvocation.MyCommand.Path
Describe "Settings Precedence" {
    $settingsTestDirectory = [System.IO.Path]::Combine($directory, "SettingsTest")
	$project1Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project1")
    $project2Root = [System.IO.Path]::Combine($settingsTestDirectory, "Project2")
    Context "settings object is explicit" {
        It "runs rules from the explicit setting file" {
              $settingsFilepath = [System.IO.Path]::Combine($project1Root, "ExplicitSettings.psd1")
              $violations = Invoke-ScriptAnalyzer -Path $project1Root -Settings $settingsFilepath -Recurse
              $violations.Count | Should Be 1
              $violations[0].RuleName | Should Be "PSAvoidUsingWriteHost"
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