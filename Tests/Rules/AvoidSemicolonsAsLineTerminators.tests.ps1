# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidSemicolonsAsLineTerminators"

    $ruleSettings = @{
        Enable = $true
    }
    $settings = @{
        IncludeRules = @($ruleName)
        Rules        = @{ $ruleName = $ruleSettings }
    }
}

Describe "AvoidSemicolonsAsLineTerminators" {
    Context "When the rule is not enabled explicitly" {
        It "Should not find violations" {
            $def = "'test';"
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def
            $violations.Count | Should -Be 0
        }
    }

    Context "Given a line ending with a semicolon" {
        It "Should find one violation" {
            $def = "'test';"
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context "Given a line with a semicolon in the middle and one at the end" {
        It "Should find one violation" {
            $def = "'test';'test';"
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations[0].Extent.StartLineNumber | Should -Be 1
            $violations[0].Extent.EndLineNumber | Should -Be 1
            $violations[0].Extent.StartColumnNumber | Should -Be 14
            $violations[0].Extent.EndColumnNumber | Should -Be 15
        }
    }


    Context "Given a multiline string with a line ending with a semicolon" {
        It "Should get the correct extent of the violation for a single semicolon" {
            $def = @"
'this line has no semicolon'
'test';
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations[0].Extent.StartLineNumber | Should -Be 2
            $violations[0].Extent.EndLineNumber | Should -Be 2
            $violations[0].Extent.StartColumnNumber | Should -Be 7
            $violations[0].Extent.EndColumnNumber | Should -Be 8
        }
    }

    Context "Given a multiline string with a line having a semicolon in the middle" {
        It "Should not find any violations" {
            $def = @"
'this line has no semicolon'
'line with a semicolon; in the middle'
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "Given a multiline string with a line having a semicolon in C# code" {
        It "Should not find any violations" {
            $def = @"
`$calcCode = `@"
public class Calc{
    public int Add(int x,int y){
        return x+y;
    }
}
"`@

Add-Type -TypeDefinition `$calcCode -Language CSharp

`$c = New-Object Calc
`$c.Add(1,2)
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "Given a multiline string with a line having a semicolon in variable assignment" {
        It "Should not find any violations" {
            $def = @"
`$allowPopupsOption=`@"
lockPref("dom.disable_open_during_load", false);
"`@
    Write `$allowPopupsOption | Out-File -Encoding UTF8 -FilePath `$pathToMozillaCfg -Append
"@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "Given a line ending with a semicolon" {
        It "Should remove the semicolon at the end" {
            $def = "'test';"
            $expected = "'test'"

            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }

    Context "Given a line with a semicolon in the middle and one at the end" {
        It "Should remove the semicolon at the end" {
            $def = "'test';'test';"
            $expected = "'test';'test'"

            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }

    Context "Given a multiline string with a line ending with a semicolon" {
        It "Should remove the semicolon at the end of the line with a semicolon" {
            $def = @"
'this line has no semicolon at the end'
'test';
"@
            $expected = @"
'this line has no semicolon at the end'
'test'
"@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
}
