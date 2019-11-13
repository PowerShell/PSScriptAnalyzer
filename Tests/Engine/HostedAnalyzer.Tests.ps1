# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

Describe "Hosted Analyzer Tests" {
    BeforeAll {
        $HostedAnalyzer = new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
    }
    AfterAll {
        $HostedAnalyzer.Dispose()
    }

    Context "Analyze Method Tests" {
        $defaultAnalyzerTests =
            @{ Script = 'gci'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = 'gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = 'get-command;gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = '$a ='; ExpectedResultCount = 2; ExpectedRuleViolation = @('ExpectedValueExpression','PSUseDeclaredVarsMoreThanAssignments') },
            @{ Script = 'gc;$a ='; ExpectedResultCount = 3; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases','ExpectedValueExpression','PSUseDeclaredVarsMoreThanAssignments') },
            @{ Script = 'write-host no'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost')  }

        Context "Tests without Settings" {

            It "Should correctly identify errors in '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $result = $HostedAnalyzer.Analyze($script)
                $result.Type | Should -Be "Script"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in the ast of '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput($script, [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.Analyze($ast, $t, "")
                $result.Type | Should -Be "Ast"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count + $errs.Count| Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                # the AST analysis does not include parse errors, so those must be removed
                $expectedRules = ($ExpectedRuleViolation |?{$_ -ne "ExpectedValueExpression"}| Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in a file of contents '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $fname = "TESTDRIVE:/{0}.ps1" -f [guid]::newguid()
                $script | Set-Content $fname
                $finfo = [System.IO.FileInfo]((Get-Item $fname).FullName)
                $result = $HostedAnalyzer.Analyze($finfo)
                $result.Type | Should -Be "File"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count + $errs.Count| Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation  | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should produce the correct error if the file is not found"  {
                $finfo = [System.IO.FileInfo]"thisFileProbablyDoesNotExist.ps1"
                $result = $HostedAnalyzer.Analyze($finfo)
                $result.Type | Should -Be "File"
                $result.Errors.Count | Should -Be 1
                $result.Errors[0].Exception |  Should -BeOfType [System.IO.FileNotFoundException]
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be 0
            }

            It "Should correctly identify errors in an AST without settings" {
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput('wjb', [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.Analyze($ast, $tokens, "")
                $result.Result.Count | Should -Be 1
                $result.Result.RuleName | Should -Be 'PSAvoidUsingCmdletAliases'
            }
        }


        Context "Tests with Settings" {

            $settingsAnalyzerTests =
                @{ Script = 'write-host no1'; ExpectedResultCount = 0; ExpectedRuleViolation = @(); Settings = $HostedAnalyzer.CreateSettings('PSGallery') },
                @{ Script = 'write-host no2'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'write-host no3'; ExpectedResultCount = 0; ExpectedRuleViolation = @(); Settings = $($s = $HostedAnalyzer.CreateSettings(); $null = $s.ExcludeRules.Add('PSAvoidUsingWriteHost'); $s) },
                @{ Script = 'gci'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'get-command;gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'write-host no'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost'); Settings = $HostedAnalyzer.CreateSettings()  }

            It "Should correctly identify errors in '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $result = $HostedAnalyzer.Analyze($script, $settings)
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in a file of contents '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $fname = "TESTDRIVE:/{0}.ps1" -f [guid]::newguid()
                $script | Set-Content $fname
                $finfo = [System.IO.FileInfo]((Get-Item $fname).FullName)
                $result = $HostedAnalyzer.Analyze($finfo, $settings)
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in the ast of '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $expectedRules = $ExpectedRuleViolation |?{$_ -ne "ExpectedValueExpression"}| Sort-Object
                $expectedRuleCount = @($expectedRules).Count
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput($script, [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.Analyze($ast, $t, $settings, "")
                $result.Type | Should -Be "Ast"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                # the AST analysis does not include parse errors, so those must be removed
                $expectedRules = ($expectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $result = $HostedAnalyzer.Analyze($script, $settings)
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }
        }

    }
}

Describe "Async Hosted Analyzer Tests" {
    BeforeAll {
        $HostedAnalyzer = new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
    }
    AfterAll {
        $HostedAnalyzer.Dispose()
    }

    Context "Async Analyze Method Tests" {
        $defaultAnalyzerTests =
            @{ Script = 'gci'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = 'gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = 'get-command;gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases') },
            @{ Script = '$a ='; ExpectedResultCount = 2; ExpectedRuleViolation = @('ExpectedValueExpression','PSUseDeclaredVarsMoreThanAssignments') },
            @{ Script = 'gc;$a ='; ExpectedResultCount = 3; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases','ExpectedValueExpression','PSUseDeclaredVarsMoreThanAssignments') },
            @{ Script = 'write-host no'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost')  }

        Context "Async Tests without Settings" {

            It "Should correctly identify errors in '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $result = $HostedAnalyzer.AnalyzeAsync($script).GetAwaiter().GetResult()
                $result.Type | Should -Be "Script"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in the ast of '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput($script, [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.AnalyzeAsync($ast, $t, "").GetAwaiter().GetResult()
                $result.Type | Should -Be "Ast"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count + $errs.Count| Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                # the AST analysis does not include parse errors, so those must be removed
                $expectedRules = ($ExpectedRuleViolation |?{$_ -ne "ExpectedValueExpression"}| Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in a file of contents '<script>' without settings"  -testcase $defaultAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation )
                $fname = "TESTDRIVE:/{0}.ps1" -f [guid]::newguid()
                $script | Set-Content $fname
                $finfo = [System.IO.FileInfo]((Get-Item $fname).FullName)
                $result = $HostedAnalyzer.AnalyzeAsync($finfo).GetAwaiter().GetResult()
                $result.Type | Should -Be "File"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count + $errs.Count| Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation  | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should produce the correct error if the file is not found"  {
                $finfo = [System.IO.FileInfo]"thisFileProbablyDoesNotExist.ps1"
                $result = $HostedAnalyzer.AnalyzeAsync($finfo).GetAwaiter().GetResult()
                $result.Type | Should -Be "File"
                $result.Errors.Count | Should -Be 1
                $result.Errors[0].Exception |  Should -BeOfType [System.IO.FileNotFoundException]
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be 0
            }

            It "Should correctly identify errors in an AST without settings" {
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput('wjb', [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.AnalyzeAsync($ast, $tokens, "").GetAwaiter().GetResult()
                $result.Result.Count | Should -Be 1
                $result.Result.RuleName | Should -Be 'PSAvoidUsingCmdletAliases'
            }
        }


        Context "Async Tests with Settings" {

            $settingsAnalyzerTests =
                @{ Script = 'write-host no1'; ExpectedResultCount = 0; ExpectedRuleViolation = @(); Settings = $HostedAnalyzer.CreateSettings('PSGallery') },
                @{ Script = 'write-host no2'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'write-host no3'; ExpectedResultCount = 0; ExpectedRuleViolation = @(); Settings = $($s = $HostedAnalyzer.CreateSettings(); $null = $s.ExcludeRules.Add('PSAvoidUsingWriteHost'); $s) },
                @{ Script = 'gci'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'get-command;gc'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingCmdletAliases'); Settings = $HostedAnalyzer.CreateSettings() },
                @{ Script = 'write-host no'; ExpectedResultCount = 1; ExpectedRuleViolation = @('PSAvoidUsingWriteHost'); Settings = $HostedAnalyzer.CreateSettings()  }

            It "Should correctly identify errors in '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $result = $HostedAnalyzer.AnalyzeAsync($script, $settings).GetAwaiter().GetResult()
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in a file of contents '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $fname = "TESTDRIVE:/{0}.ps1" -f [guid]::newguid()
                $script | Set-Content $fname
                $finfo = [System.IO.FileInfo]((Get-Item $fname).FullName)
                $result = $HostedAnalyzer.AnalyzeAsync($finfo, $settings).GetAwaiter().GetResult()
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in the ast of '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $expectedRules = $ExpectedRuleViolation |?{$_ -ne "ExpectedValueExpression"}| Sort-Object
                $expectedRuleCount = @($expectedRules).Count
                $tokens = $errs = $null
                $ast = [System.Management.Automation.Language.Parser]::ParseInput($script, [ref]$tokens, [ref]$errs)
                $result = $HostedAnalyzer.AnalyzeAsync($ast, $t, $settings, "").GetAwaiter().GetResult()
                $result.Type | Should -Be "Ast"
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                # the AST analysis does not include parse errors, so those must be removed
                $expectedRules = ($expectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }

            It "Should correctly identify errors in '<script>' with settings"  -testcase $settingsAnalyzerTests {
                param ( $script, $expectedResultCount, $expectedRuleViolation, $settings )
                $result = $HostedAnalyzer.AnalyzeAsync($script, $settings).GetAwaiter().GetResult()
                $result.Errors | Should -BeNullOrEmpty
                $result.TerminatingErrors | Should -BeNullOrEmpty
                $result.Result.Count | Should -Be $expectedResultCount
                $observedRules = ($result.Result.RuleName | Sort-Object) -join ":"
                $expectedRules = ($ExpectedRuleViolation | Sort-Object) -join ":"
                $observedRules | Should -Be $expectedRules
            }
        }

    }
}

Describe "Create Settings APIs" {
    BeforeAll {
        $HostedAnalyzer = new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
    }
    AfterAll {
        $HostedAnalyzer.Dispose()
    }

    Context "Create Default Settings" {
        BeforeAll {
            $settings = $HostedAnalyzer.CreateSettings()
        }

        It "Should be able to create a default settings object" {
            $settings | Should -BeOfType [Microsoft.Windows.PowerShell.ScriptAnalyzer.Settings]
        }

        $settingsPropertyValues =
            @{ Name = "RecurseCustomRulePath"; Value = $false; type = "bool" },
            @{ Name = "IncludeDefaultRules";   Value = $true;  type = "bool" },
            @{ Name = "FilePath";              Value = $null;  type = "string" },
            @{ Name = "IncludeRules";          Value = 0;      type = "array" },
            @{ Name = "ExcludeRules";          Value = 0;      type = "array" },
            @{ Name = "Severities";            Value = 0;      type = "array" },
            @{ Name = "CustomRulePath";        Value = $null;  type = "string" },
            @{ Name = "RuleArguments";         Value = 0;      type = "array" }
        It "Should have the proper values set on the <Name> property" -testcase $settingsPropertyValues {
            param ( $Name, $Value, $Type )
            switch -regex ( $type ) {
                "bool|string" { $settings.$name | Should -Be $value }
                "array" { $settings.$name.Count | Should -Be $value }
                default { throw "unknown type" }
            }
        }
    }

    Context "Create Settings via hashtable" {

        It "Should be able to create a simple settings object from a hashtable" {
            $setting = $HostedAnalyzer.CreateSettings(@{ ExcludeRules = "PSAvoidUsingCmdletAliases" })
            $result = $hostedanalyzer.Analyze('gci',$setting)
            $result.Result.Count | Should -Be 0
        }

        It "Should be able to create a configurable rule settings object from a hashtable" {
            $ruleName = "PSUseCorrectCasing"
            $setting = $HostedAnalyzer.CreateSettings(@{
                IncludeRules = @( $ruleName )
                Rules = @{
                    $ruleName = @{
                        Enable = $true
                    }
                }
            })
            $result = $HostedAnalyzer.Analyze('get-command', $setting)
            $result.Result.Count | Should -Be 1
            $result.Result.RuleName | Should -Be $ruleName
        }
    }

    Context "Create Settings via well known name" {
        It "Should create a setting for CodeFormatting" {
            $setting = $HostedAnalyzer.CreateSettings("CodeFormatting")
            $createdRules = ($setting.IncludeRules | Sort-Object) -join ":"
            $expectedRules = "PSAlignAssignmentStatement:PSPlaceCloseBrace:PSPlaceOpenBrace:PSUseConsistentIndentation:PSUseConsistentWhitespace:PSUseCorrectCasing"
            $createdRules | Should -Be $expectedRules

        }

        It "Should create settings which can be used in analyze" {
            $setting = $HostedAnalyzer.CreateSettings("CodeFormatting")
            $result = $HostedAnalyzer.Analyze('get-childitem', $setting)
            $result.Result.Count | Should -Be 1
            $result.Result.RuleName | Should -Be "PSUseCorrectCasing"
        }
    }

}

Describe "Get Rule Apis" {
    BeforeAll {
        $HostedAnalyzer = new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
        $AnalyzerRules = Get-ScriptAnalyzerRule
        $UsePattern = '*use*'
        $UseRules = Get-ScriptAnalyzerRule -Name $UsePattern
        $RuleArray = "PSUseSupportsShouldProcess","PSUseApprovedVerbs"
        $ArrayRules = Get-ScriptAnalyzerRule -Name $RuleArray
    }
    AfterAll {
        $HostedAnalyzer.Dispose()
    }

    $testCases = @{ Rules = $null; ExpectedRules = $AnalyzerRules; Name = "All Rules" },
        @{ Rules = $UsePattern; ExpectedRules = $UseRules; Name = "Pattern Rules" },
        @{ Rules = $ArrayRules; ExpectedRules = $ArrayRules; Name = "Array Rules" }

    It "Hosted analyzer should get the same rule list as the cmdlet for '<Name>'" -testcases $testCases {
        param ( $Rules, $ExpectedRules, $Name )
        $retrievedRules = $HostedAnalyzer.GetBuiltinRules($Rules)
        $retrievedRules.Count | Should -Be $ExpectedRules.Count
        $retrievedRuleString = ($retrievedRules.Name|Sort-Object) -join ":"
        $analyzerRuleString = ($AnalyzerRules.Name|Sort-Object) -join ":"
        $retrievedRuleString | Should -BeExactly $analyzerRuleString
    }

}

Describe "Fix Api" {
    BeforeAll {
        $HostedAnalyzer = new-object Microsoft.Windows.PowerShell.ScriptAnalyzer.Hosting.HostedAnalyzer
        $Setting1 = $HostedAnalyzer.CreateSettings()
        $Setting1.ExcludeRules.Add("PSAvoidUsingCmdletAliases")
    }
    AfterAll {
        $HostedAnalyzer.Dispose()
    }

    $testCases = @{ Script = "gci"; Expected = "Get-ChildItem" },
        @{ Script = "gci;gc"; Expected = "Get-ChildItem;Get-Content" },
        @{ Script = "gci|`nii"; Expected = "Get-ChildItem|`nInvoke-Item" }

    It "Should be able to fix errors in '<Script>' with default rules" -TestCases $testCases {
        param ( $Script, $Expected )
        $result = $HostedAnalyzer.Fix($script)
        $result | Should -BeExactly $expected
    }

    $testCases = @{ Script = "gci"; Expected = "Get-ChildItem"; Settings = $hostedAnalyzer.CreateSettings() },
        @{ Script = "gci;gc"; Expected = "Get-ChildItem;Get-Content"; Settings = $hostedAnalyzer.CreateSettings() },
        @{ Script = "gci|`nii"; Expected = "Get-ChildItem|`nInvoke-Item"; Settings = $hostedAnalyzer.CreateSettings() }

    It "Should be able to fix errors in '<Script>' for only included rules when settings are provided" -TestCases $testCases {
        param ( $Script, $Expected, $settings )
        $result = $HostedAnalyzer.Fix($script, $settings)
        $result | Should -BeExactly $expected
    }

}
