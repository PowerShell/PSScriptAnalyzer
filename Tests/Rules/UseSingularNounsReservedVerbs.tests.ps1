Import-Module PSScriptAnalyzer
$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory
Import-Module (Join-Path $testRootDirectory 'PSScriptAnalyzerTestHelper.psm1')

$nounViolationMessage = "The cmdlet 'Verb-Files' uses a plural noun. A singular noun should be used instead."
$verbViolationMessage = "The cmdlet 'Verb-Files' uses an unapproved verb."
$nounViolationName = "PSUseSingularNouns"
$verbViolationName = "PSUseApprovedVerbs"
$violations = Invoke-ScriptAnalyzer $directory\BadCmdlet.ps1
$nounViolations = $violations | Where-Object {$_.RuleName -eq $nounViolationName}
$verbViolations = $violations | Where-Object {$_.RuleName -eq $verbViolationName}
$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1
$nounNoViolations = $noViolations | Where-Object {$_.RuleName -eq $nounViolationName}
$verbNoViolations = $noViolations | Where-Object {$_.RuleName -eq $verbViolationName}

# this rule doesn't exist in the non desktop version of PSScriptAnalyzer
if (-not (Test-PSEditionCoreCLR))
{
    Describe "UseSingularNouns" {
        Context "When there are violations" {
            It "has a cmdlet singular noun violation" {
                $nounViolations.Count | Should Be 1
            }

            It "has the correct description message" {
                $nounViolations[0].Message | Should Match $nounViolationMessage
            }

            It "has the correct extent" {
            $nounViolations[0].Extent.Text | Should be "Verb-Files"
            }
        }

        Context "When function names have nouns from whitelist" {

            It "ignores function name ending with Data" {
                $nounViolationScript = @'
Function Add-SomeData
{
    Write-Output "Adding some data"
}
'@
                Invoke-ScriptAnalyzer -ScriptDefinition $nounViolationScript `
                    -IncludeRule "PSUseSingularNouns" `
                    -OutVariable violations
                $violations.Count | Should Be 0
            }
        }

        Context "When there are no violations" {
            It "returns no violations" {
                $nounNoViolations.Count | Should Be 0
            }
        }
    }
}

Describe "UseApprovedVerbs" {
    Context "When there are violations" {
        It "has an approved verb violation" {
            $verbViolations.Count | Should Be 1
        }

        It "has the correct description message" {
            $verbViolations[0].Message | Should Match $verbViolationMessage
        }

        It "has the correct extent" {
                $verbViolations[0].Extent.Text | Should be "Verb-Files"
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $verbNoViolations.Count | Should Be 0
        }
    }
}