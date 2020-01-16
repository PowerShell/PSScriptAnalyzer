# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

$script:RuleName = 'PSUseCompatibleSyntax'

Describe "PSUseCompatibleSyntax" {
    BeforeAll {
        $testCases = @(
            @{ Script = '$x = [MyClass]::new()'; Versions = @(3,4) }
            @{ Script = '$member = "Hi"; $x.$member'; Versions = @() }
            @{ Script = 'Write-Host "Banana"'; Versions = @() }
            @{ Script = '[System.VeryInnocuousType]::RunApiMethod($obj)'; Versions = @() }
            @{ Script = '$y.$methodWithAVeryLongName()'; Versions = @(3) }
            @{ Script = '$typeExpression::$staticMember'; Versions = @() }
            @{ Script = '$typeExpression::$dynamicStaticMethodName()'; Versions = @(3) }
        )

        # PS v3/4 won't parse classes or enums
        if ($PSVersionTable.PSVersion.Major -ge 5)
        {
            $testCases += @(
                @{ Script = "class MyClass { }"; Versions = @(3,4) }
                @{ Script = "enum MyEnum { One; Two }"; Versions = @(3,4) }
            )
        }

        # PS v6+ won't parse workflows
        if ($PSVersionTable.PSVersion.Major -le 5)
        {
            $testCases += @(
                @{ Script = 'workflow Banana { Do-ExpensiveCommandOnAnotherMachine -Argument "Banana" }'; Versions = @(6) }
            )
        }
    }

    foreach ($v in 3,4,5,6)
    {
        It "Finds issues for PSv$v in '<Script>'" -TestCases $testCases {
            param([string]$Script, $Versions)

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $Script -IncludeRule PSUseCompatibleSyntax -Settings @{ Rules = @{ PSUseCompatibleSyntax = @{ Enable = $true; TargetVersions = @("$v.0") } } }

            if ($Versions -contains $v)
            {
                $diagnostics.Count | Should -Be 1
            }
            else
            {
                $diagnostics.Count | Should -Be 0
            }
        }
    }

    It "Finds incompatibilities in a script file" {
        $settings = @{ Rules = @{ PSUseCompatibleSyntax = @{ Enable = $true; TargetVersions = @("3.0", "4.0", "5.1", "6.0") } } }

        $diagnostics = Invoke-ScriptAnalyzer -IncludeRule PSUseCompatibleSyntax -Path "$PSScriptRoot/CompatibilityRuleAssets/IncompatibleScript.ps1" -Settings $settings `
            | Where-Object { $_.RuleName -eq 'PSUseCompatibleSyntax' }

        if ($PSVersionTable.PSVersion.Major -ge 5)
        {
            $expected = 5
        }
        else
        {
            # PSv3/4 can't detect class/enum parts
            $expected = 4
        }

        $diagnostics.Count | Should -Be $expected
    }

    It "Ensures there are no incompatibilities in PSSA build files" {
        $settings = @{ Rules = @{ PSUseCompatibleSyntax = @{ Enable = $true; TargetVersions = @("3.0", "4.0", "5.1", "6.0") } } }

        $diagnostics = Invoke-ScriptAnalyzer -IncludeRule PSUseCompatibleSyntax -Path "$PSScriptRoot/../../" -Settings $settings

        $diagnostics.Count | Should -Be 0
    }
}