# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    function Get-Extent {
        param($line, $startLineNum, $startColumnNum, $endLineNum, $endColumnNum)
        $scriptPositionType = 'System.Management.Automation.Language.ScriptPosition'
        $scriptExtentType = 'System.Management.Automation.Language.ScriptExtent'
        $extentStartPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $startLineNum, $startColumnNum, $line
        $extentEndPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $endLineNum, $endColumnNum, $line
        New-Object -TypeName $scriptExtentType -ArgumentList $extentStartPos, $extentEndPos
    }

    function Test-Extent {
        param(
            $translatedExtent,
            $expectedStartLineNumber,
            $expectedStartColumnNumber,
            $expectedEndLineNumber,
            $expectedEndColumnNumber)

        $translatedExtent.StartLineNumber | Should -Be $expectedStartLineNumber
        $translatedExtent.StartColumnNumber | Should -Be $expectedStartColumnNumber
        $translatedExtent.EndLineNumber | Should -Be $expectedEndLineNumber
        $translatedExtent.EndColumnNumber | Should -Be $expectedEndColumnNumber
    }

    $extNamespace = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]
}


Describe "String extension methods" {
    Context "When a text is given to GetLines" {
        It "Should return only one line if input is a single line." {
            $def = "This is a single line"
            $extNamespace::GetLines($def) | Get-Count | Should -Be 1
        }

        It "Should return 2 lines if input string has 2 lines." {
            $def = @'
This is line one.
This is line two.
'@
            $extNamespace::GetLines($def) | Get-Count | Should -Be 2
        }
    }
}
Describe "IScriptExtent extension methods" {
    Context "When a Range type input is given to ToRange" {
        It "Should maintain integrity" {
            $extent = Get-Extent $null 1 2 3 4
            $range = $extNamespace::ToRange($extent)

            $range.Start.Line | Should -Be $extent.StartLineNumber
            $range.Start.Column | Should -Be $extent.StartColumnNumber
            $range.End.Line | Should -Be $extent.EndLineNumber
            $range.End.Column | Should -Be $extent.EndColumnNumber
        }
    }
}

Describe "FunctionDefinitionAst extension methods" {
    Context "When a function parameter declaration is given to GetParameterAsts" {
        BeforeAll {
            $funcDefnAst = {function foo ($param1, $param2) {}}.Ast.EndBlock.Statements[0]
            $paramBlockAst = $null
            $parameterAsts = $extNamespace::GetParameterAsts($funcDefnAst, [ref] $paramBlockAst)
        }

        It "Should return the parameters" {
            $parameterAsts | Get-Count | Should -Be 2
        }

        It "Should set paramBlock to `$null" {
            $paramBlock | Should -Be $null
        }
    }

    Context "When a function with param block is given to GetParameterAsts" {
        BeforeAll {
            $funcDefnAst = {
                function foo {
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0]
            $paramBlockAst = $null
            $parameterAsts = $extNamespace::GetParameterAsts($funcDefnAst, [ref] $paramBlockAst)
        }

        It "Should return the parameters" {
            $parameterAsts | Get-Count | Should -Be 2
        }

        It "Should set paramBlock" {
            $paramBlockAst | Should -Not -Be $null
        }
    }
}

Describe "ParamBlockAst extension methods" {
    Context "GetCmdletBindingAttributeAst" {
        It "Should return the cmdletbinding attribute if present" {
            $funcDefnAst = {
                function foo {
                    [CmdletBinding()]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0]
            $extNamespace::GetCmdletBindingAttributeAst($funcDefnAst.Body.ParamBlock) | Should -Not -Be $null
        }
    }
}

Describe "AttributeAst extension methods" {
    Context "IsCmdletBindingAttributeAst" {
        It "Should return true if the attribute is a cmdletbinding attribute" {
            $funcDefnAst = {
                function foo {
                    [CmdletBinding()]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0]
            $extNamespace::IsCmdletBindingAttributeAst($funcDefnAst.Body.ParamBlock.Attributes[0]) |
                Should -BeTrue
        }
    }

    Context "GetSupportsShouldProcessAst" {
        It "Should return the SupportsShouldProcess named attribute argument" {
            $funcDefnAst = {
                function foo {
                    [CmdletBinding(SupportsShouldProcess)]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0]
            $attrAst = $extNamespace::GetSupportsShouldProcessAst($funcDefnAst.Body.ParamBlock.Attributes[0])
            $attrAst | Should -Not -Be $null
            $attrAst.Extent.Text | Should -Be "SupportsShouldProcess"
        }
    }
}

Describe "NamedAttributeArgumentAst" {
    Context "IsTrue" {
        It "Should return true if expression is omitted" {
            $attrAst = {
                function foo {
                    [CmdletBinding(SupportsShouldProcess)]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0].Body.ParamBlock.Attributes[0].NamedArguments[0]
            $expressionAst = $null
            $extNamespace::GetValue($attrAst, [ref]$expressionAst) | Should -BeTrue
            $expressionAst | Should -Be $null
        }

        It "Should return true if argument value is `$true" {
            $attrAst = {
                function foo {
                    [CmdletBinding(SupportsShouldProcess=$true)]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0].Body.ParamBlock.Attributes[0].NamedArguments[0]
            $expressionAst = $null
            $extNamespace::GetValue($attrAst, [ref]$expressionAst) | Should -BeTrue
            $expressionAst | Should -Not -Be $null
        }

        It "Should return false if argument value is `$false" {
            $attrAst = {
                function foo {
                    [CmdletBinding(SupportsShouldProcess=$false)]
                    param($param1, $param2)
                }}.Ast.EndBlock.Statements[0].Body.ParamBlock.Attributes[0].NamedArguments[0]
            $expressionAst = $null
            $extNamespace::GetValue($attrAst, [ref]$expressionAst) | Should -BeFalse
            $expressionAst | Should -Not -Be $null

        }
    }
}
