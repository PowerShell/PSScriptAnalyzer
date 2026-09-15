# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

using namespace System.Management.Automation.Language

[Diagnostics.CodeAnalysis.SuppressMessage('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'False positive')]
[Diagnostics.CodeAnalysis.SuppressMessage('PSUseApprovedVerbs', '', Justification = 'Required for custom test')]
param()

BeforeDiscovery {
    function Should-Parse([string] $ActualValue, [switch] $Negate, [string] $Because) {
        $errors = $Null
        $null = [Parser]::ParseInput($ActualValue, [ref]$null, [ref]$errors)
        $succeeded = -not $errors -xor $Negate
        if (-not $succeeded) {
            $not = if ($Negate) { ' not' }
            $failureMessage = "Expected '$ActualValue'$Not to parse$(if($Because) { " because $Because"})."
        }

        return [PSCustomObject]@{
            Succeeded      = $succeeded
            FailureMessage = $failureMessage
        }
    }

    $ShouldParseSplat = @{
        Name               = 'Parse'
        InternalName       = 'Should-Parse'
        Test               = ${function:Should-Parse}
        SupportsArrayInput = $true
    }
    Add-ShouldOperator @ShouldParseSplat
}

BeforeAll {
    $ruleName = "PSAvoidUsingNewObject"
    $ruleMessage = "Avoid using the 'New-Object' cmdlet to create objects of type '{0}' as it might perform poorly."
    $correctionDescription = "Use the type initializer '[{0}]' to construct or cast the intended object."
}

Describe "AvoidUsingNewObject" {

    BeforeAll {
        $Settings = @{
            IncludeRules = @($ruleName)
            Rules        = @{ $ruleName = @{ Enable = $true } }
        }
    }

    Context "Examples" {

        It 'Version "1.2.3"' {
            $scriptDefinition = { $Version = New-Object -TypeName Version -ArgumentList "1.2.3" }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object -TypeName Version -ArgumentList "1.2.3"'
            $violations.Message                          | Should -Be ($ruleMessage -f 'Version')
            $violations.RuleSuppressionID                | Should -Be 'Version'
            $violations.SuggestedCorrections.Text        | Should -Be '[Version]"1.2.3"'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'Version')
        }

        It 'PSCustomObject Property' {
            $scriptDefinition = {
                for ($i = 0; $i -lt 100000; $i++) {
                    $resultObject = New-Object PSCustomObject -Property @{
                        Name = "Name$i"
                        Value = $i
                    }
                }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -BeLike 'New-Object PSCustomObject -Property @{*}'
            $violations.Message                          | Should -Be ($ruleMessage -f 'PSCustomObject')
            $violations.RuleSuppressionID                | Should -Be 'PSCustomObject'
            $violations.SuggestedCorrections.Text        | Should -BeLike '`[PSCustomObject`]@{*}'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'PSCustomObject')
        }

        It 'HashSet InvariantCultureIgnoreCase' {
            $scriptDefinition = {
                $hashSet = New-Object `
                    -TypeName 'System.Collections.Generic.HashSet[String]' `
                    -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Not -BeNullOrEmpty
            $violations.Message                          | Should -Be ($ruleMessage -f 'System.Collections.Generic.HashSet[String]')
            $violations.RuleSuppressionID                | Should -Be 'System.Collections.Generic.HashSet[String]'
            $violations.SuggestedCorrections.Text        | Should -Be '[System.Collections.Generic.HashSet[String]]::new([StringComparer]::InvariantCultureIgnoreCase)'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'System.Collections.Generic.HashSet[String]')
        }
    }

    Context "Cast" {

        It "String 'Hello World'" {
            $scriptDefinition = { $String = New-Object String 'Hello World' }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be "New-Object String 'Hello World'"
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections.Text        | Should -Be "[String]'Hello World'"
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'String')
        }

        It "String ArgumentList 'Hello World'" {
            $scriptDefinition = { $String = New-Object -TypeName String -ArgumentList 'Hello World' }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be "New-Object -TypeName String -ArgumentList 'Hello World'"
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections.Text        | Should -Be "[String]'Hello World'"
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'String')
        }

        It 'String ArgumentList Test' {
            $scriptDefinition = { $String = New-Object -TypeName String -ArgumentList Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object -TypeName String -ArgumentList Test'
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections.Text        | Should -Be '[String]"Test"'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'String')
        }
    }

    Context 'Construct' {

        It 'Empty string' {
            $scriptDefinition = { $String = New-Object String }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object String'
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections.Text        | Should -Be '[String]::new()'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'String')
        }

        It 'String ArgumentList ($Test)' {
            $scriptDefinition = { $String = New-Object -TypeName String -ArgumentList ($Test) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object -TypeName String -ArgumentList ($Test)'
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections.Text        | Should -Be '[String]::new($Test)'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'String')
        }

        It 'int[] (1..3)' {
            $scriptDefinition = { $Integers = New-Object int[] (1..3) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object int[] (1..3)'
            $violations.Message                          | Should -Be ($ruleMessage -f 'int[]')
            $violations.RuleSuppressionID                | Should -Be 'int[]'
            $violations.SuggestedCorrections.Text        | Should -Be '[int[]]::new(1..3)'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'int[]')
        }

        It 'HashSet[String] ($strings, $ignoreCase)' {
            $scriptDefinition = {
                $strings = [string[]]('a', 'A')
                $ignoreCase = [StringComparer]::InvariantCultureIgnoreCase
                $hashSet = New-Object "System.Collections.Generic.HashSet[String]" ($strings, $ignoreCase)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object "System.Collections.Generic.HashSet[String]" ($strings, $ignoreCase)'
            $violations.Message                          | Should -Be ($ruleMessage -f 'System.Collections.Generic.HashSet[String]')
            $violations.RuleSuppressionID                | Should -Be 'System.Collections.Generic.HashSet[String]'
            $violations.SuggestedCorrections.Text        | Should -Be '[System.Collections.Generic.HashSet[String]]::new($strings, $ignoreCase)'
            $violations.SuggestedCorrections.Text        | Should -Parse
            $violations.SuggestedCorrections.Description | Should -Be ($correctionDescription -f 'System.Collections.Generic.HashSet[String]')
        }
    }

    Context "Permit" {

        It 'ComObject' {
            $scriptDefinition = {
                $IE1 = New-Object -ComObject InternetExplorer.Application -Property @{ Navigate2="www.microsoft.com"; Visible = $true }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'Write-Host' {
            $scriptDefinition = {
                Write-Host New-Object String 123
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'Splat' {
            $scriptDefinition = {
                for ($i = 0; $i -lt 100000; $i++) {
                    $Splat = @{
                        TypeName = 'PSCustomObject'
                        Property = @{
                            Name = "Name$i"
                            Value = $i
                        }
                    }
                    $resultObject = New-Object @Splat
                }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'Property and ArgumentList' {
            $scriptDefinition = {
                Class MyClass {
                    $Foo
                    $Bar
                    MyClass($Bar) { $this.Bar = $Bar }
                }
                $MyObject = New-Object -TypeName MyClass -Property @{ Foo = 1 } -ArgumentList 2
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context 'Specials' {

        It 'Variable property' {
            $scriptDefinition = { New-Object MyClass -Property $properties }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]$properties'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Integer argument' {
            $scriptDefinition = { New-Object MyClass 1 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]1'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Bare string argument' {
            $scriptDefinition = { New-Object MyClass Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]"Test"'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Single quoted string argument' {
            $scriptDefinition = { New-Object MyClass 'Test' }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be "[MyClass]'Test'"
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Double quoted string argument' {
            $scriptDefinition = { New-Object MyClass "Test" }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]"Test"'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Bare string argument with embedded double quotes' {
            $scriptDefinition = { New-Object MyClass Test""123 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]"Test123"'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument list' {
            $scriptDefinition = { New-Object MyClass 1, 2 }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new(1, 2)'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument group' {
            $scriptDefinition = { New-Object MyClass (1, 2) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new(1, 2)'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument array' {
            $scriptDefinition = { New-Object MyClass @(1, 2) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new(1, 2)'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument subexpression' {
            $scriptDefinition = { New-Object MyClass $(1,2) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new(1,2)'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument group with single item' {
            $scriptDefinition = { New-Object MyClass (,1) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new((,1))'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument array with single item' {
            $scriptDefinition = { New-Object MyClass @(,1) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new(@(,1))'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument subexpression with single item' {
            $scriptDefinition = { New-Object MyClass $(,1) }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.SuggestedCorrections.Text | Should -Be '[MyClass]::new($(,1))'
            $violations.SuggestedCorrections.Text | Should -Parse
        }

        It 'Argument variable' {
            # The SuggestedCorrections is inconclusive, as we can't be sure if the variable contains
            # a single value that can be used in a type initializer,
            # or if it contains multiple values that would require splatting,
            # which can't be automatically determined from the AST.

            $scriptDefinition = { New-Object String $Test }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations.Count                            | Should -Be 1
            $violations.Severity                         | Should -Be Warning
            $violations.Extent.Text                      | Should -Be 'New-Object String $Test'
            $violations.Message                          | Should -Be ($ruleMessage -f 'String')
            $violations.RuleSuppressionID                | Should -Be 'String'
            $violations.SuggestedCorrections             | Should -BeNullOrEmpty
        }

    }

    Context "Disable" {

        BeforeAll {
            $Settings = @{
                IncludeRules = @($ruleName)
                Rules        = @{ $ruleName = @{ Enable = $false } }
            }
        }

        It 'Version "1.2.3"' {
            $scriptDefinition = { $Version = New-Object -TypeName Version -ArgumentList "1.2.3" }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'PSCustomObject Property' {
            $scriptDefinition = {
                for ($i = 0; $i -lt 100000; $i++) {
                    $resultObject = New-Object PSCustomObject -Property @{
                        Name = "Name$i"
                        Value = $i
                    }
                }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'HashSet InvariantCultureIgnoreCase' {
            $scriptDefinition = {
                $hashSet = New-Object `
                    -TypeName 'System.Collections.Generic.HashSet[String]' `
                    -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Suppress" {

        It 'Version "1.2.3"' {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidUsingNewObject', '', Justification = 'Test')] # All ids
                param()
                $Version = New-Object -TypeName Version -ArgumentList "1.2.3"
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'PSCustomObject Property' {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidUsingNewObject', 'PSCustomObject', Justification = 'Test')]
                param()
                for ($i = 0; $i -lt 100000; $i++) {
                    $resultObject = New-Object PSCustomObject -Property @{
                        Name = "Name$i"
                        Value = $i
                    }
                }
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }

        It 'HashSet InvariantCultureIgnoreCase' {
            $scriptDefinition = {
                [Diagnostics.CodeAnalysis.SuppressMessage('PSAvoidUsingNewObject', 'System.Collections.Generic.HashSet[String]', Justification = 'Test')]
                param()
                $hashSet = New-Object `
                    -TypeName 'System.Collections.Generic.HashSet[String]' `
                    -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
            }.ToString()
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $Settings
            $violations | Should -BeNullOrEmpty
        }
    }

    Context "Fix" {

        It "MyClass Properties" {
            $tempFile = Join-Path $TestDrive 'TestScript1.ps1'
            Set-Content -LiteralPath $tempFile -Value {New-Object MyClass -Property $properties} -NoNewLine
            $violations = Invoke-ScriptAnalyzer -Path $tempFile -Settings $Settings -fix
            Get-Content -LiteralPath $tempFile -Raw | Should -Be {[MyClass]$properties}.ToString()
        }

        It 'PSCustomObject Property' {
            $tempFile = Join-Path $TestDrive 'TestScript2.ps1'
            Set-Content -LiteralPath $tempFile -NoNewline -Value {
                for ($i = 0; $i -lt 100000; $i++) {
                    $resultObject = New-Object PSCustomObject -Property @{
                        Name = "Name$i"
                        Value = $i
                    }
                }
            }
            $violations = Invoke-ScriptAnalyzer -Path $tempFile -Settings $Settings -fix
            $Content = Get-Content -LiteralPath $tempFile -Raw
            $Content | Should -BeLike '*[PSCustomObject]*'
            $Content | Should -Parse
        }

        It 'HashSet InvariantCultureIgnoreCase' {
            $tempFile = Join-Path $TestDrive 'TestScript3.ps1'
            Set-Content -LiteralPath $tempFile -NoNewline -Value {
                $hashSet = New-Object `
                    -TypeName 'System.Collections.Generic.HashSet[String]' `
                    -ArgumentList ([StringComparer]::InvariantCultureIgnoreCase)
            }
            $violations = Invoke-ScriptAnalyzer -Path $tempFile -Settings $Settings -fix
            $Content = (Get-Content -LiteralPath $tempFile -Raw).Trim()
            $Content | Should -Be '$hashSet = [System.Collections.Generic.HashSet[String]]::new([StringComparer]::InvariantCultureIgnoreCase)'
        }
    }
}