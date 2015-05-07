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