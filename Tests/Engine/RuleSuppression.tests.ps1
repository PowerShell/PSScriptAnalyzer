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
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionAvoidUsernameAndPassword -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }

        It "All PSAvoidUsingUserNameAndPassWordParams violations with extent created using ScriptExtent constructor" {
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionAvoidUsernameAndPassword -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" -IncludeSuppressions
            $allrecords.Count | Should -Be 1
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
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword -IncludeRule "PSAvoidUsingPlainTextForPassword" -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }

        It "All PSAvoidUsingPlainTextForPassword violations including suppresses violation for the given ID" {
            $ruleSuppressionIdAvoidPlainTextPassword1 = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1")]
    param(
    [string] $password1
    )
}
'@
            $allrecords1 = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword1 -IncludeRule "PSAvoidUsingPlainTextForPassword" -IncludeSuppressions
            $allrecords1.Count | Should -Be 1

            $ruleSuppressionIdAvoidPlainTextPassword2 = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1")]
    param(
    [string] $password1,
    [string] $password2
    )
}
'@
            $allrecords2 = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword2 -IncludeRule "PSAvoidUsingPlainTextForPassword" -IncludeSuppressions
            $allrecords2.Count | Should -Be 2
        }

        It "PSAvoidUsingPlainTextForPassword violation for the given ID with sereval suppressions" {
            $ruleSuppressionIdAvoidPlainTextPassword = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="a")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="b")]
    param(
    [string] $password1,
    [string] $password2
    )
}
'@
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword -IncludeRule "PSAvoidUsingPlainTextForPassword" -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }

        It "All PSAvoidUsingPlainTextForPassword violations including those for the given ID and suppressed several times" {
            $ruleSuppressionIdAvoidPlainTextPassword1 = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="a")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="b")]
    param(
    [string] $password1
    )
}
'@
            $allrecords1 = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword1 -IncludeRule "PSAvoidUsingPlainTextForPassword" -IncludeSuppressions
            $allrecords1.Count | Should -Be 1

             $ruleSuppressionIdAvoidPlainTextPassword2 = @'
function SuppressPwdParam()
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="a")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingPlainTextForPassword", "password1",Justification="b")]
    param(
    [string] $password1,
    [string] $password2
    )
}
'@
            $allrecords2 = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionIdAvoidPlainTextPassword2 -IncludeRule "PSAvoidUsingPlainTextForPassword" -IncludeSuppressions
            $allrecords2.Count | Should -Be 2
        }
    }

    Context "Rule suppression within DSC Configuration definition" {
        It "Suppresses rule" -skip:($IsLinux -or $IsMacOS -or ($PSVersionTable.PSVersion.Major -lt 5)) {
            $suppressedRule = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionInConfiguration -SuppressedOnly
            $suppressedRule.Count | Should -Be 1
        }

        It "All rule violations" -skip:($IsLinux -or $IsMacOS -or ($PSVersionTable.PSVersion.Major -lt 5)) {
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionInConfiguration -IncludeSuppressions
            $allrecords.Count | Should -Be 3
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
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $externalRuleSuppression -CustomRulePath (Join-Path $PSScriptRoot "CommunityAnalyzerRules") -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }

        It "Violations of an external ast rule including suppresses violation" {
            $externalRuleSuppression = @'
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','')]
    param() # without the param block, powershell parser throws up!
    Write-Host "write-host"
'@
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $externalRuleSuppression -CustomRulePath (Join-Path $PSScriptRoot "CommunityAnalyzerRules") -IncludeSuppressions
            $allrecords.Count | Should -Be 1
        }

        It "Violation of an external ast rule with sereval suppressions" {
            $externalRuleSuppression = @'
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','',Justification="a")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','',Justification="b")]
    param() # without the param block, powershell parser throws up!
    Write-Host "write-host"
'@
            $suppressed = Invoke-ScriptAnalyzer -ScriptDefinition $externalRuleSuppression -CustomRulePath (Join-Path $PSScriptRoot "CommunityAnalyzerRules") -SuppressedOnly
            $suppressed.Count | Should -Be 1
        }

        It "Violations of an external ast rule including violation suppressed sereval times" {
            $externalRuleSuppression = @'
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','',Justification="a")]
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','',Justification="b")]
    param() # without the param block, powershell parser throws up!
    Write-Host "write-host"
'@
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $externalRuleSuppression -CustomRulePath (Join-Path $PSScriptRoot "CommunityAnalyzerRules") -IncludeSuppressions
            $allrecords.Count | Should -Be 1
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
        It "all objects violating PSAvoidUsingWriteHost including suppresses objects that match the regular expression" {
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 4
        }

        It "objects that match the regular expression with several suppressions" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-ba[rz]',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-bar',Justification="b")]
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

        It "all objects violating PSAvoidUsingWriteHost including those that match the regular expression and suppressed several times" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-ba[rz]',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-bar',Justification="b")]
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 4
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

        It "all objects violating PSAvoidUsingWriteHost including suppresses objects that match glob pattern with glob in the end" {
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 3
        }

        It "objects that match glob pattern with glob in the end and suppressed several times" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-*',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-*',Justification="b")]
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

        It "all objects violating PSAvoidUsingWriteHost including those that match glob pattern with glob in the end and suppressed several times" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-*',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='start-*',Justification="b")]
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 3
        }

        It "suppresses objects that match glob pattern with glob in the beginning" {
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

        It "all objects violating PSAvoidUsingWriteHost including suppresses objects that match glob pattern with glob in the beginning" {
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 3
        }

        It "objects that match glob pattern with glob in the beginning and suppressed several times" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='*-bar',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='*-bar',Justification="b")]
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

        It "all objects violating PSAvoidUsingWriteHost including those that match glob pattern with glob in the beginning and suppressed several times" {
            $scriptDef = @'
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='*-bar',Justification="a")]
            [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '', Scope='Function', Target='*-bar',Justification="b")]
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
            $allrecords = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule 'PSAvoidUsingWriteHost' -IncludeSuppressions
            $allrecords.Count | Should -Be 3
        }
    }
 }
