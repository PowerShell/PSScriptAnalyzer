# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidMultipleTypesParameter"

    $settings = @{
        IncludeRules = @($ruleName)
    }
}

Describe 'AvoidMultipleTypesParameter' {
    it 'Should find 3 violations for paramters have more than one type spceifiers' {
        $def = @'
function F1 ($s1, $p1){}
function F2 ([int] $s2, [int] $p2){}
function F3 ([int][switch] $s3, [int] $p3){}
function F4 ([int][ref] $s4, [int] $p4){}
function F5 ([int][switch][boolean] $s5, [int] $p5){}
'@
        Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 3
    }

    it 'Should get the correct extent of the violation ' {
        $def = @'
function F1 ($s1, $p1){}
function F2 ([int] $s2, [int] $p2){}
function F3 ([int][switch] $s3, [int] $p3){}
function F4 ([int][ref] $s4, [int] $p4){}
function F5 ([int][switch][boolean] $s5, [int] $p5){}
'@
        Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations[0].Extent.StartLineNumber | Should -Be 3
        $violations[0].Extent.EndLineNumber | Should -Be 3
        $violations[1].Extent.StartLineNumber | Should -Be 4
        $violations[1].Extent.EndLineNumber | Should -Be 4
        $violations[2].Extent.StartLineNumber | Should -Be 5
        $violations[2].Extent.EndLineNumber | Should -Be 5
    }

    it 'Should get the correct error messaage of the violation ' {
        $def = @'
function F1 ($s1, $p1){}
function F2 ([int] $s2, [int] $p2){}
function F3 ([int][switch] $s3, [int] $p3){}
function F4 ([int][ref] $s4, [int] $p4){}
function F5 ([int][switch][boolean] $s5, [int] $p5){}
'@
        Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations[0].Message | Should -Match 'Parameter ''\$s3'' has more than one type specifier.'
        $violations[1].Message | Should -Match 'Parameter ''\$s4'' has more than one type specifier.'
        $violations[2].Message | Should -Match 'Parameter ''\$s5'' has more than one type specifier.'
    }

    it 'Should not have violations for paramters have one or less type spceifier' {
        $def = @'
function F1 ($s1, $p1){}
function F2 ([int] $s2, [int] $p2){}
function F3 ([ValidateSet()][int] $s3, [int] $p3){}
function F4 ([Parameter(Mandatory=$true)][ValidateSet()][int] $s4, [int] $p4){}
'@
        Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        $violations.Count | Should -Be 0
    }
}
