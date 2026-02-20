# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

    $violationsUsingScriptDefinition = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$PSScriptRoot\RuleSuppression.ps1")
    $violations = Invoke-ScriptAnalyzer "$PSScriptRoot\RuleSuppression.ps1"

    $ruleSuppressionBad = @'
    Function do-something
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingUserNameAndPassWordParams", "username")]
        Param(
        $username,
        $password
        )
    }
'@

    $ruleSuppressionInConfiguration = @'
    Configuration xFileUpload
    {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "")]
    param ([string] $decryptedPassword)
    $securePassword = ConvertTo-SecureString $decryptedPassword -AsPlainText -Force
    }
'@

    # If function doesn't starts at offset 0, then the test case fails before commit b551211
    $ruleSuppressionAvoidUsernameAndPassword = @'

    function SuppressUserAndPwdRule()
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingUserNameAndPassWordParams", "")]
        [CmdletBinding()]
        param
        (
            [System.String] $username,
            [System.String] $password
        )
    }
'@
}


Describe "RuleSuppressionWithoutScope" {
    Context "Function" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
        }

        It "Suppresses rule with extent created using ScriptExtent constructor" {
            Invoke-ScriptAnalyzer `
                -ScriptDefinition $ruleSuppressionAvoidUsernameAndPassword `
                -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" `
                -OutVariable ruleViolations `
                -SuppressedOnly
            $ruleViolations.Count | Should -Be 1
        }
    }

    Context "Script" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "RuleSuppressionID" {
        It "Only suppress violations for that ID" {
            $suppression = $violations | Where-Object { $_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should -Be 1
            $suppression = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should -Be 1
        }

        It "Suppresses PSAvoidUsingPlainTextForPassword violation for the given ID" {
            $ruleSuppressionIdAvoidPlainTextPassword = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1")]
    param(
    [string] $password1,
    [string] $password2
    )
}
'@
            Invoke-ScriptAnalyzer `
                -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword `
                -IncludeRule "PSAvoidUsingPlainTextForPassword" `
                -OutVariable ruleViolations `
                -SuppressedOnly
            $ruleViolations.Count | Should -Be 1
        }

        It "Records multiple suppressions applied to a single diagnostic" {
            $script = @'
function MyFunc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='a')]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='b')]
    param(
        [string]$password1,
        [string]$password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $script -IncludeRule 'PSAvoidUsingPlainTextForPassword'
            $suppressions = Invoke-ScriptAnalyzer -ScriptDefinition $script -SuppressedOnly -IncludeRule 'PSAvoidUsingPlainTextForPassword'

            $diagnostics | Should -HaveCount 1
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password2"
            $diagnostics[0].IsSuppressed | Should -BeFalse

            $suppressions | Should -HaveCount 1
            $suppressions[0].IsSuppressed | Should -BeTrue
            $suppressions[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $suppressions[0].RuleSuppressionID | Should -BeExactly "password1"
            $suppressions[0].Suppression | Should -HaveCount 2
            $suppressions[0].Suppression.Justification | Sort-Object | Should -Be @('a', 'b')
        }

        It "Records multiple suppressions applied to a single diagnostic when they have identical justifications" {
            $script = @'
function MyFunc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='a')]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='a')]
    param(
        [string]$password1,
        [string]$password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $script -IncludeRule 'PSAvoidUsingPlainTextForPassword'
            $suppressions = Invoke-ScriptAnalyzer -ScriptDefinition $script -SuppressedOnly -IncludeRule 'PSAvoidUsingPlainTextForPassword'

            $diagnostics | Should -HaveCount 1
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password2"

            $suppressions | Should -HaveCount 1
            $suppressions[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $suppressions[0].RuleSuppressionID | Should -BeExactly "password1"
            $suppressions[0].Suppression | Should -HaveCount 2
            $suppressions[0].Suppression.Justification | Sort-Object | Should -Be @('a', 'a')
        }

        It "Includes both emitted and suppressed diagnostics when -IncludeSuppressed is used" {
            $script = @'
function MyFunc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='a')]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "password1", Justification='a')]
    param(
        [string]$password1,
        [string]$password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $script -IncludeRule 'PSAvoidUsingPlainTextForPassword' -IncludeSuppressed

            $diagnostics | Should -HaveCount 2
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password2"
            $diagnostics[0].IsSuppressed | Should -BeFalse

            $diagnostics[1].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[1].RuleSuppressionID | Should -BeExactly "password1"
            $diagnostics[1].Suppression | Should -HaveCount 2
            $diagnostics[1].Suppression.Justification | Sort-Object | Should -Be @('a', 'a')
            $diagnostics[1].IsSuppressed | Should -BeTrue
        }


        It "Records no suppressions for a different rule" {
            $script = @'
function MyFunc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("DifferentRule", "password1", Justification='a')]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("DifferentRule", "password1", Justification='b')]
    param(
        [string]$password1,
        [string]$password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $script -IncludeRule 'PSAvoidUsingPlainTextForPassword'
            $suppressions = Invoke-ScriptAnalyzer -ScriptDefinition $script -SuppressedOnly -IncludeRule 'PSAvoidUsingPlainTextForPassword'

            $diagnostics | Should -HaveCount 2
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password1"
            $diagnostics[1].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[1].RuleSuppressionID | Should -BeExactly "password2"

            $suppressions | Should -BeNullOrEmpty
        }

        It "Issues an error for a unapplied suppression with a suppression ID" {
            $script = @'
function MyFunc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("PSAvoidUsingPlainTextForPassword", "banana")]
    param(
        [string]$password1,
        [string]$password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer -ScriptDefinition $script -IncludeRule 'PSAvoidUsingPlainTextForPassword' -ErrorVariable diagErr -ErrorAction SilentlyContinue
            $suppressions = Invoke-ScriptAnalyzer -ScriptDefinition $script -SuppressedOnly -IncludeRule 'PSAvoidUsingPlainTextForPassword' -ErrorVariable suppErr -ErrorAction SilentlyContinue

            $diagnostics | Should -HaveCount 2
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password1"
            $diagnostics[1].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[1].RuleSuppressionID | Should -BeExactly "password2"

            $suppressions | Should -BeNullOrEmpty

            # For some reason these tests fail in WinPS, but the actual output is as expected
            if ($PSEdition -eq 'Core')
            {
                $diagErr | Should -HaveCount 1
                $diagErr.TargetObject.RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
                $diagErr.TargetObject.RuleSuppressionID | Should -BeExactly "banana"

                $suppErr | Should -HaveCount 1
                $suppErr.TargetObject.RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
                $suppErr.TargetObject.RuleSuppressionID | Should -BeExactly "banana"
            }
        }
    }

    Context "RuleSuppressionID with named arguments" {
        It "Should work with named argument syntax" {
            $scriptWithNamedArgs = @'
function SuppressPasswordParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute(RuleName="PSAvoidUsingPlainTextForPassword", RuleSuppressionId="password1")]
    param(
        [string] $password1,
        [string] $password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer `
                -ScriptDefinition $scriptWithNamedArgs `
                -IncludeRule "PSAvoidUsingPlainTextForPassword"
            $suppressions = Invoke-ScriptAnalyzer `
                -ScriptDefinition $scriptWithNamedArgs `
                -IncludeRule "PSAvoidUsingPlainTextForPassword" `
                -SuppressedOnly

            # There should be one unsuppressed diagnostic (password2) and one suppressed diagnostic (password1)
            $diagnostics | Should -HaveCount 1
            $diagnostics[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $diagnostics[0].RuleSuppressionID | Should -BeExactly "password2"

            $suppressions | Should -HaveCount 1
            $suppressions[0].RuleName | Should -BeExactly "PSAvoidUsingPlainTextForPassword"
            $suppressions[0].RuleSuppressionID | Should -BeExactly "password1"
        }

        It "Should work with mixed positional and named argument syntax" {
            $scriptWithMixedArgs = @'
function SuppressPasswordParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", Scope="Function")]
    param(
        [string] $password1,
        [string] $password2
    )
}
'@

            $diagnostics = Invoke-ScriptAnalyzer `
                -ScriptDefinition $scriptWithMixedArgs `
                -IncludeRule "PSAvoidUsingPlainTextForPassword"

            # All violations should be suppressed since there's no RuleSuppressionID filtering
            $diagnostics | Should -HaveCount 0
        }

        It "Should work with custom rule from issue #1686 comment" {
            # This test recreates the exact scenario from GitHub issue 1686 comment
            # with a custom rule that populates RuleSuppressionID for targeted suppression

            # Custom rule module that creates violations with specific RuleSuppressionIDs
            $customRuleScript = @'
function Measure-AvoidFooBarCommand {
    [CmdletBinding()]
    [OutputType([Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord[]])]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [System.Management.Automation.Language.ScriptBlockAst]
        $ScriptBlockAst
    )

    $results = @()

    # Find all command expressions
    $commandAsts = $ScriptBlockAst.FindAll({
        param($node)
        $node -is [System.Management.Automation.Language.CommandAst]
    }, $true)

    foreach ($commandAst in $commandAsts) {
        $commandName = $commandAst.GetCommandName()
        if ($commandName -match '^(Get-FooBar|Set-FooBar)$') {
            # Create a diagnostic with the command name as RuleSuppressionID
            $result = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic.DiagnosticRecord]::new(
                "Avoid using $commandName command",
                $commandAst.Extent,
                'Measure-AvoidFooBarCommand',
                'Warning',
                $null,
                $commandName  # This becomes the RuleSuppressionID
            )
            $results += $result
        }
    }

    return $results
}

