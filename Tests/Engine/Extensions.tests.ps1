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
