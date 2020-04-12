# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
    $editableTextType = "Microsoft.Windows.PowerShell.ScriptAnalyzer.EditableText"
    $textEditType = "Microsoft.Windows.PowerShell.ScriptAnalyzer.TextEdit"
}

Describe "EditableText class" {
    Context "When a single edit is given for application" {
        It "Should replace in a single line string in the middle" {
            $def = 'This is just a single line.'
            $edit = New-Object -TypeName $textEditType -ArgumentList 1,14,1,22,"one"
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be "This is just one line."
        }

        It "Should replace in a single line string in the start" {
            $def = 'This is just a single line.'
            $edit = New-Object -TypeName $textEditType -ArgumentList 1,1,1,5,"That"
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be 'That is just a single line.'
        }

        It "Should replace in a single line string in the end" {
            $def = 'This is just a single line.'
            $edit = New-Object -TypeName $textEditType -ArgumentList 1,23,1,27,"sentence"
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be 'This is just a single sentence.'
        }

        It "Should replace in multi-line string" {
            $def = @"
This is an outer string
that spans more than
two lines
"@
            $expected = @"
This is an outer string
that spans three
lines
"@
            $newText = @"
three

"@
            $edit = New-Object -TypeName $textEditType -ArgumentList 2,12,3,5,$newText
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be $expected
        }

        It "Should delete in a multi-line string" {
            $def = @'
function foo {
    param(
        [bool] $param1,
        [bool] $whatif
    )
}
'@
            $expected = @'
function foo {
    param(
        [bool] $param1
    )
}
'@
            $newText = ''
            $edit = New-Object -TypeName $textEditType -ArgumentList 3,23,4,23,$newText
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be $expected
        }

        It "Should insert in a multi-line string" {
            $def = @'
function foo {
    param(
        [bool] $param1
    )
}
'@
            $expected = @'
function foo {
    [CmdletBinding()]
    param(
        [bool] $param1
    )
}
'@
            # Editor does not allow trailing white-spaces, hence this weird construct.
            $newText = @"
    [CmdletBinding()]

"@
            $edit = New-Object -TypeName $textEditType -ArgumentList 2,1,2,1,$newText
            $editableText = New-Object -TypeName $editableTextType -ArgumentList $def
            $result = $editableText.ApplyEdit($edit)
            $result.ToString() | Should -Be $expected
        }

        It "Should return a read-only collection of lines in the text" {
            $def = @'
function foo {
    param(
        [bool] $param1
    )
}
'@
            $text = New-Object `
                -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.EditableText" `
                -ArgumentList @($def)

            {$text.Lines.Add("abc")} | Should -Throw
        }

        It "Should return the correct number of lines in the text" {
            $def = @'
function foo
{
get-childitem
$x=1+2
$hashtable = @{
property1 = "value"
    anotherProperty = "another value"
}
}
'@
            $text = New-Object `
                -TypeName "Microsoft.Windows.PowerShell.ScriptAnalyzer.EditableText" `
                -ArgumentList @($def)
            $text.LineCount | Should -Be 9
        }
     }
}
