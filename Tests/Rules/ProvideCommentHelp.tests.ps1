# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

    $violationMessage = "The cmdlet 'Comment' does not have a help comment."
    $violationName = "PSProvideCommentHelp"
    $ruleSettings = @{
        Enable = $true
        ExportedOnly = $false
        BlockComment = $true
        Placement = "before"
        VSCodeSnippetCorrection = $false
    }

    $settings = @{
        IncludeRules = @("PSProvideCommentHelp")
        Rules = @{ PSProvideCommentHelp = $ruleSettings }
    }

    $violations = Invoke-ScriptAnalyzer $PSScriptRoot\BadCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

    if ($PSVersionTable.PSVersion -ge [Version]'5.0.0') {
        $dscViolations = Invoke-ScriptAnalyzer -ErrorAction SilentlyContinue $PSScriptRoot\DSCResourceModule\DSCResources\MyDscResource\MyDscResource.psm1 | Where-Object {$_.RuleName -eq $violationName}
    }

    $noViolations = Invoke-ScriptAnalyzer $PSScriptRoot\GoodCmdlet.ps1 | Where-Object {$_.RuleName -eq $violationName}

    function Test-Correction {
        param($scriptDef, $expectedCorrection, $settings)

        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDef -Settings $settings
        $violations.Count | Should -Be 1

        # We split the lines because appveyor checks out files with "\n" endings
        # on windows, which results in inconsistent line endings between corrections
        # and result.
        $resultLines = $violations[0].SuggestedCorrections[0].Text -split "\r?\n"
        $expectedLines = $expectedCorrection -split "\r?\n"
        $resultLines.Count | Should -Be $expectedLines.Count
        for ($i = 0; $i -lt $resultLines.Count; $i++) {
            $resultLine = $resultLines[$i]
            $expectedLine = $expectedLines[$i]
            $resultLine | Should -Be $expectedLine
        }
    }
}

Describe "ProvideCommentHelp" {
    Context "When there are violations" {
        It "has 2 provide comment help violations" {
            $violations.Count | Should -Be 2
        }

        It "has the correct description message" {
            $violations[1].Message | Should -Match $violationMessage
        }

        It "has extent that includes only the function name" {
            $violations[1].Extent.Text | Should -Be "Comment"
        }

        It "should find violations in functions that are not exported" {
            $def = @'
function foo {
}
'@

            Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings | Get-Count | Should -Be 1
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
            Test-Correction $def $expectedCorrection $settings
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
            Test-Correction $def $expectedCorrection $settings
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
            Test-Correction $def $expectedCorrection $settings
        }

        It "should return a help snippet correction with line comment style" {
            $def = @'
function foo {
    param($param1, $param2)
}

Export-ModuleMember -Function foo
'@
            $expectedCorrection = @'
##############################
#.SYNOPSIS
#Short description
#
#.DESCRIPTION
#Long description
#
#.PARAMETER param1
#Parameter description
#
#.PARAMETER param2
#Parameter description
#
#.EXAMPLE
#An example
#
#.NOTES
#General notes
##############################

'@
            $ruleSettings.'BlockComment' = $false
            Test-Correction $def $expectedCorrection $settings
        }

        It "should return a vscode snippet with line comment style" {
            $def = @'
function foo {
    param($param1, $param2)
}

Export-ModuleMember -Function foo
'@
            $expectedCorrection = @'
##############################
#.SYNOPSIS
#${1:Short description}
#
#.DESCRIPTION
#${2:Long description}
#
#.PARAMETER param1
#${3:Parameter description}
#
#.PARAMETER param2
#${4:Parameter description}
#
#.EXAMPLE
#${5:An example}
#
#.NOTES
#${6:General notes}
##############################

'@
            $ruleSettings.'BlockComment' = $false
            $ruleSettings.'VSCodeSnippetCorrection' = $true
            Test-Correction $def $expectedCorrection $settings
        }

        It "should return a help snippet correction with 2 parameters at the begining of function body" {
            $def = @'
function foo {
    param($param1, $param2)
}
'@
            $expectedCorrection = @'
{
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
            $ruleSettings.'ExportedOnly' = $false
            $ruleSettings.'BlockComment' = $true
            $ruleSettings.'VSCodeSnippetCorrection' = $false
            $ruleSettings.'Placement' = 'begin'
            Test-Correction $def $expectedCorrection $settings
        }

        It "should return a help snippet correction with 2 parameters at the end of function body" {
            $def = @'
function foo {
    param($param1, $param2)
}
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
            $ruleSettings.'ExportedOnly' = $false
            $ruleSettings.'BlockComment' = $true
            $ruleSettings.'VSCodeSnippetCorrection' = $false
            $ruleSettings.'Placement' = 'end'
            Test-Correction $def $expectedCorrection $settings
        }

        It "should return a help snippet correction with correct indentation" {
            $def = @'
    function foo {
        param($param1)
    }
'@
            $s = ' '
            $expectedCorrection = @"
    <#
    .SYNOPSIS
    Short description
$s$s$s$s
    .DESCRIPTION
    Long description
$s$s$s$s
    .PARAMETER param1
    Parameter description
$s$s$s$s
    .EXAMPLE
    An example
$s$s$s$s
    .NOTES
    General notes
    #>

"@
            $ruleSettings.'ExportedOnly' = $false
            $ruleSettings.'BlockComment' = $true
            $ruleSettings.'VSCodeSnippetCorrection' = $false
            $ruleSettings.'Placement' = 'before'
            Test-Correction $def $expectedCorrection $settings
        }


        It "Does not count violation in DSC class" -Skip:($PSVersionTable.PSVersion -lt '5.0') {
            $dscViolations.Count | Should -Be 0
        }
    }

    Context "When there are no violations" {
        It "returns no violations" {
            $noViolations.Count | Should -Be 0
        }
    }
}