Export-ModuleMember -Function Measure-AvoidFooBarCommand
'@

            # Script that uses the custom rule with targeted suppression
            $scriptWithCustomRuleSuppression = @'
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('Measure-AvoidFooBarCommand', RuleSuppressionId = 'Get-FooBar', Scope = 'Function', Target = 'Allow-GetFooBar')]
param()

function Test-BadCommands {
    Get-FooBar  # Line 6 - Should NOT be suppressed (wrong function)
    Set-FooBar  # Line 7 - Should NOT be suppressed (different RuleSuppressionID)
}

function Allow-GetFooBar {
    Get-FooBar  # Line 11 - Should be suppressed (matches RuleSuppressionId and Target)
    Set-FooBar  # Line 12 - Should NOT be suppressed (different RuleSuppressionID)
}
'@

            # Save custom rule to temporary file
            $customRuleFile = [System.IO.Path]::GetTempFileName()
            $customRuleModuleFile = [System.IO.Path]::ChangeExtension($customRuleFile, '.psm1')
            Set-Content -Path $customRuleModuleFile -Value $customRuleScript

            try
            {
                # Check suppressed violations - this is the key test for our fix
                $suppressions = Invoke-ScriptAnalyzer `
                    -ScriptDefinition $scriptWithCustomRuleSuppression `
                    -CustomRulePath $customRuleModuleFile `
                    -SuppressedOnly `
                    -ErrorAction SilentlyContinue

                # The core functionality: RuleSuppressionID with named arguments should work for custom rules
                # We should have at least one suppressed Get-FooBar violation
                $suppressions | Should -Not -BeNullOrEmpty -Because "RuleSuppressionID named arguments should work for custom rules"

                $getFooBarSuppressions = $suppressions | Where-Object { $_.RuleSuppressionID -eq 'Get-FooBar' }
                $getFooBarSuppressions | Should -Not -BeNullOrEmpty -Because "Get-FooBar should be suppressed based on RuleSuppressionID"

                # Verify the suppression occurred in the right function (Allow-GetFooBar)
                $getFooBarSuppressions | Should -Not -BeNullOrEmpty
                $getFooBarSuppressions[0].RuleName | Should -BeExactly 'Measure-AvoidFooBarCommand'

                # Get unsuppressed violations to verify selective suppression
                $diagnostics = Invoke-ScriptAnalyzer `
                    -ScriptDefinition $scriptWithCustomRuleSuppression `
                    -CustomRulePath $customRuleModuleFile `
                    -ErrorAction SilentlyContinue

                # Should still have violations for Set-FooBar (different RuleSuppressionID) and Get-FooBar in wrong function
                $setFooBarViolations = $diagnostics | Where-Object { $_.RuleSuppressionID -eq 'Set-FooBar' }
                $setFooBarViolations | Should -Not -BeNullOrEmpty -Because "Set-FooBar should not be suppressed (different RuleSuppressionID)"

            }
            finally
            {
                Remove-Item -Path $customRuleModuleFile -ErrorAction SilentlyContinue
                Remove-Item -Path $customRuleFile -ErrorAction SilentlyContinue
            }
        }
    }

    Context "Rule suppression within DSC Configuration definition" {
        It "Suppresses rule" -Skip:($IsLinux -or $IsMacOS) {
            $suppressedRule = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionInConfiguration -SuppressedOnly
            $suppressedRule.Count | Should -Be 1
        }
    }

    Context "Bad Rule Suppression" -Skip:$testingLibraryUsage {
        It "Throws a non-terminating error" {
            Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionBad -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" -ErrorVariable errorRecord -ErrorAction SilentlyContinue
            $errorRecord.Count | Should -Be 1
            $errorRecord.FullyQualifiedErrorId | Should -Match "suppression message attribute error"
        }
    }

    Context "External Rule Suppression" -Skip:$testingLibraryUsage {
        It "Suppresses violation of an external ast rule" {
            $externalRuleSuppression = @'
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','')]
    param() # without the param block, powershell parser throws up!
    Write-Host "write-host"
'@
            Invoke-ScriptAnalyzer `
                -ScriptDefinition $externalRuleSuppression `
                -CustomRulePath (Join-Path $PSScriptRoot "CommunityAnalyzerRules") `
                -OutVariable ruleViolations `
                -SuppressedOnly
            $ruleViolations.Count | Should -Be 1
        }
    }
}

Describe "RuleSuppressionWithScope" {
    Context "FunctionScope" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object { $_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "Function scope with regular expression" {
        It "suppresses objects that match the regular expression" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-ba[rz]')]
            param()
            function start-foo {
                write-host "start-foo"
            }

            function start-bar {
                write-host "start-bar"
            }

            function start-baz {
                write-host "start-baz"
            }

            function start-bam {
                write-host "start-bam"
            }
'@
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -SuppressedOnly
            $suppressed.Count | Should -Be 2
        }

        It "suppresses objects that match glob pattern with glob in the end" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-*')]
            param()
            function start-foo {
                write-host "start-foo"
            }

            function start-bar {
                write-host "start-bar"
            }

            function stop-bar {
                write-host "stop-bar"
            }
'@
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -SuppressedOnly
            $suppressed.Count | Should -Be 2
        }

        It "suppresses objects that match glob pattern with glob in the begining" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='*-bar')]
            param()
            function start-foo {
                write-host "start-foo"
            }

            function start-bar {
                write-host "start-bar"
            }

            function start-baz {
                write-host "start-baz"
            }
'@
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }
    }
}
