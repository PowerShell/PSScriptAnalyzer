$directory = Split-Path -Parent $MyInvocation.MyCommand.Path
$testRootDirectory = Split-Path -Parent $directory

Import-Module (Join-Path $testRootDirectory "PSScriptAnalyzerTestHelper.psm1")

Describe "UseConsistentIndentation" {
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

        It "Should preserve script when using PipelineIndentation <PipelineIndentation>" -TestCases @(
                @{ PipelineIndentation = 'IncreaseIndentationForFirstPipeline' }
                @{ PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline' }
                @{ PipelineIndentation = 'NoIndentation' }
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
