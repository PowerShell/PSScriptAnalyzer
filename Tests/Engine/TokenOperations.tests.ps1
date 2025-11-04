# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "TokenOperations" {
    It "Should return correct AST position for assignment operator in hash table" {
        $scriptText = @'
$h = @{
    a = 72
    b = @{ z = "hi" }
}
'@
        $tokens = $null
        $parseErrors = $null
        $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
        $tokenOperations = New-Object Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations -ArgumentList @($tokens, $scriptAst)
        $operatorToken = $tokens | Where-Object { $_.Extent.StartLineNumber -eq 3 -and $_.Extent.StartColumnNumber -eq 14}
        $hashTableAst = $tokenOperations.GetAstPosition($operatorToken)
        $hashTableAst | Should -BeOfType [System.Management.Automation.Language.HashTableAst]
        $hashTableAst.Extent.Text | Should -Be '@{ z = "hi" }'
    }

    Context 'Braced Member Access Ranges' {

        BeforeDiscovery {
            $RangeTests = @(
                @{
                    Name = 'No braced member access'
                    ScriptDef = '$object.Prop'
                    ExpectedRanges = @()
                }
                @{
                    Name = 'No braced member access on braced variable name'
                    ScriptDef = '${object}.Prop'
                    ExpectedRanges = @()
                }
                @{
                    Name = 'Braced member access'
                    ScriptDef = '$object.{Prop}'
                    ExpectedRanges = @(
                        ,@(8, 14)
                    )
                }
                @{
                    Name = 'Braced member access with spaces'
                    ScriptDef = '$object. { Prop }'
                    ExpectedRanges = @(
                        ,@(9, 17)
                    )
                }
                @{
                    Name = 'Braced member access with newline'
                    ScriptDef = "`$object.`n{ Prop }"
                    ExpectedRanges = @(
                        ,@(9, 17)
                    )
                }
                @{
                    Name = 'Braced member access with comment'
                    ScriptDef = "`$object. <#comment#>{Prop}"
                    ExpectedRanges = @(
                        ,@(20, 26)
                    )
                }
                @{
                    Name = 'Braced member access with multi-line comment'
                    ScriptDef = "`$object. <#`ncomment`n#>{Prop}"
                    ExpectedRanges = @(
                        ,@(22, 28)
                    )
                }
                @{
                    Name = 'Braced member access with inline comment'
                    ScriptDef = "`$object. #comment`n{Prop}"
                    ExpectedRanges = @(
                        ,@(18, 24)
                    )
                }
                @{
                    Name = 'Braced member access with inner curly braces'
                    ScriptDef = "`$object.{{Prop}}"
                    ExpectedRanges = @(
                        ,@(8, 16)
                    )
                }
                @{
                    Name = 'Indexed Braced member access'
                    ScriptDef = "`$object[0].{Prop}"
                    ExpectedRanges = @(
                        ,@(11, 17)
                    )
                }
                @{
                    Name = 'Parenthesized Braced member access'
                    ScriptDef = "(`$object).{Prop}"
                    ExpectedRanges = @(
                        ,@(10, 16)
                    )
                }
                @{
                    Name = 'Chained Braced member access'
                    ScriptDef = "`$object.{Prop}.{InnerProp}"
                    ExpectedRanges = @(
                        ,@(8, 14)
                        ,@(15, 26)
                    )
                }
                @{
                    Name = 'Multiple Braced member access in larger script'
                    ScriptDef = @'
$var = 1
$a.prop.{{inner}}
$a.{
    $a.{Prop}
}
'@
                    ExpectedRanges = @(
                        ,@(17, 26)
                        ,@(30, 47)
                    )
                }
            )
        }

        It 'Should correctly identify range for <Name>' -ForEach $RangeTests {
            $tokens = $null
            $parseErrors = $null
            $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($ScriptDef, [ref] $tokens, [ref] $parseErrors)
            $tokenOperations = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations]::new($tokens, $scriptAst)
            $ranges = $tokenOperations.GetBracedMemberAccessRanges()
            $ranges.Count | Should -Be $ExpectedRanges.Count
            for ($i = 0; $i -lt $ranges.Count; $i++) {
                $ranges[$i].Item1 | Should -Be $ExpectedRanges[$i][0]
                $ranges[$i].Item2 | Should -Be $ExpectedRanges[$i][1]
            }
        }

        It 'Should not identify dot-sourcing as braced member access' {
            $scriptText = @'
. {5+5}
$a=4;. {10+15}
'@
            $tokens = $null
            $parseErrors = $null
            $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
            $tokenOperations = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations]::new($tokens, $scriptAst)
            $ranges = $tokenOperations.GetBracedMemberAccessRanges()
            $ranges.Count | Should -Be 0
        }

        It 'Should not return a range for an incomplete bracket pair (parse error)' {
            $scriptText = @'
$object.{MemberName
'@
            $tokens = $null
            $parseErrors = $null
            $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
            $tokenOperations = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations]::new($tokens, $scriptAst)
            $ranges = $tokenOperations.GetBracedMemberAccessRanges()
            $ranges.Count | Should -Be 0
        }

        It 'Should find the correct range for null-conditional braced member access' {
            $scriptText = '$object?.{Prop}'
            $tokens = $null
            $parseErrors = $null
            $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
            $tokenOperations = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations]::new($tokens, $scriptAst)
            $ranges = $tokenOperations.GetBracedMemberAccessRanges()
            $ranges.Count | Should -Be 1
            $ExpectedRanges = @(
                ,@(9, 15)
            )
            for ($i = 0; $i -lt $ranges.Count; $i++) {
                $ranges[$i].Item1 | Should -Be $ExpectedRanges[$i][0]
                $ranges[$i].Item2 | Should -Be $ExpectedRanges[$i][1]
            }
        } -Skip:$($PSVersionTable.PSVersion.Major -lt 7)

        It 'Should find the correct range for nested null-conditional braced member access' {
            $scriptText = '$object?.{Prop?.{InnerProp}}'
            $tokens = $null
            $parseErrors = $null
            $scriptAst = [System.Management.Automation.Language.Parser]::ParseInput($scriptText, [ref] $tokens, [ref] $parseErrors)
            $tokenOperations = [Microsoft.Windows.PowerShell.ScriptAnalyzer.TokenOperations]::new($tokens, $scriptAst)
            $ranges = $tokenOperations.GetBracedMemberAccessRanges()
            $ranges.Count | Should -Be 1
            $ExpectedRanges = @(
                ,@(9, 28)
            )
            for ($i = 0; $i -lt $ranges.Count; $i++) {
                $ranges[$i].Item1 | Should -Be $ExpectedRanges[$i][0]
                $ranges[$i].Item2 | Should -Be $ExpectedRanges[$i][1]
            }
        } -Skip:$($PSVersionTable.PSVersion.Major -lt 7)

    }

}