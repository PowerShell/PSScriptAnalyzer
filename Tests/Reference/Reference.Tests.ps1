# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.
Describe "Compiling a hosted analyzer" {
    BeforeAll {
        dotnet build --configuration Release
        $TestRoot = $PSScriptRoot
        $assembly = "${TestRoot}/bin/Release/netstandard2.0/Reference.dll"
    }

    It "dotnet build should have created an assembly" {
        $assembly | Should -Exist
    }

    It "The assembly should be loadable" {
        $m = Import-Module "${assembly}" -PassThru
        $m.Name | Should -Be "Reference"
    }

    Context "The hosted analyzer should be able to analyze" {
        BeforeAll {
            $HostedAnalyzer = [HostedAnalyzerTest.Analyzer]::new()
        }

        It "Should be able to analyze for Test1" {
            $result = $HostedAnalyzer.Test1()
            $result.Result.Count | Should -Be 1
            $result.Result.RuleName | Should -Be "PSAvoidUsingCmdletAliases"
        }

        It "Should be able to analyze for Test2" {
            $result = $HostedAnalyzer.Test2()
            $result.Result.Count | Should -Be 2
            $expected =  "PSAvoidUsingCmdletAliases","PSAvoidUsingCmdletAliases" | Sort-Object
            $result.Result.RuleName | Sort-Object | Should -Be $expected
        }

        It "Should be able to analyze for Test3" {
            $result = $HostedAnalyzer.Test3()
            $result.Result.Count | Should -Be 3
            $expected = "PSAvoidUsingCmdletAliases","PSAvoidUsingCmdletAliases","PSUseDeclaredVarsMoreThanAssignments" | Sort-Object
            $result.Result.RuleName | Sort-Object | Should -Be $expected
        }

        It "Should be able to analyze for Test4" {
            $result = $HostedAnalyzer.Test4()
            $result | Should -BeExactly "Get-ChildItem"
        }

        It "Should be able to analyze for Test5" {
            $result = $HostedAnalyzer.Test5()
            $result | Should -BeExactly 'Get-ChildItem|ForEach-Object{$_}'
        }

        It "Should be able to analyze for Test6" {
            $result = $HostedAnalyzer.Test6()
            $result | Should -BeExactly 'Get-ChildItem | ForEach-Object { $_ } | Where-Object { $_ }'
        }

    }
}
