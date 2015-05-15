Import-Module PSScriptAnalyzer
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
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
        }
       
    }

    Context "Test Get-ScriptAnalyzer with customized rules" {
        It "will show the customized rule" {
            $customizedRulePath = Get-ScriptAnalyzerRule  -CustomizedRulePath $directory\samplerule\samplerule.psm1 | Where-Object {$_.RuleName -eq $measure}
            $customizedRulePath.Count | Should Be 1
        }
       
    }

    Context "Test Invoke-ScriptAnalyzer with customized rules" {
        It "will show the customized rule in the results" {
            $customizedRulePath = Invoke-ScriptAnalyzer $directory\TestScript.ps1 -CustomizedRulePath $directory\samplerule\samplerule.psm1 | Where-Object {$_.Message -eq $message}
            $customizedRulePath.Count | Should Be 1
        }
    }

}