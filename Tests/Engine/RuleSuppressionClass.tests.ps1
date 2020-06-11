# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $script:skipForV3V4 = $true
    if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
        $script:skipForV3V4 = $false
    }

    $violationsUsingScriptDefinition = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$PSScriptRoot\RuleSuppression.ps1")
    $violations = Invoke-ScriptAnalyzer "$PSScriptRoot\RuleSuppression.ps1"
}

Describe "RuleSuppressionWithoutScope" {

    Context "Class" {
        It "Does not raise violations" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingInvokeExpression" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingInvokeExpression" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "FunctionInClass" {
        It "Does not raise violations" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingCmdletAliases" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingCmdletAliases" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "Script" {
        It "Does not raise violations" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "RuleSuppressionID" {
        It "Only suppress violations for that ID" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should -Be 1
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should -Be 1
        }
    }
}

Describe "RuleSuppressionWithScope" {
    Context "FunctionScope" {
        It "Does not raise violations" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "ClassScope" {
        It "Does not raise violations" -skip:$script:skipForV3V4 {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingConvertToSecureStringWithPlainText" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingConvertToSecureStringWithPlainText" }
            $suppression.Count | Should -Be 0
        }
    }
}
