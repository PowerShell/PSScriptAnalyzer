# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $violationMessage = "is not permitted in Constrained Language Mode"
    $violationName = "PSUseConstrainedLanguageMode"
    $ruleName = $violationName
    
    # The rule is disabled by default, so we need to enable it
    $settings = @{
        IncludeRules = @($ruleName)
        Rules = @{
            $ruleName = @{
                Enable = $true
            }
        }
    }
}

Describe "UseConstrainedLanguageMode" {
    Context "When Add-Type is used" {
        It "Should detect Add-Type usage" {
            $def = @'
Add-Type -TypeDefinition @"
    public class TestType {
        public static string Test() { return "test"; }
    }
"@
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $violations[0].RuleName | Should -Be $violationName
            $violations[0].Message | Should -BeLike "*Add-Type*$violationMessage*"
        }

        It "Should not flag other commands" {
            $def = 'Get-Process | Where-Object { $_.Name -eq "powershell" }'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
    }

    Context "When New-Object with COM is used" {
        It "Should detect New-Object -ComObject usage" {
            $def = 'New-Object -ComObject "Scripting.FileSystemObject"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*COM object*$violationMessage*"
        }

        It "Should not flag New-Object without -ComObject" {
            $def = 'New-Object -TypeName System.Collections.ArrayList'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations | Where-Object { $_.RuleName -eq $violationName } | Should -BeNullOrEmpty
        }
    }

    Context "When XAML is used" {
        It "Should detect XAML usage" {
            $def = @'
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <Button>Click me</Button>
</Window>
"@
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*XAML*$violationMessage*"
        }
    }

    Context "When Invoke-Expression is used" {
        It "Should detect Invoke-Expression usage" {
            $def = 'Invoke-Expression "Get-Process"'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations.Count | Should -Be 1
            $matchingViolations[0].Message | Should -BeLike "*Invoke-Expression*$violationMessage*"
        }
    }

    Context "Informational severity" {
        It "Should have Information severity" {
            $def = 'Add-Type -AssemblyName System.Windows.Forms'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $matchingViolations = $violations | Where-Object { $_.RuleName -eq $violationName }
            $matchingViolations[0].Severity | Should -Be 'Information'
        }
    }
}
