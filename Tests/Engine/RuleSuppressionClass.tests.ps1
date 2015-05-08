if ($PSVersionTable.PSVersion -ge [Version]'5.0') {
    Import-Module -Verbose PSScriptAnalyzer
    $directory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $violations = Invoke-ScriptAnalyzer $directory\RuleSuppressionClass.ps1
    
    Describe "RuleSuppressionWithoutScope" {
        Context "Class" {
            It "Does not raise violations" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingInvokeExpression" }
                $suppression.Count | Should Be 0
            }
        }
    
        Context "FunctionInClass" {
            It "Does not raise violations" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingCmdletAliases" }
                $suppression.Count | Should Be 0
            }
        }
    
        Context "Script" {
            It "Does not raise violations" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
                $suppression.Count | Should Be 0
            }
        }
    
        Context "RuleSuppressionID" {
            It "Only suppress violations for that ID" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUninitializedVariable" }
                $suppression.Count | Should Be 1
            }
        }
    }
    
    Describe "RuleSuppressionWithScope" {
        Context "FunctionScope" {
            It "Does not raise violations" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
                $suppression.Count | Should Be 1
            }
        }
    
        Context "ClassScope" {
            It "Does not raise violations" {
                $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingConvertToSecureStringWithPlainText" }
                $suppression.Count | Should Be 0
            }
        }
    }
}