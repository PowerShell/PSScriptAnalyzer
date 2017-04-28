Import-Module PSScriptAnalyzer

function Get-Extent {
    param($line, $startLineNum, $startColumnNum, $endLineNum, $endColumnNum)
    $scriptPositionType = 'System.Management.Automation.Language.ScriptPosition'
    $scriptExtentType = 'System.Management.Automation.Language.ScriptExtent'
    $extentStartPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $startLineNum, $startColumnNum, $line
    $extentEndPos = New-Object -TypeName $scriptPositionType -ArgumentList $null, $endLineNum, $endColumnNum, $line
    New-Object -TypeName $scriptExtentType -ArgumentList $extentStartPos, $extentEndPos
}

function Test-Extent {
    param(
        $translatedExtent,
        $expectedStartLineNumber,
        $expectedStartColumnNumber,
        $expectedEndLineNumber,
        $expectedEndColumnNumber)

    $translatedExtent.StartLineNumber | Should Be $expectedStartLineNumber
    $translatedExtent.StartColumnNumber | Should Be $expectedStartColumnNumber
    $translatedExtent.EndLineNumber | Should Be $expectedEndLineNumber
    $translatedExtent.EndColumnNumber | Should Be $expectedEndColumnNumber
}

$extNamespace = [Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions.Extensions]
Describe "IScriptExtent Translate Extension" {
    Context "When a delta is given" {
        It "Should translate the line numbers for valid positive line delta" {
            $extent = Get-Extent $null 1 1 2 2
            $translatedExtent = $extNamespace::Translate($extent, 1, 0)
            Test-Extent $translatedExtent 2 1 3 2
        }

        It "Should translate the column numbers for valid positive column delta" {
            $extent = Get-Extent $null 1 1 2 2
            $translatedExtent = $extNamespace::Translate($extent, 0, 1)
            Test-Extent $translatedExtent 1 2 2 3
        }

        It "Should translate the line and column numbers for valid positive line/column delta" {
            $extent = Get-Extent $null 1 1 2 2
            $translatedExtent = $extNamespace::Translate($extent, 1, 1)
            Test-Extent $translatedExtent 2 2 3 3
        }

        It "Should throw if translated start line number is less than 1" {
            $extent = Get-Extent $null 1 1 2 2
            {$extNamespace::Translate($extent, -1, 0)} | Should Throw
        }

        It "Should throw if translated start column number is less than 1" {
            $extent = Get-Extent $null 1 1 2 2
            {$extNamespace::Translate($extent, 0, -2)} | Should Throw
        }

        It "Should throw if translated end column number is less than 1" {
            $extent = Get-Extent $null 1 1 2 1
            {$extNamespace::Translate($extent, 0, -1)} | Should Throw
        }
    }
}
