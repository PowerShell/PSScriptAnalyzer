# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $nounViolationMessage = "The cmdlet 'Verb-Files' uses a plural noun. A singular noun should be used instead."
    $verbViolationMessage = "The cmdlet 'Verb-Files' uses an unapproved verb."
    $nounViolationName = "PSUseSingularNouns"
    $verbViolationName = "PSUseApprovedVerbs"
    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\BadCmdlet.ps1
    $nounViolations = $violations | Where-Object {$_.RuleName -eq $nounViolationName}
    $verbViolations = $violations | Where-Object {$_.RuleName -eq $verbViolationName}
    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1
    $nounNoViolations = $noViolations | Where-Object {$_.RuleName -eq $nounViolationName}
    $verbNoViolations = $noViolations | Where-Object {$_.RuleName -eq $verbViolationName}
}

# UseSingularNouns rule doesn't exist in the non desktop version of PSScriptAnalyzer due to missing .Net Pluralization API
Describe "UseSingularNouns" {
    Context "When there are violations" {
        It "has a cmdlet singular noun violation" {
            $nounViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $nounViolations[0].Message | Should -Match $nounViolationMessage
        }

        It "has the correct extent" {
        $nounViolations[0].Extent.Text | Should -Be "Verb-Files"
        }
    }

    Context "When function names have nouns from allowlist" {

        It "ignores function name ending with Data by default" {
            $nounViolationScript = @'
Function Add-SomeData
{
Write-Output "Adding some data"
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $nounViolationScript `
                -IncludeRule "PSUseSingularNouns" `
                -OutVariable violations
            $violations.Count | Should -Be 0
        }

        It "ignores function name ending with Windows by default" {
            $nounViolationScript = @'
Function Test-Windows
{
Write-Output "Testing Microsoft Windows"
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $nounViolationScript `
                -IncludeRule "PSUseSingularNouns" `
                -OutVariable violations
            $violations.Count | Should -Be 0
        }

        It "ignores function names defined in settings" {
            $nounViolationScript = @'
Function Get-Bananas
{
Write-Output "Bananas"
}
'@
            Invoke-ScriptAnalyzer -ScriptDefinition $nounViolationScript -Settings @{
                IncludeRules = @("PSUseSingularNouns")
                Rules        = @{ PSUseSingularNouns = @{ NounAllowList = "Bananas" } }
            } | Should -BeNullOrEmpty
        }

    }

    Context "When there are no violations" {
        It "returns no violations" {
            $nounNoViolations.Count | Should -Be 0
        }
    }

    Context "Inline tests" {
        It 'Correctly diagnoses and corrects <Script>' -TestCases @(
            @{ Script = 'function Get-Bananas { "bananas" }'; Extent = @{ StartCol = 10; EndCol = 21 }; Correction = 'Get-Banana' }
            @{ Script = 'function ConvertTo-StartingCriteria { "criteria" }'; Extent = @{ StartCol = 10; EndCol = 36 }; Correction = 'ConvertTo-StartingCriterion' }
            @{ Script = 'function Invoke-Data { "data" }' }
            @{ Script = 'function Get-Banana { "bananas" }' }
            @{ Script = 'function get-banana { "bananas" }' }
            @{ Script = 'function banana { "bananas" }' }
        ) {
            param([string]$Script, $Extent, $Correction)

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

            $diagnostics.SuggestedCorrections.StartLineNumber | Should -BeExactly $expectedStartLine
            $diagnostics.SuggestedCorrections.EndLineNumber | Should -BeExactly $expectedEndLine
            $diagnostics.SuggestedCorrections.StartColumnNumber | Should -BeExactly $Extent.StartCol
            $diagnostics.SuggestedCorrections.EndColumnNumber | Should -BeExactly $Extent.EndCol
            $diagnostics.SuggestedCorrections.Text | Should -BeExactly $Correction
        }
    }
    Context 'Suppression' {
        It 'Can be suppressed by RuleSuppressionId' {
            $scriptDef = @"
function Get-Elements {
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('$nounViolationName', 'Get-Elements')]
    param()
}
"@
            $warnings = @(Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef)
            $warnings.Count | Should -Be 0
        }
    }
}

Describe "UseApprovedVerbs" {
    Context "When there are violations" {
        It "has an approved verb violation" {
            $verbViolations.Count | Should -Be 1
        }

        It "has the correct description message" {
            $verbViolations[0].Message | Should -Match $verbViolationMessage
        }

        It "has the correct extent" {
                $verbViolations[0].Extent.Text | Should -Be "Verb-Files"
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $verbNoViolations.Count | Should -Be 0
        }
    }
}
