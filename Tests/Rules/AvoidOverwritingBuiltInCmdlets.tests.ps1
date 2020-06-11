# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $ruleName = "PSAvoidOverwritingBuiltInCmdlets"

    $ruleSettingsWindows = @{$ruleName = @{PowerShellVersion = @('desktop-5.1.14393.206-windows') } }
    $ruleSettingsCore = @{$ruleName = @{PowerShellVersion = @('core-6.1.0-windows') } }
    $ruleSettingsBoth = @{$ruleName = @{PowerShellVersion = @('core-6.1.0-windows', 'desktop-5.1.14393.206-windows') } }

    $settings = @{
        IncludeRules = @($ruleName)
    }

    # Get-Something is not a built in cmdlet on any platform and should never be flagged
    # Get-ChildItem is available on all versions of PowerShell and should always be flagged
    # Get-Clipboard is available on PowerShell 5 but not 6 and should be flagged conditionally
    $scriptDefinition = @"
    function Get-Something {
        Write-Output "Get-Something"
    }

    function Get-ChildItem {
        Write-Output "Get-ChildItem"
    }

    function Get-Clipboard {
        Write-Output "Get-Clipboard"
    }
"@
}

describe 'AvoidOverwritingBuiltInCmdlets' {
    context 'No settings specified' {
        it 'should default to core-6.1.0-windows if running PS 6+ and desktop-5.1.14393.206-windows if it is not' {
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings

            if ($PSVersionTable.PSVersion.Major -gt 5) {
                $violations.Count | Should -Be 1
                $violations.Extent.StartLineNumber | Should -Be 5
            }

            else {
                $violations.Count | Should -Be 2
                $violations[1].Extent.StartLineNumber | Should -Be 9
            }
        }
    }

    context 'PowerShellVersion explicitly set to Windows PowerShell' {
        BeforeAll {
            $settings['Rules'] = $ruleSettingsWindows
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
        }

        it 'should find two violations' {
            $violations.Count | Should -Be 2
        }
        it 'should find the violations on the correct line' {
            $violations[0].Extent.StartLineNumber | Should -Be 5
            $violations[0].Extent.EndLineNumber | Should -Be 7

            $violations[1].Extent.StartLineNumber | Should -Be 9
            $violations[1].Extent.EndLineNumber | Should -Be 11
        }
    }

    context 'PowerShellVersion explicitly set to PowerShell 6' {
        BeforeAll {
            $settings['Rules'] = $ruleSettingsCore
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
        }

        it 'should find one violation' {
            $violations.Count | Should -Be 1
        }
        it 'should find the correct violating function' {
            $violations.Extent.StartLineNumber | Should -Be 5
            $violations.Extent.EndLineNumber | Should -Be 7
        }
    }

    context 'PowerShellVersion explicitly set to both Windows PowerShell and PowerShell 6' {
        BeforeAll {
            $settings['Rules'] = $ruleSettingsBoth
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
        }

        it 'should find three violations' {
            $violations.Count | Should -Be 3
        }
        it 'should find the correct violating functions' {
            $violations[0].Extent.StartLineNumber | Should -Be 5
            $violations[0].Extent.EndLineNumber | Should -Be 7

            $violations[1].Extent.StartLineNumber | Should -Be 5
            $violations[1].Extent.EndLineNumber | Should -Be 7

            $violations[2].Extent.StartLineNumber | Should -Be 9
            $violations[2].Extent.EndLineNumber | Should -Be 11

        }
    }

}
