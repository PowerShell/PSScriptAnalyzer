# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

using namespace System.Management.Automation.Language

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
param()

BeforeAll {
    $ruleName = "PSAvoidUsingArrayList"
    $ruleMessage = "The ArrayList class is used in '{0}'. Consider using a generic collection or a fixed array instead."
}

Describe "AvoidArrayList" {
    Context "When there are violations" {

        BeforeAll {
            $violationFileName = "$PSScriptRoot\AvoidUsingArrayList.ps1"
            $violations = Invoke-ScriptAnalyzer $violationFileName | Where-Object RuleName -eq $ruleName

            $violationExtents = @{}
                [Parser]::ParseFile($violationFileName, [ref] $null, [ref] $null).FindAll({
                    $Args[0] -is [AssignmentStatementAst] -and
                    $Args[0].Left.Extent.Text -eq '$List'
            }, $false).Right.Extent.foreach{ $violationExtents[$_.StartLineNumber] = $_ }
        }

        It "Should return 12 violations" {
            $violations.Count | Should -Be 12
        }

        It "Should return a correct violation record" -ForEach $violations {
            $expectedViolation = $violationExtents[$_.Line]
            $_.Extent.Text | Should -Be $expectedViolation.Text
            $_.Message     | Should -Be ($ruleMessage -f $expectedViolation.Text)
            $_.Severity    | Should -Be Warning
            $_.ScriptName  | Should -Be AvoidUsingArrayList.ps1
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
