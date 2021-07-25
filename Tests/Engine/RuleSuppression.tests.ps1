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
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should -Be 0
        }
    }

    Context "RuleSuppressionID" {
        It "Only suppress violations for that ID" {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should -Be 1
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
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

    Context "Rule suppression within DSC Configuration definition" {
        It "Suppresses rule" -skip:($IsLinux -or $IsMacOS -or ($PSVersionTable.PSVersion.Major -lt 5)) {
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
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should -Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
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
