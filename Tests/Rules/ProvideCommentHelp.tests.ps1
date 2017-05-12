$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module PSScriptAnalyzer
Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$violationMessage = "The cmdlet 'Comment' does not have a help comment."
$violationName = "PSProvideCommentHelp"
$violations = Invoke-ScriptAnalyzer $directory\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
    $dscViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $directory\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
}

$noViolations = Invoke-ScriptAnalyzer $directory\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

function Test-Correction {
    param($scriptDef, $expectedCorrection)

    $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -IncludeRule $violationName
    $violations.Count | Should Be 1
    $violations[0].SuggestedCorrections[0].Text | Should Be $expectedCorrection
}

Describe "ProvideCommentHelp" {
    Context "When there are violations" {
        It "has 2 provide comment help violations" {
            $violations.Count | Should Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should Match $violationMessage
        }

        It "has extent that includes only the function name" {
            $violations[1].Extent.Text | Should Be "Comment"
        }

        It "should return a help snippet correction with 0 parameters" {
            $def = @'
function foo {
}

Export-ModuleMember -Function foo
'@
            $expectedCorrection = @'
<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.EXAMPLE
An example

.NOTES
General notes
#>

'@
            Test-Correction $def $expectedCorrection
        }

        It "should return a help snippet correction with 1 parameters" {
            $def = @'
function foo {
    param($param1)
}

Export-ModuleMember -Function foo
'@
            $expectedCorrection = @'
<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.PARAMETER param1
Parameter description

.EXAMPLE
An example

.NOTES
General notes
#>

'@
            Test-Correction $def $expectedCorrection
        }

        It "should return a help snippet correction with 2 parameters" {
            $def = @'
function foo {
    param($param1, $param2)
}

Export-ModuleMember -Function foo
'@
            $expectedCorrection = @'
<#
.SYNOPSIS
Short description

.DESCRIPTION
Long description

.PARAMETER param1
Parameter description

.PARAMETER param2
Parameter description

.EXAMPLE
An example

.NOTES
General notes
#>

'@
            Test-Correction $def $expectedCorrection
        }

        if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
            It "Does not count violation in DSC class" {
                $dscViolations.Count | Should Be 0
            }
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should Be 0
        }
    }
}