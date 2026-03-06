# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $Allman = @'
enum Color
{
    Black,
    White
}

function Test-Code
{
    [CmdletBinding()]
    param
    (
        [int] $ParameterOne
    )
    end
    {
        if (10 -gt $ParameterOne)
        {
            "Greater"
        }
        else
        {
            "Lesser"
        }
    }
}
'@

    $OTBS = @'
enum Color {
    Black,
    White
}

function Test-Code {
    [CmdletBinding()]
    param(
        [int] $ParameterOne
    )
    end {
        if (10 -gt $ParameterOne) {
            "Greater"
        } else {
            "Lesser"
        }
    }
}
'@
    $OTPS = @'
enum Color {
    Black,
    White
}

function Test-Code {
    [CmdletBinding()]
    param(
        [int] $ParameterOne
    )
    end {
        if (10 -gt $ParameterOne) {
            "Greater"
        }
        else {
            "Lesser"
        }
    }
}
'@
    $Stroustrup = @'
enum Color {
    Black,
    White
}

function Test-Code
{
    [CmdletBinding()]
    param(
        [int]$ParameterOne
    )
    end {
        if(10 -gt $ParameterOne) {
            "Greater"
        }
        else {
            "Lesser"
        }
    }
}
'@
}

Describe "CodeFormattingPresets" {
    Context "Allman" {
        It "To Allman from OTBS" {
            Invoke-Formatter -ScriptDefinition $OTBS -Settings 'CodeFormattingAllman' | Should -Be $Allman
        }
        It "To Allman from OTPS" {
            Invoke-Formatter -ScriptDefinition $OTPS -Settings 'CodeFormattingAllman' | Should -Be $Allman
        }
        It "To Allman from Stroustrup" {
            Invoke-Formatter -ScriptDefinition $Stroustrup -Settings 'CodeFormattingAllman' | Should -Be $Allman
        }
    }

    Context "OTBS" {
        It "To OTBS from Allman" {
            Invoke-Formatter -ScriptDefinition $Allman -Settings 'CodeFormattingOTBS' | Should -Be $OTBS
        }
        It "To OTBS from OTPS" {
            Invoke-Formatter -ScriptDefinition $OTPS -Settings 'CodeFormattingOTBS' | Should -Be $OTBS
        }
        It "To OTBS from Stroustrup" {
            Invoke-Formatter -ScriptDefinition $Stroustrup -Settings 'CodeFormattingOTBS' | Should -Be $OTBS
        }
    }

    Context "OTPS" {
        It "To OTPS from Allman" {
            Invoke-Formatter -ScriptDefinition $Allman -Settings 'CodeFormattingOTPS' | Should -Be $OTPS
        }
        It "To OTPS from OTBS" {
            Invoke-Formatter -ScriptDefinition $OTBS -Settings 'CodeFormattingOTPS' | Should -Be $OTPS
        }
        It "To OTPS from Stroustrup" {
            Invoke-Formatter -ScriptDefinition $Stroustrup -Settings 'CodeFormattingOTPS' | Should -Be $OTPS
        }
    }

    Context "Stroustrup" {
        It "To Stroustrup from Allman" {
            Invoke-Formatter -ScriptDefinition $Allman -Settings 'CodeFormattingStroustrup' | Should -Be $Stroustrup
        }
        It "To Stroustrup from OTBS" {
            Invoke-Formatter -ScriptDefinition $OTBS -Settings 'CodeFormattingStroustrup' | Should -Be $Stroustrup
        }
        It "To Stroustrup from OTPS" {
            Invoke-Formatter -ScriptDefinition $OTPS -Settings 'CodeFormattingStroustrup' | Should -Be $Stroustrup
        }
    }
}