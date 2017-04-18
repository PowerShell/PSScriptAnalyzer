Import-Module PSScriptAnalyzer

Describe "IScriptExtent Extension Methods" {
    Context "When single line inner extent is compared with a single line outer extent" {
        BeforeAll {
            $outer = "This is the outer string"
            $scriptPositionType = 'System.Management.Automation.Language.ScriptPosition'
            $scriptExtentType = 'System.Management.Automation.Language.ScriptExtent'

            $extentGetter = {
                param($line, $startLineNum, $startColumnNum, $endLineNum, $endColumnNum)
                $extentStartPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $startLineNum, $startColumnNum, $line
                $extentEndPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $endLineNum, $endColumnNum, $line
                New-Object -TypeName $scriptExtentType -ArgumentList $extentStartPos, $extentEndPos
            }

            $outerExtent = & $extentGetter $outer 1 1 1 ($outer.Length + 1)
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the begin boundary" {
            $innerExtent = & $extentGetter $outer 1 1 1 2
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return true if the inner extent is strictly contained in the outer extent on the end boundary" {
            $innerExtent = & $extentGetter $outer 1 23 1 25
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]::Contains($outerExtent, $innerExtent) | Should Be $true
        }

        It "Should return false if the inner extent is partially contained in the outer extent" {
            $innerExtent = & $extentGetter $outer 1 23 1 26
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]::Contains($outerExtent, $innerExtent) | Should Be $false
        }

        It "Should return false if the inner extent is strictly not contained in the outer extent" {
            $innerExtent = & $extentGetter $outer 1 25 1 27
            [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]::Contains($outerExtent, $innerExtent) | Should Be $false
        }
    }
}
