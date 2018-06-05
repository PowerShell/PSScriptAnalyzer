$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

$ruleName = "PSAvoidTrailingWhitespace"

$settings = @{
    IncludeRules = @($ruleName)
}

Describe "AvoidTrailingWhitespace" {
    $testCases = @(
        @{
            Type       = 'spaces'
            Whitespace = '     '
        }

        @{
            Type       = 'tabs'
            Whitespace = "`t`t`t"
        }
    )

    It 'Should find a violation when a line contains trailing <Type>' -TestCases $testCases {
        param (
            [string] $Whitespace
        )

        $def = "`$null = `$null$Whitespace"
        $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
        Test-CorrectionExtentFromContent $def $violations 1 $Whitespace ''
    }
}
