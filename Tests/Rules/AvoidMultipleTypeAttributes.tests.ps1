# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "AvoidMultipleTypeAttributes"

    $settings = @{
        IncludeRules = @($ruleName)
    }
}

Describe 'AvoidMultipleTypeAttributes' {
    It 'Correctly diagnoses and corrects <Script>' -TestCases @(
        @{ Script = 'function F1 ($s1, $p1){}' }
        @{ Script = 'function F2 ([int] $s2, [int] $p2){}' }
        @{ Script = 'function F3 ([int][switch] $s3, [int] $p3){}';Extent = @{ StartCol = 28; EndCol = 31 }; Message = 'Parameter ''$s3'' has more than one type specifier.' }
        @{ Script = 'function F4 ([int][ref] $s4, [int] $p4){}';Extent = @{ StartCol = 25; EndCol = 28 }; Message = 'Parameter ''$s4'' has more than one type specifier.' }
        @{ Script = 'function F5 ([int][switch][boolean] $s5, [int] $p5){}';Extent = @{ StartCol = 37; EndCol = 40 }; Message = 'Parameter ''$s5'' has more than one type specifier.' }
        @{ Script = 'function F6 ([ValidateSet()][int] $s6, [int] $p6){}' }
        @{ Script = 'function F7 ([Parameter(Mandatory=$true)][ValidateSet()][int] $s7, [int] $p7){}' }
    ) {
        param([string]$Script, $Extent, $Message)

        $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $Script

        if (-not $Extent)
        {
            $diagnostics | Should -BeNullOrEmpty
            return
        }

        $expectedStartLine = if ($Extent.StartLine) { $Extent.StartLine } else { 1 }
        $expectedEndLine = if ($Extent.EndLine) { $Extent.EndLine } else { 1 }

        $diagnostics.Extent.StartLineNumber | Should -BeExactly $expectedStartLine
        $diagnostics.Extent.EndLineNumber | Should -BeExactly $expectedEndLine
        $diagnostics.Extent.StartColumnNumber | Should -BeExactly $Extent.StartCol
        $diagnostics.Extent.EndColumnNumber | Should -BeExactly $Extent.EndCol

        $diagnostics.Message | Should -BeExactly $Message
    }
}
