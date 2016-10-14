# Check if PSScriptAnalyzer is already loaded so we don't
# overwrite a test version of Invoke-ScriptAnalyzer by
# accident
if (!(Get-Module PSScriptAnalyzer) -and !$testingLibraryUsage)
{
	Import-Module -Verbose PSScriptAnalyzer
}

$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$violationsUsingScriptDefinition = Invoke-ScriptAnalyzer -ScriptDefinition (Get-Content -Raw "$directory\RuleSuppression.ps1")
$violations = Invoke-ScriptAnalyzer "$directory\RuleSuppression.ps1"

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

Describe "RuleSuppressionWithoutScope" {
    Context "Function" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object { $_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should Be 0
        }

        It "Suppresses rule with extent created using ScriptExtent constructor" {
            Invoke-ScriptAnalyzer `
              -ScriptDefinition $ruleSuppressionAvoidUsernameAndPassword `
              -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" `
              -OutVariable ruleViolations `
              -SuppressedOnly
            $ruleViolations.Count | Should Be 1
	    }

    }

    Context "Script" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSProvideCommentHelp" }
            $suppression.Count | Should Be 0
        }
    }

    Context "RuleSuppressionID" {
        It "Only suppress violations for that ID" {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should Be 1
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidDefaultValueForMandatoryParameter" }
            $suppression.Count | Should Be 1
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
            $ruleViolations.Count | Should Be 1
        }
    }

    Context "Rule suppression within DSC Configuration definition" {
        It "Suppresses rule" -skip:((Test-PSEditionCoreCLRLinux) -or ($PSVersionTable.PSVersion -lt [Version]'5.0.0')) {
            $suppressedRule = Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionInConfiguration -SuppressedOnly
            $suppressedRule.Count | Should Be 1
        }
    }

    if (!$testingLibraryUsage)
    {
        Context "Bad Rule Suppression" {
            It "Throws a non-terminating error" {
                Invoke-ScriptAnalyzer -ScriptDefinition $ruleSuppressionBad -IncludeRule "PSAvoidUsingUserNameAndPassWordParams" -ErrorVariable errorRecord -ErrorAction SilentlyContinue
                $errorRecord.Count | Should Be 1
                $errorRecord.FullyQualifiedErrorId | Should match "suppression message attribute error"
            }
        }

        Context "External Rule Suppression" {
            $externalRuleSuppression = @'
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('CommunityAnalyzerRules\Measure-WriteHost','')]
param() # without the param block, powershell parser throws up!
Write-Host "write-host"
'@
            It "Suppresses violation of an external ast rule" {
                Invoke-ScriptAnalyzer `
                    -ScriptDefinition $externalRuleSuppression `
                    -CustomRulePath (Join-Path $directory "CommunityAnalyzerRules") `
                    -OutVariable ruleViolations `
                    -SuppressedOnly
                $ruleViolations.Count | Should Be 1
            }
        }
    }
}

Describe "RuleSuppressionWithScope" {
    Context "FunctionScope" {
        It "Does not raise violations" {
            $suppression = $violations | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should Be 0
            $suppression = $violationsUsingScriptDefinition | Where-Object {$_.RuleName -eq "PSAvoidUsingPositionalParameters" }
            $suppression.Count | Should Be 0
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
            $suppressed.Count | Should Be 2
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
            $suppressed.Count | Should Be 2
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
            $suppressed.Count | Should Be 1
        }
    }
 }