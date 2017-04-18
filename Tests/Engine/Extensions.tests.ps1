Import-Module PSScriptAnalyzer


Describe "IScriptExtent Extension Methods" {
$extensionType = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]
           $scriptPositionType = 'System.Management.Automation.Language.ScriptPosition'
            $scriptExtentType = 'System.Management.Automation.Language.ScriptExtent'

function Get-Extent
{
                param($line, $startLineNum, $startColumnNum, $endLineNum, $endColumnNum)
                $extentStartPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $startLineNum, $startColumnNum, $line
                $extentEndPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $endLineNum, $endColumnNum, $line
                New-Object -TypeName $scriptExtentType -ArgumentList $extentStartPos, $extentEndPos
}

    Context "When a single line inner extent is compared with a single line outer extent" {
        BeforeAll {
            $outer = "This is the outer string"
             $outerExtent = Get-Extent $outer 1 1 1 ($outer.Length + 1)
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the begin boundary" {
            $innerExtent = Get-Extent $outer 1 1 1 2
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the end boundary" {
            $innerExtent = Get-Extent $outer 1 23 1 25
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return false if the inner extent is partially contained in the outer extent" {
            $innerExtent = Get-Extent $outer 1 23 1 26
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $false
        }

        It "Should return false if the inner extent is strictly not contained in the outer extent" {
            $innerExtent = Get-Extent $outer 1 25 1 27
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $false
        }
    }

    Context "When a multi-line inner extent is compared with a multi-line outer extent" {
        BeforeAll {
            $outer = @"
This is an outer string
that spans more than
two lines
"@
            $lines = $outer -split '\r?\n'
            $outerExtent = Get-Extent $outer 1 1 $lines.Count ($lines[2].Length + 1)
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the begin boundary"{
            $innerExtent = Get-Extent $null 1 1 2 2
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the end boundary"{
            $innerExtent = Get-Extent $null 2 8 3 10
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return true if the inner extent is strictly contained in the outer extent around the middle"{
            $innerExtent = Get-Extent $null 1 8 3 2
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return false if the inner extent is partially contained in the outer extent" {
            $innerExtent = Get-Extent $null 3 8 3 26
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $false
        }

        It "Should return false if the inner extent is strictly not contained in the outer extent" {
            $innerExtent = Get-Extent $null 4 25 1 27
            $extensionType::Contains($outerExtent, $innerExtent) | Should Be $false
        }

    }
}
