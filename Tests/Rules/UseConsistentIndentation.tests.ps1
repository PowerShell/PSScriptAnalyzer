# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

BeforeAll {
    $testRootDirectory = Split-Path -Parent $PSScriptRoot
    Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")
}

Describe "UseConsistentIndentation" {
    BeforeAll {
        function Invoke-FormatterAssertion {
            param(
                [string] $ScriptDefinition,
                [string] $ExpectedScriptDefinition,
                [int] $NumberOfExpectedWarnings,
                [hashtable] $Settings
            )

            # Unit test just using this rule only
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $scriptDefinition -Settings $settings
            $violations.Count | Should -Be $NumberOfExpectedWarnings -Because $ScriptDefinition
            Invoke-Formatter -ScriptDefinition $scriptDefinition -Settings $settings | Should -Be $ExpectedScriptDefinition -Because $ScriptDefinition
            # Integration test with all default formatting rules
            Invoke-Formatter -ScriptDefinition $scriptDefinition | Should -Be $ExpectedScriptDefinition -Because $ScriptDefinition
        }
    }
    BeforeEach {
        $indentationUnit = ' '
        $indentationSize = 4
        $ruleConfiguration = @{
            Enable          = $true
            IndentationSize = 4
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            Kind            = 'space'
        }

        $settings = @{
            IncludeRules = @("PSUseConsistentIndentation")
            Rules        = @{
                PSUseConsistentIndentation = $ruleConfiguration
            }
        }
    }

    Context "When top level indentation is not consistent" {
        It "Should detect a violation" {
            $def = @'
 function foo ($param1)
{

}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context "When nested indenation is not consistent" {
        It "Should find a violation" {
            $def = @'
function foo ($param1)
{
"abc"
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }
    }

    Context "When a multi-line hashtable is provided" {
        It "Should find violations" {
            $def = @'
$hashtable = @{
a = 1
b = 2
    c = 3
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 2
        }
    }

    Context "When a multi-line array is provided" {
        It "Should find violations" {
            $def = @'
$array = @(
1,
    2,
3)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 2
        }
    }

    Context "When a param block is provided" {
        It "Should find violations" {
            $def = @'
param(
            [string] $param1,

[string]
    $param2,

        [string]
$param3
)
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 4
        }
    }

    Context 'LParen indentation' {
        It 'Should preserve script when line starts with LParen' {
            $IdempotentScriptDefinition = @'
function test {
    (foo | bar {
        baz
    })
    Do-Something
}
'@
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition | Should -Be $idempotentScriptDefinition
        }

            It 'Should preserve script when line starts and ends with LParen' {
                $IdempotentScriptDefinition = @'
function test {
    (
        foo | bar {
            baz
        }
    )
    Do-Something
}
'@
                Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition | Should -Be $idempotentScriptDefinition
            }

            It 'Should preserve script when line starts and ends with LParen but trailing comment' {
                $IdempotentScriptDefinition = @'
function test {
    ( # comment
        foo | bar {
            baz
        }
    )
    Do-Something
}
'@
                Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition | Should -Be $idempotentScriptDefinition
            }

        It 'Should preserve script when there is Newline after LParen' {
            $IdempotentScriptDefinition = @'
function test {
    $result = (
        Get-Something
    ).Property
    Do-Something
}
'@
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition | Should -Be $idempotentScriptDefinition
        }

    It 'Should preserve script when there is a comment and Newline after LParen' {
        $IdempotentScriptDefinition = @'
function test {
    $result = ( # comment
        Get-Something
    ).Property
    Do-Something
}
'@
        Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition | Should -Be $idempotentScriptDefinition
    }

    It 'Should find violation in script when LParen is first token on a line and is not followed by Newline' {
        $ScriptDefinition = @'
     (foo)
     (bar)
'@
        $ExpectedScriptDefinition = @'
(foo)
(bar)
'@
    Invoke-FormatterAssertion $ScriptDefinition $ExpectedScriptDefinition 2 $settings
    }
}

    Context "When a sub-expression is provided" {
        It "Should not find a violations" {
            $def = @'
function foo {
    $x = $("abc")
    $x
}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }
    }

    Context "When a multi-line command is given" {

        It "When a comment is in the middle of a multi-line statement with preceding and succeeding line continuations" {
            $scriptDefinition = @'
foo `
# comment
-bar `
-baz
'@
            $expected = @'
foo `
    # comment
    -bar `
    -baz
'@
            Invoke-FormatterAssertion $scriptDefinition $expected 3 $settings
        }

        It "When a comment is in the middle of a multi-line statement with preceding pipeline and succeeding line continuation " {
            $scriptDefinition = @'
foo |
# comment
bar `
-baz
'@
            $expected = @'
foo |
    # comment
    bar `
        -baz
'@
            Invoke-FormatterAssertion $scriptDefinition $expected 3 $settings
        }

        It "When a comment is after a pipeline and before the newline " {
            $scriptDefinition = @'
foo | # comment
bar
'@
            $expected = @'
foo | # comment
    bar
'@
            Invoke-FormatterAssertion $scriptDefinition $expected 1 $settings
        }

        It "Should find a violation if a pipleline element is not indented correctly" {
            $def = @'
get-process |
where-object {$_.Name -match 'powershell'}
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
        }

        It "Should not find a violation if a pipleline element is indented correctly" {
            $def = @'
get-process |
    where-object {
        $_.Name -match 'powershell'
    }
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 0
        }

        It "Should ignore comment in the pipleline" {
            $def = @'
  get-process |
    where-object Name -match 'powershell' | # only this is indented correctly
select Name,Id |
       format-list
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 3
        }

        It "Should indent properly after line continuation (backtick) character" {
            $def = @'
$x = "this " + `
"Should be indented properly"
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = "`"Should be indented properly`""
                CorrectionText   = (New-Object -TypeName String -ArgumentList $indentationUnit, $indentationSize) + "`"Should be indented properly`""
            }
            Test-CorrectionExtentFromContent @params
        }

        It "Should indent pipelines correctly using IncreaseIndentationAfterEveryPipeline option" {
            $def = @'
foo |
    bar |
baz
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = "baz"
                CorrectionText   = (New-Object -TypeName String -ArgumentList $indentationUnit, ($indentationSize * 2)) + 'baz'
            }
            Test-CorrectionExtentFromContent @params
        }

        It "Should indent hashtable correctly using <PipelineIndentation> option" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            },
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
            },
            @{
                PipelineIndentation = 'NoIndentation'
            }
            @{
                PipelineIndentation = 'None'
            }
        ) {
            Param([string] $PipelineIndentation)
            $scriptDefinition = @'
@{
        foo = "value1"
    bar = "value2"
}
'@
            $settings = @{
                IncludeRules = @('PSUseConsistentIndentation')
                Rules = @{ PSUseConsistentIndentation = @{ Enable = $true; PipelineIndentation = $PipelineIndentation } }
            }
            Invoke-Formatter -Settings $settings -ScriptDefinition $scriptDefinition | Should -Be @'
@{
    foo = "value1"
    bar = "value2"
}
'@

        }

        It "Should indent pipelines correctly using <PipelineIndentation> option" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                ExpectCorrection    = $true
            },
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                ExpectCorrection    = $true
            },
            @{
                PipelineIndentation = 'NoIndentation'
                ExpectCorrection    = $false
            }
            @{
                PipelineIndentation = 'None'
                ExpectCorrection    = $false
            }
        ) {
            Param([string] $PipelineIndentation, [bool] $ExpectCorrection)
            $def = @'
foo | bar |
baz
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            if ($ExpectCorrection) {
                $violations.Count | Should -Be 1
                $params = @{
                    RawContent       = $def
                    DiagnosticRecord = $violations[0]
                    CorrectionsCount = 1
                    ViolationText    = "baz"
                    CorrectionText   = $indentationUnit * $indentationSize + 'baz'
                }
                Test-CorrectionExtentFromContent @params
            }
            else
            {
                $violations | Should -BeNullOrEmpty
            }
        }

        It 'Should preserve script when using PipelineIndentation None' -TestCases @(
            @{ IdempotentScriptDefinition = @'
foo |
bar
'@
            }
            @{ IdempotentScriptDefinition = @'
foo |
    bar
'@
            },
            @{ IdempotentScriptDefinition = @'
foo |
    bar -Parameter1
'@
            },
            @{ IdempotentScriptDefinition = @'
Get-TransportRule |
Where-Object @{ $_.name -match "a"} |
Select-Object @{ E = $SenderDomainIs | Sort-Object }
Foreach-Object { $_.FullName }
'@
            }
            ) {
        param ($IdempotentScriptDefinition)

        $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = 'None'
        Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
    }

        It 'Should preserve script when using PipelineIndentation IncreaseIndentationAfterEveryPipeline' -TestCases @(
            @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
            @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
            ) {
        param ($PipelineIndentation)
            $IdempotentScriptDefinition = @'
Get-TransportRule |
    Select-Object @{ Key = $SenderDomainIs | Sort-Object }
baz
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should preserve script when using PipelineIndentation <PipelineIndentation>" -TestCases @(
                @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
                @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
                @{ PipelineIndentation = 'NoIndentation' }
                @{ PipelineIndentation = 'None' }
                ) {
            param ($PipelineIndentation)
            $idempotentScriptDefinition = @'
function hello {
    if ($true) {
        "hello" | Out-Host
    }
}
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should preserve script when using PipelineIndentation <PipelineIndentation>" -TestCases @(
            @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
            @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
            @{ PipelineIndentation = 'NoIndentation' }
            @{ PipelineIndentation = 'None' }
            ) {
        param ($PipelineIndentation)
        $idempotentScriptDefinition = @'
function foo {
    function bar {
        Invoke-Something | ForEach-Object {
        }
    }
}
'@
        $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
        Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
    }

    It "Should preserve script when using PipelineIndentation <PipelineIndentation> for multi-line pipeline due to backtick" -TestCases @(
        @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
        @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
        @{ PipelineIndentation = 'NoIndentation' }
        @{ PipelineIndentation = 'None' }
        ) {
    param ($PipelineIndentation)
    $idempotentScriptDefinition = @'
Describe 'describe' {
    It 'it' {
        { 'To be,' -or `
                -not 'to be' } | Should -Be 'the question'
    }
}
'@
    $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
    Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
}
        It "Should preserve script when using PipelineIndentation <PipelineIndentation> for complex multi-line pipeline" -TestCases @(
            @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
            @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
            @{ PipelineIndentation = 'NoIndentation' }
            @{ PipelineIndentation = 'None' }
        ) {
            param ($PipelineIndentation)
            $idempotentScriptDefinition = @'
function foo {
    bar | baz {
        Get-Item
    } | Invoke-Item
    $iShouldStayAtTheSameIndentationLevel
}
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should indent pipelines correctly using NoIndentation option" {
            $def = @'
foo |
    bar |
        baz
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = 'NoIndentation'
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 2
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[1]
                CorrectionsCount = 1
                ViolationText    = "        baz"
                CorrectionText   = (New-Object -TypeName String -ArgumentList $indentationUnit, ($indentationSize * 0)) + 'baz'
            }
            Test-CorrectionExtentFromContent @params
        }

        It "Should indent properly after line continuation (backtick) character with pipeline" {
            $def = @'
foo |
    bar `
| baz
'@
            $violations = Invoke-ScriptAnalyzer -ScriptDefinition $def -Settings $settings
            $violations.Count | Should -Be 1
            $params = @{
                RawContent       = $def
                DiagnosticRecord = $violations[0]
                CorrectionsCount = 1
                ViolationText    = "| baz"
                CorrectionText   = (New-Object -TypeName String -ArgumentList $indentationUnit, $indentationSize) + "| baz"
            }
            Test-CorrectionExtentFromContent @params
        }
    }

    Context "When a nested multi-line pipeline is inside a pipelined script block" {

        It "Should preserve indentation with nested pipeline using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
        Get-Process |
            Select-Object -Last 1
    }
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
        Get-Process |
            Select-Object -Last 1
    }
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
$Test |
ForEach-Object {
    Get-Process |
    Select-Object -Last 1
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
    Get-Process |
            Select-Object -Last 1
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should recover indentation after nested pipeline block using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
function foo {
    $Test |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $thisLineShouldBeAtOneIndent
}
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
function foo {
    $Test |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $thisLineShouldBeAtOneIndent
}
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
function foo {
    $Test |
    ForEach-Object {
        Get-Process |
        Select-Object -Last 1
    }
    $thisLineShouldBeAtOneIndent
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
function foo {
    $Test |
        ForEach-Object {
        Get-Process |
                Select-Object -Last 1
    }
    $thisLineShouldBeAtOneIndent
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should handle multiple sequential nested pipeline blocks using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
function foo {
    $a |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $b |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $stillCorrect
}
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
function foo {
    $a |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $b |
        ForEach-Object {
            Get-Process |
                Select-Object -Last 1
        }
    $stillCorrect
}
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
function foo {
    $a |
    ForEach-Object {
        Get-Process |
        Select-Object -Last 1
    }
    $b |
    ForEach-Object {
        Get-Process |
        Select-Object -Last 1
    }
    $stillCorrect
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
function foo {
    $a |
        ForEach-Object {
        Get-Process |
                Select-Object -Last 1
    }
    $b |
        ForEach-Object {
        Get-Process |
                Select-Object -Last 1
    }
    $stillCorrect
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should handle inner pipeline with 3+ elements using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
        Get-Process |
            Where-Object Path |
            Select-Object -Last 1
    }
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
        Get-Process |
            Where-Object Path |
                Select-Object -Last 1
    }
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
$Test |
ForEach-Object {
    Get-Process |
    Where-Object Path |
    Select-Object -Last 1
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
$Test |
    ForEach-Object {
    Get-Process |
            Where-Object Path |
            Select-Object -Last 1
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should handle outer pipeline on same line as command using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
$Test | ForEach-Object {
    Get-Process |
        Select-Object -Last 1
}
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
$Test | ForEach-Object {
    Get-Process |
        Select-Object -Last 1
}
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
$Test | ForEach-Object {
    Get-Process |
    Select-Object -Last 1
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
$Test | ForEach-Object {
    Get-Process |
        Select-Object -Last 1
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should handle deeply nested pipelines (3 levels) using <PipelineIndentation>" -TestCases @(
            @{
                PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
                IdempotentScriptDefinition = @'
$a |
    ForEach-Object {
        $b |
            ForEach-Object {
                Get-Process |
                    Select-Object -Last 1
            }
    }
'@
            }
            @{
                PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
                IdempotentScriptDefinition = @'
$a |
    ForEach-Object {
        $b |
            ForEach-Object {
                Get-Process |
                    Select-Object -Last 1
            }
    }
'@
            }
            @{
                PipelineIndentation = 'NoIndentation'
                IdempotentScriptDefinition = @'
$a |
ForEach-Object {
    $b |
    ForEach-Object {
        Get-Process |
        Select-Object -Last 1
    }
}
'@
            }
            @{
                PipelineIndentation = 'None'
                IdempotentScriptDefinition = @'
$a |
    ForEach-Object {
    $b |
            ForEach-Object {
        Get-Process |
                    Select-Object -Last 1
    }
}
'@
            }
        ) {
            param ($PipelineIndentation, $IdempotentScriptDefinition)

            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }

        It "Should handle single-line inner pipeline inside multi-line outer pipeline using <PipelineIndentation>" -TestCases @(
            @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
            @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
            @{ PipelineIndentation = 'NoIndentation' }
            @{ PipelineIndentation = 'None' }
        ) {
            param ($PipelineIndentation)

            $idempotentScriptDefinition = @'
$Test | ForEach-Object {
    Get-Process | Select-Object -Last 1
}
'@
            $settings.Rules.PSUseConsistentIndentation.PipelineIndentation = $PipelineIndentation
            Invoke-Formatter -ScriptDefinition $IdempotentScriptDefinition -Settings $settings | Should -Be $IdempotentScriptDefinition
        }
    }

    Context "When multiple openers appear on the same line" {
        It "Should not double-indent for paren-then-brace: .foreach({" {
            $def = @'
@('a', 'b').foreach({
        $_.ToUpper()
    })
'@
            $expected = @'
@('a', 'b').foreach({
    $_.ToUpper()
})
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should not double-indent for brace-then-paren: {(" {
            $def = @'
@('a', 'b').foreach({(
        $_.ToUpper()
    )})
'@
            $expected = @'
@('a', 'b').foreach({(
    $_.ToUpper()
)})
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should not double-indent for array-then-hashtable on same line: @(@{" {
            $idempotentScriptDefinition = @'
$x = @(@{
    key = 'value'
})
'@
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should not double-indent when non-opener tokens separate openers: ([PSCustomObject]@{" {
            $def = @'
$list.Add([PSCustomObject]@{
        Name = "Test"
        Value = 123
    })
'@
            $expected = @'
$list.Add([PSCustomObject]@{
    Name = "Test"
    Value = 123
})
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should indent normally when all openers are closed on the same line" {
            $idempotentScriptDefinition = @'
$list.Add([PSCustomObject]@{Name = "Test"; Value = 123})
'@
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should handle closing brace and paren on separate lines" {
            $def = @'
@('a', 'b').foreach({
            $_.ToUpper()
        }
    )
'@
            $expected = @'
@('a', 'b').foreach({
    $_.ToUpper()
}
)
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should handle nested .foreach({ }) calls" {
            $def = @'
@(1, 2).foreach({
@('a', 'b').foreach({
"$_ and $_"
})
})
'@
            $expected = @'
@(1, 2).foreach({
    @('a', 'b').foreach({
        "$_ and $_"
    })
})
'@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }

        It "Should still indent each opener separately when on different lines" {
            $idempotentScriptDefinition = @'
$x = @(
    @{
        key = 'value'
    }
)
'@
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }

        It "Should still indent normally for sub-expressions" {
            $idempotentScriptDefinition = @'
$(
    Get-Process
)
'@
            Invoke-Formatter -ScriptDefinition $idempotentScriptDefinition -Settings $settings | Should -Be $idempotentScriptDefinition
        }
    }

    Context "When tabs instead of spaces are used for indentation" {
        BeforeEach {
            $settings.Rules.PSUseConsistentIndentation.Kind = 'tab'
        }

        It "Should indent using tabs" {
            $def = @'
function foo
{
get-childitem
$x=1+2
$hashtable = @{
property1 = "value"

'@ + "`t" + @'
anotherProperty = "another value"
}
}
'@
            ${t} = "`t"
            $expected = @"
function foo
{
${t}get-childitem
${t}`$x=1+2
${t}`$hashtable = @{
${t}${t}property1 = "value"
${t}${t}anotherProperty = "another value"
${t}}
}
"@
            Invoke-Formatter -ScriptDefinition $def -Settings $settings | Should -Be $expected
        }
    }
}
